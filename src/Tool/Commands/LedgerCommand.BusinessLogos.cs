using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class LedgerCommand
    {
        private static async Task HandleBusinessLogoCreateAsync(long businessId, FileInfo logoFileInfo, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            byte[] data = await File.ReadAllBytesAsync(logoFileInfo.FullName);
            string contentType = logoFileInfo.Extension.ToLowerInvariant() switch
            {
                ".gif" => "image/gif",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                var ext => throw new InvalidOperationException($"Unsupported file extension [{ext}]. Must be one of: .gif, .jpg, .jpeg, .png, .webp"),
            };

            Contracts.Ledger.BusinessLogo logo = new()
            {
                BusinessId = businessId,
                ContentType = contentType,
                Data = data,
            };

            logo = await client.Ledger.CreateBusinessLogoAsync(businessId, logo);
            Console.WriteLine(new { logo.BusinessId, logo.ContentType, DataLength = logo.Data?.Length, logo.CreatedDate, logo.ModifiedDate }.ToJsonString(indented: true));
        }

        private static async Task HandleBusinessLogoUpdateAsync(long businessId, FileInfo logoFileInfo, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            byte[] data = await File.ReadAllBytesAsync(logoFileInfo.FullName);
            string contentType = logoFileInfo.Extension.ToLowerInvariant() switch
            {
                ".gif" => "image/gif",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                var ext => throw new InvalidOperationException($"Unsupported file extension [{ext}]. Must be one of: .gif, .jpg, .jpeg, .png, .webp"),
            };

            Contracts.Ledger.BusinessLogo logo = new()
            {
                BusinessId = businessId,
                ContentType = contentType,
                Data = data,
            };

            logo = await client.Ledger.UpdateBusinessLogoAsync(businessId, logo);
            Console.WriteLine(new { logo.BusinessId, logo.ContentType, DataLength = logo.Data?.Length, logo.CreatedDate, logo.ModifiedDate }.ToJsonString(indented: true));
        }

        private static async Task HandleBusinessLogoGetAsync(long businessId, FileInfo outputFileInfo, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Ledger.BusinessLogo logo = await client.Ledger.GetBusinessLogoAsync(businessId);
            await File.WriteAllBytesAsync(outputFileInfo.FullName, logo.Data);
            Console.WriteLine($"Written {logo.Data.Length:N0} bytes ({logo.ContentType}) to {outputFileInfo.FullName}");
        }

        private static async Task HandleBusinessLogoDeleteAsync(long businessId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            await client.Ledger.DeleteBusinessLogoAsync(businessId);
        }
    }
}
