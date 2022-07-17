using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace number_sequence.DataAccess
{
    public class GoogleSheetDataAccess
    {
        private readonly Options.Google options;
        private readonly ILogger<GoogleSheetDataAccess> logger;

        public GoogleSheetDataAccess(IOptions<Options.Google> googleOptions, ILogger<GoogleSheetDataAccess> logger)
        {
            this.options = googleOptions.Value;
            this.logger = logger;
        }

        private SheetsService service;

        private bool inited = false;
        private void EnsureInit()
        {
            if (!this.inited)
            {
                using var credentialStream = new MemoryStream(Encoding.UTF8.GetBytes(this.options.Credentials));
                credentialStream.Position = 0;
                var serviceAccountCredential = ServiceAccountCredential.FromServiceAccountData(credentialStream);

                this.service = new SheetsService(
                    new BaseClientService.Initializer
                    {
                        HttpClientInitializer = serviceAccountCredential,
                        ApplicationName = "number-sequence"
                    });

                this.inited = true;
            }
        }

        public async Task GetAsync(string spreadsheetId, string range, CancellationToken cancellationToken)
        {
            this.EnsureInit();

            SpreadsheetsResource.ValuesResource.GetRequest request = this.service.Spreadsheets.Values.Get(spreadsheetId, range);
            ValueRange response = await request.ExecuteAsync(cancellationToken);
        }
    }
}
