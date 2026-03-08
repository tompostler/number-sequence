using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Controllers
{
    public sealed partial class LedgerController
    {
        [HttpPost("businesses/{id}/logo")]
        public async Task<IActionResult> CreateBusinessLogoAsync(long id, [FromBody] BusinessLogo logo, CancellationToken cancellationToken)
        {
            logo.BusinessId = id;

            string[] allowedContentTypes = ["image/gif", "image/jpeg", "image/png", "image/webp"];
            if (!allowedContentTypes.Contains(logo.ContentType))
            {
                return this.BadRequest($"Content type [{logo.ContentType}] is not allowed. Must be one of: {string.Join(", ", allowedContentTypes)}.");
            }

            const int maxBytes = 64 * 1024;
            if (logo.Data?.Length > maxBytes)
            {
                return this.BadRequest($"Image size [{logo.Data.Length:N0} bytes] exceeds maximum allowed [{maxBytes:N0} bytes].");
            }

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            bool businessExists = await nsContext.InvoiceBusinesses
                .AnyAsync(x => x.AccountName == this.User.Identity.Name && x.Id == id, cancellationToken);
            if (!businessExists)
            {
                return this.NotFound();
            }

            bool logoExists = await nsContext.InvoiceBusinessLogos
                .AnyAsync(x => x.BusinessId == id, cancellationToken);
            if (logoExists)
            {
                return this.Conflict();
            }

            _ = nsContext.InvoiceBusinessLogos.Add(logo);
            _ = await nsContext.SaveChangesAsync(cancellationToken);
            return this.Ok(logo);
        }

        [HttpPut("businesses/{id}/logo")]
        public async Task<IActionResult> UpdateBusinessLogoAsync(long id, [FromBody] BusinessLogo logo, CancellationToken cancellationToken)
        {
            string[] allowedContentTypes = ["image/gif", "image/jpeg", "image/png", "image/webp"];
            if (!allowedContentTypes.Contains(logo.ContentType))
            {
                return this.BadRequest($"Content type [{logo.ContentType}] is not allowed. Must be one of: {string.Join(", ", allowedContentTypes)}.");
            }

            const int maxBytes = 64 * 1024;
            if (logo.Data?.Length > maxBytes)
            {
                return this.BadRequest($"Image size [{logo.Data.Length:N0} bytes] exceeds maximum allowed [{maxBytes:N0} bytes].");
            }

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            bool businessExists = await nsContext.InvoiceBusinesses
                .AnyAsync(x => x.AccountName == this.User.Identity.Name && x.Id == id, cancellationToken);
            if (!businessExists)
            {
                return this.NotFound();
            }

            BusinessLogo logoRecord = await nsContext.InvoiceBusinessLogos
                .SingleOrDefaultAsync(x => x.BusinessId == id, cancellationToken);
            if (logoRecord == default)
            {
                return this.NotFound();
            }

            logoRecord.ContentType = logo.ContentType;
            logoRecord.Data = logo.Data;
            logoRecord.ModifiedDate = DateTimeOffset.UtcNow;

            _ = await nsContext.SaveChangesAsync(cancellationToken);
            return this.Ok(logoRecord);
        }

        [HttpGet("businesses/{id}/logo")]
        public async Task<IActionResult> GetBusinessLogoAsync(long id, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            bool businessExists = await nsContext.InvoiceBusinesses
                .AnyAsync(x => x.AccountName == this.User.Identity.Name && x.Id == id, cancellationToken);
            if (!businessExists)
            {
                return this.NotFound();
            }

            BusinessLogo logo = await nsContext.InvoiceBusinessLogos
                .SingleOrDefaultAsync(x => x.BusinessId == id, cancellationToken);
            if (logo == default)
            {
                return this.NotFound();
            }

            return this.Ok(logo);
        }

        [HttpDelete("businesses/{id}/logo")]
        public async Task<IActionResult> DeleteBusinessLogoAsync(long id, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            bool businessExists = await nsContext.InvoiceBusinesses
                .AnyAsync(x => x.AccountName == this.User.Identity.Name && x.Id == id, cancellationToken);
            if (!businessExists)
            {
                return this.NotFound();
            }

            BusinessLogo logo = await nsContext.InvoiceBusinessLogos
                .SingleOrDefaultAsync(x => x.BusinessId == id, cancellationToken);
            if (logo == default)
            {
                return this.NotFound();
            }

            _ = nsContext.InvoiceBusinessLogos.Remove(logo);
            _ = await nsContext.SaveChangesAsync(cancellationToken);
            return this.Ok();
        }
    }
}
