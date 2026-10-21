using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Pages.UI.Libraries
{
    [RequiresToken(AccountRoles.Library)]
    public sealed class LocationsModel : LibraryPageModel
    {
        public LocationsModel(NsTcpWtfClient nsClient) : base(nsClient) { }

        /// <summary>
        /// Sorted by path, so each location follows its parent and the list reads as a tree.
        /// </summary>
        public List<LibraryLocation> Locations { get; private set; }

        public Dictionary<long, int> Depths { get; private set; }

        public async Task<IActionResult> OnGetAsync(long libraryId, CancellationToken cancellationToken)
        {
            if (await this.LoadLibraryAsync(libraryId, cancellationToken) is IActionResult notFound)
            {
                return notFound;
            }

            this.Locations = await this.NsClient.Library.GetLocationsAsync(libraryId, cancellationToken);
            Dictionary<long, LibraryLocation> byId = this.Locations.ToDictionary(x => x.Id);
            this.Depths = this.Locations.ToDictionary(x => x.Id, x =>
            {
                int depth = 0;
                for (long? parent = x.ParentLocationId; parent.HasValue && depth < byId.Count; parent = byId.GetValueOrDefault(parent.Value)?.ParentLocationId)
                {
                    depth++;
                }
                return depth;
            });
            return this.Page();
        }

        public async Task<IActionResult> OnPostCreateAsync(long libraryId, string name, long? parentLocationId, string notes, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                this.ErrorMessage = "A location needs a name.";
                return this.Redirect($"/ui/library/{libraryId}/locations");
            }

            LibraryLocation location = new() { Name = name.Trim(), ParentLocationId = parentLocationId, Notes = notes?.Trim() };
            return await this.RunAndRedirectAsync(
                () => this.NsClient.Library.CreateLocationAsync(libraryId, location, cancellationToken),
                $"Added {location.Name}.",
                $"/ui/library/{libraryId}/locations");
        }

        public async Task<IActionResult> OnPostUpdateAsync(long libraryId, long locationId, string name, long? parentLocationId, string notes, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                this.ErrorMessage = "A location needs a name.";
                return this.Redirect($"/ui/library/{libraryId}/locations");
            }

            LibraryLocation location = new() { Id = locationId, Name = name.Trim(), ParentLocationId = parentLocationId, Notes = notes?.Trim() };
            return await this.RunAndRedirectAsync(
                () => this.NsClient.Library.UpdateLocationAsync(libraryId, location, cancellationToken),
                $"Saved {location.Name}.",
                $"/ui/library/{libraryId}/locations");
        }

        public async Task<IActionResult> OnPostDeleteAsync(long libraryId, long locationId, CancellationToken cancellationToken)
            => await this.RunAndRedirectAsync(
                () => this.NsClient.Library.DeleteLocationAsync(libraryId, locationId, cancellationToken),
                "Location deleted.",
                $"/ui/library/{libraryId}/locations");
    }
}
