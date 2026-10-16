using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Exceptions;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Controllers
{
    public sealed partial class LibraryController
    {
        [HttpGet("libraries/{libraryId}/locations")]
        public async Task<IActionResult> GetLocationsAsync(long libraryId, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Viewer, cancellationToken);
            List<LibraryLocation> locations = await nsContext.LibraryLocations
                .AsNoTracking()
                .Where(x => x.LibraryId == libraryId)
                .ToListAsync(cancellationToken);

            Dictionary<long, string> paths = BuildLocationPaths(locations, " › ");
            foreach (LibraryLocation location in locations)
            {
                location.Path = paths[location.Id];
            }
            return this.Ok(locations.OrderBy(x => x.Path, StringComparer.CurrentCultureIgnoreCase).ToList());
        }

        [HttpPost("libraries/{libraryId}/locations")]
        public async Task<IActionResult> CreateLocationAsync(long libraryId, [FromBody] LibraryLocation location, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            await EnsureLocationInLibraryAsync(nsContext, libraryId, location.ParentLocationId, cancellationToken);

            LibraryLocation toInsert = new()
            {
                LibraryId = libraryId,
                ParentLocationId = location.ParentLocationId,
                Name = location.Name,
                Notes = location.Notes,
            };
            _ = nsContext.LibraryLocations.Add(toInsert);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation($"[{this.AccountName}] created location [{toInsert.Id}] [{toInsert.Name}] under [{toInsert.ParentLocationId}] in library [{libraryId}].");
            return this.Ok(toInsert);
        }

        [HttpPut("libraries/{libraryId}/locations")]
        public async Task<IActionResult> UpdateLocationAsync(long libraryId, [FromBody] LibraryLocation location, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            List<LibraryLocation> locations = await nsContext.LibraryLocations.Where(x => x.LibraryId == libraryId).ToListAsync(cancellationToken);
            LibraryLocation locationRecord = locations.SingleOrDefault(x => x.Id == location.Id);
            if (locationRecord == default)
            {
                return this.NotFound($"Location [{location.Id}] not found in library [{libraryId}].");
            }

            // A new parent must be in this library and must not be the location itself or anything inside it.
            if (location.ParentLocationId.HasValue)
            {
                Dictionary<long, LibraryLocation> byId = locations.ToDictionary(x => x.Id);
                if (!byId.ContainsKey(location.ParentLocationId.Value))
                {
                    return this.BadRequest($"Location [{location.ParentLocationId}] is not in library [{libraryId}].");
                }
                int hops = 0;
                for (long? ancestor = location.ParentLocationId; ancestor.HasValue && hops++ <= byId.Count; ancestor = byId.GetValueOrDefault(ancestor.Value)?.ParentLocationId)
                {
                    if (ancestor == locationRecord.Id)
                    {
                        return this.BadRequest($"Location [{locationRecord.Id}] can't be moved inside itself.");
                    }
                }
            }

            locationRecord.ParentLocationId = location.ParentLocationId;
            locationRecord.Name = location.Name;
            locationRecord.Notes = location.Notes;
            locationRecord.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation($"[{this.AccountName}] updated location [{locationRecord.Id}] [{locationRecord.Name}] under [{locationRecord.ParentLocationId}] in library [{libraryId}].");
            return this.Ok(locationRecord);
        }

        /// <summary>
        /// Refused while copies or child locations are still in it, so nothing silently ends up with no location. Scan
        /// sessions at the location go with it: they are a tool for reconciling the location, not history of the copies,
        /// and keeping them would make any location that was ever scanned impossible to delete.
        /// </summary>
        [HttpDelete("libraries/{libraryId}/locations/{locationId}")]
        public async Task<IActionResult> DeleteLocationAsync(long libraryId, long locationId, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            if (!await nsContext.LibraryLocations.AnyAsync(x => x.Id == locationId && x.LibraryId == libraryId, cancellationToken))
            {
                return this.NotFound($"Location [{locationId}] not found in library [{libraryId}].");
            }

            int copies = await nsContext.LibraryCopies.CountAsync(x => x.LocationId == locationId, cancellationToken);
            int children = await nsContext.LibraryLocations.CountAsync(x => x.ParentLocationId == locationId, cancellationToken);
            if (copies > 0 || children > 0)
            {
                return this.Conflict($"Location [{locationId}] still holds {copies} copies and {children} locations. Move them first.");
            }

            int sessions = 0;
            await InBulkTransactionAsync(
                nsContext,
                async ct =>
                {
                    sessions = await nsContext.LibraryScanSessions.Where(x => x.LocationId == locationId).ExecuteDeleteAsync(ct);
                    _ = await nsContext.LibraryLocations.Where(x => x.Id == locationId).ExecuteDeleteAsync(ct);
                },
                cancellationToken);

            this.logger.LogInformation($"[{this.AccountName}] deleted location [{locationId}] in library [{libraryId}] with {sessions} scan sessions.");
            return this.NoContent();
        }

        /// <summary>
        /// Each location's full name from the outermost location down, joined by <paramref name="separator"/>. A parent that
        /// can't be found (or a cycle, which updates refuse to create) ends the walk rather than throwing.
        /// </summary>
        private static Dictionary<long, string> BuildLocationPaths(IReadOnlyCollection<LibraryLocation> locations, string separator)
        {
            Dictionary<long, LibraryLocation> byId = locations.ToDictionary(x => x.Id);
            Dictionary<long, string> paths = [];
            foreach (LibraryLocation location in locations)
            {
                List<string> names = [];
                HashSet<long> seen = [];
                for (LibraryLocation current = location; current != null && seen.Add(current.Id);
                    current = current.ParentLocationId.HasValue ? byId.GetValueOrDefault(current.ParentLocationId.Value) : null)
                {
                    names.Add(current.Name);
                }
                names.Reverse();
                paths[location.Id] = string.Join(separator, names);
            }
            return paths;
        }

        /// <summary>
        /// <paramref name="locationId"/> and every location inside it, for filters where "Office" should include its shelves.
        /// </summary>
        private static async Task<List<long>> GetLocationAndDescendantIdsAsync(NsContext nsContext, long libraryId, long locationId, CancellationToken cancellationToken)
        {
            var locations = await nsContext.LibraryLocations
                .AsNoTracking()
                .Where(x => x.LibraryId == libraryId)
                .Select(x => new { x.Id, x.ParentLocationId })
                .ToListAsync(cancellationToken);
            if (!locations.Any(x => x.Id == locationId))
            {
                throw new BadRequestException($"Location [{locationId}] is not in library [{libraryId}].");
            }

            ILookup<long?, long> childrenByParent = locations.ToLookup(x => x.ParentLocationId, x => x.Id);
            List<long> result = [];
            Queue<long> pending = new([locationId]);
            HashSet<long> seen = [];
            while (pending.TryDequeue(out long current))
            {
                if (seen.Add(current))
                {
                    result.Add(current);
                    foreach (long child in childrenByParent[current])
                    {
                        pending.Enqueue(child);
                    }
                }
            }
            return result;
        }
    }
}
