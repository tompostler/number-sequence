using Azure.Storage.Blobs;
using Cronos;
using DurableTask.Core;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using number_sequence.DataAccess;
using number_sequence.Models;
using number_sequence.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace number_sequence.Services.Background.LatexGeneration
{
    public sealed class ChiroEquineLatexGenerationBackgroundService : SqlSynchronizedBackgroundService
    {
        private readonly GoogleSheetDataAccess googleSheetDataAccess;
        private readonly NsStorage nsStorage;

        public ChiroEquineLatexGenerationBackgroundService(
            GoogleSheetDataAccess googleSheetDataAccess,
            NsStorage nsStorage,
            IOptions<Options.Google> googleOptions,
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<ChiroEquineLatexGenerationBackgroundService> logger,
            TelemetryClient telemetryClient)
            : base(serviceProvider, sentinals, logger, telemetryClient)
        {
            this.googleSheetDataAccess = googleSheetDataAccess;
            this.nsStorage = nsStorage;
        }

        protected override CronExpression Cron => CronExpression.Parse("*/12 7-23 * * *");

        protected override async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            // Get the template information
            LatexTemplate template = await nsContext.LatexTemplates.FirstOrDefaultAsync(x => x.Id == NsStorage.C.LTBP.ChiroEquine, cancellationToken);
            if (template == default)
            {
                this.logger.LogInformation("No template defined.");
                return;
            }

            // Skip the number of rows that we already know about
            int numberOfKnownRows = await nsContext.LatexTemplateSpreadsheetRows
                .CountAsync(x => x.SpreadsheetId == template.SpreadsheetId, cancellationToken);
            string spreadsheetRange = template.SpreadsheetRange.Replace("1", Math.Max(1, numberOfKnownRows).ToString());
            this.logger.LogInformation($"Skipping {numberOfKnownRows} known rows and querying range {spreadsheetRange}");

            // Get the data from the spreadsheet. The first row is the headers
            IList<IList<object>> data = await this.googleSheetDataAccess.GetAsync(template.SpreadsheetId, spreadsheetRange, cancellationToken);
            IList<object> headers = data[0];
            data = data.Skip(1).ToList();

            // Only on reset or initial deployment, no data
            if (!data.Any())
            {
                this.logger.LogInformation("No rows of data.");
                return;
            }

            // Check each row of data to see if it's already been processed
            // Only process one additional row at a time
            string[] row = default;
            LatexTemplateSpreadsheetRow latexTemplateRow = default;
            bool newWork = false;
            for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
            {
                row = data[rowIndex].Select(x => x as string).ToArray();
                string id = string.Join('|', row).ComputeSHA256();
                latexTemplateRow = await nsContext.LatexTemplateSpreadsheetRows
                    .SingleOrDefaultAsync(x => x.SpreadsheetId == template.SpreadsheetId && x.RowId == id, cancellationToken);

                if (latexTemplateRow != default)
                {
                    this.logger.LogInformation($"Data row {id} ({rowIndex + numberOfKnownRows + 1}) was inserted for processing at {latexTemplateRow.CreatedDate:u} and processed {latexTemplateRow.ProcessedAt:u}");
                    continue;
                }
                else
                {
                    this.logger.LogInformation($"Data row {id} ({rowIndex + numberOfKnownRows + 1}) is new. Setting up for processing.");
                    latexTemplateRow = new()
                    {
                        SpreadsheetId = template.SpreadsheetId,
                        RowId = id,
                        LatexDocumentId = id.MakeHumanFriendly() + '_' + template.Id,
                        ProcessedAt = DateTimeOffset.UtcNow
                    };
                    _ = nsContext.LatexTemplateSpreadsheetRows.Add(latexTemplateRow);
                    newWork = true;
                    break;
                }
            }
            if (!newWork)
            {
                this.logger.LogInformation("No new work.");
                return;
            }

            // Create the new records for generating the document
            LatexDocument latexDocument = new()
            {
                Id = latexTemplateRow.LatexDocumentId
            };
            _ = nsContext.LatexDocuments.Add(latexDocument);

            // Copy the template blob(s) to the working directory
            BlobClient templateLatexBlob = default;
            await foreach (BlobClient templateBlob in this.nsStorage.EnumerateAllBlobsForLatexTemplateAsync(template.Id, cancellationToken))
            {
                string targetPath = $"{NsStorage.C.LBP.Input}/{templateBlob.Name.Substring((template.Id + "/").Length).Replace("template", latexDocument.Id)}";
                BlobClient targetBlob = this.nsStorage.GetBlobClientForLatexJob(latexDocument.Id, targetPath);
                this.logger.LogInformation($"Copying {templateBlob.Uri} to {targetBlob.Uri}");
                _ = await targetBlob.SyncCopyFromUriAsync(
                    templateBlob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1)),
                    cancellationToken: cancellationToken);

                if (templateBlob.Name.EndsWith("template.tex"))
                {
                    templateLatexBlob = targetBlob;
                }
            }

            string customAppend(string existing, string prefix, int index)
            {
                if (index >= row.Length)
                {
                    return string.Empty;
                }
                else if (!string.IsNullOrWhiteSpace(row[index]))
                {
                    return (string.IsNullOrWhiteSpace(existing) ? string.Empty : ", ")
                        + $"{prefix} {row[index]}".Trim().EscapeForLatex();
                }
                else
                {
                    return string.Empty;
                }
            }


            // Parse the data row into meaningful replacement values
            //  0  A Submission timestamp

            // Intake info
            //  1  B Patient Name
            //  2  C Owner Name
            //  3  D Date of Service
            //  4  E CC email(s)
            string patientName = row[1]?.Trim();
            string ownerName = row[2]?.Trim();
            var dateOfService = DateTimeOffset.Parse(row[3]);
            string[] ccEmail = row[4].Split(new[] { ',', ';' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            // Head
            //  5  F Other notes
            //  6  G Occiput
            //  7  H TMJ
            string head = string.Empty;
            head += customAppend(head, "Occiput", 6);
            head += customAppend(head, "TMJ", 7);
            head += customAppend(head, string.Empty, 5);

            // Cervical
            //  8  I Other Notes
            //  9  J C1
            // 10  K C2
            // 11  L C3
            // 12  M C4
            // 13  N C5
            // 14  O C6
            // 15  P C7
            string cervical = string.Empty;
            cervical += customAppend(cervical, "C1", 9);
            cervical += customAppend(cervical, "C2", 10);
            cervical += customAppend(cervical, "C3", 11);
            cervical += customAppend(cervical, "C4", 12);
            cervical += customAppend(cervical, "C5", 13);
            cervical += customAppend(cervical, "C6", 14);
            cervical += customAppend(cervical, "C7", 15);
            cervical += customAppend(cervical, string.Empty, 8);

            // Thoracic
            // 16  Q Other Notes
            // 17  R T1
            // 18  S T2
            // 19  T T3
            // 20  U T4
            // 21  V T5
            // 22  W T6
            // 23  X T7
            // 24  Y T8
            // 25  Z T9
            // 26 AA T10
            // 27 AB T11
            // 28 AC T12
            // 29 AD T13
            // 30 AE T14
            // 31 AF T15
            // 32 AG T16
            // 33 AH T17
            // 34 AI T18
            // 89 CL Sternum
            string thoracic = string.Empty;
            thoracic += customAppend(thoracic, "T1", 17);
            thoracic += customAppend(thoracic, "T2", 18);
            thoracic += customAppend(thoracic, "T3", 19);
            thoracic += customAppend(thoracic, "T4", 20);
            thoracic += customAppend(thoracic, "T5", 21);
            thoracic += customAppend(thoracic, "T6", 22);
            thoracic += customAppend(thoracic, "T7", 23);
            thoracic += customAppend(thoracic, "T8", 24);
            thoracic += customAppend(thoracic, "T9", 25);
            thoracic += customAppend(thoracic, "T10", 26);
            thoracic += customAppend(thoracic, "T11", 27);
            thoracic += customAppend(thoracic, "T12", 28);
            thoracic += customAppend(thoracic, "T13", 29);
            thoracic += customAppend(thoracic, "T14", 30);
            thoracic += customAppend(thoracic, "T15", 31);
            thoracic += customAppend(thoracic, "T16", 32);
            thoracic += customAppend(thoracic, "T17", 33);
            thoracic += customAppend(thoracic, "T18", 34);
            thoracic += customAppend(thoracic, "Sternum", 89);
            thoracic += customAppend(thoracic, string.Empty, 16);

            // Ribs
            // 35 AJ R1
            // 36 AK R2
            // 37 AL R3
            // 38 AM R4
            // 39 AN R5
            // 40 AO R6
            // 41 AP R7
            // 42 AQ R8
            // 43 AR R9
            // 44 AS R10
            // 45 AT R11
            // 46 AU R12
            // 47 AV R13
            // 48 AW R14
            // 49 AX R15
            // 50 AY R16
            // 51 AZ R17
            // 52 BA R18
            string ribs = string.Empty;
            ribs += customAppend(ribs, "R1", 35);
            ribs += customAppend(ribs, "R2", 36);
            ribs += customAppend(ribs, "R3", 37);
            ribs += customAppend(ribs, "R4", 38);
            ribs += customAppend(ribs, "R5", 39);
            ribs += customAppend(ribs, "R6", 40);
            ribs += customAppend(ribs, "R7", 41);
            ribs += customAppend(ribs, "R8", 42);
            ribs += customAppend(ribs, "R9", 43);
            ribs += customAppend(ribs, "R10", 44);
            ribs += customAppend(ribs, "R11", 45);
            ribs += customAppend(ribs, "R12", 46);
            ribs += customAppend(ribs, "R13", 47);
            ribs += customAppend(ribs, "R14", 48);
            ribs += customAppend(ribs, "R15", 49);
            ribs += customAppend(ribs, "R16", 50);
            ribs += customAppend(ribs, "R17", 51);
            ribs += customAppend(ribs, "R18", 52);

            // Lumbar
            // 53 BB Other notes
            // 54 BC L1
            // 55 BD L2
            // 56 BE L3
            // 57 BF L4
            // 58 BG L5
            // 59 BH L6
            // 60 BI L3/L4 Intertransverse
            // 61 BJ L4/L5 Intertransverse
            // 62 BK L5/L6 Intertransverse
            string lumbar = string.Empty;
            lumbar += customAppend(lumbar, "L1", 54);
            lumbar += customAppend(lumbar, "L2", 55);
            lumbar += customAppend(lumbar, "L3", 56);
            lumbar += customAppend(lumbar, "L4", 57);
            lumbar += customAppend(lumbar, "L5", 58);
            lumbar += customAppend(lumbar, "L6", 59);
            lumbar += customAppend(lumbar, "L3/L4 Intertransverse", 60);
            lumbar += customAppend(lumbar, "L4/L5 Intertransverse", 61);
            lumbar += customAppend(lumbar, "L5/L6 Intertransverse", 62);
            lumbar += customAppend(lumbar, string.Empty, 53);

            // Sacrum
            // 63 BL Other notes
            // 64 BM Base
            // 65 BN Apex
            // Pelvic
            // 66 BO Other notes
            // 67 BP Left
            // 68 BQ Right
            // 90 CM Traction
            string pelvicSacral = string.Empty;
            pelvicSacral += customAppend(pelvicSacral, "Base", 64);
            pelvicSacral += customAppend(pelvicSacral, "Apex", 65);
            pelvicSacral += customAppend(pelvicSacral, "Sacrum", 63);
            pelvicSacral += customAppend(pelvicSacral, "Left", 67);
            pelvicSacral += customAppend(pelvicSacral, "Right", 68);
            pelvicSacral += customAppend(pelvicSacral, string.Empty, 90);
            pelvicSacral += customAppend(pelvicSacral, "Pelvis", 66);

            // Left forelimb
            // 69 BR Other notes
            // 70 BS Scapula
            // 71 BT Humorous
            // 72 BU Ulna
            // 73 BV Radius
            // 74 BW Carpus
            // 75 BX Metatarsals/Phalanges
            string leftForelimb = string.Empty;
            leftForelimb += customAppend(leftForelimb, "Scapula", 70);
            leftForelimb += customAppend(leftForelimb, "Humorous", 71);
            leftForelimb += customAppend(leftForelimb, "Ulna", 72);
            leftForelimb += customAppend(leftForelimb, "Radius", 73);
            leftForelimb += customAppend(leftForelimb, "Carpus", 74);
            leftForelimb += customAppend(leftForelimb, "Metatarsals/Phalanges", 75);
            leftForelimb += customAppend(leftForelimb, string.Empty, 69);

            // Right forelimb
            // 76 BY Other notes
            // 77 BZ Scapula
            // 78 CA Humorous
            // 79 CB Ulna
            // 80 CC Radius
            // 81 CD Carpus
            // 82 CE Metatarsals/Phalanges
            string rightForelimb = string.Empty;
            rightForelimb += customAppend(rightForelimb, "Scapula", 77);
            rightForelimb += customAppend(rightForelimb, "Humorous", 78);
            rightForelimb += customAppend(rightForelimb, "Ulna", 79);
            rightForelimb += customAppend(rightForelimb, "Radius", 80);
            rightForelimb += customAppend(rightForelimb, "Carpus", 81);
            rightForelimb += customAppend(rightForelimb, "Metatarsals/Phalanges", 82);
            rightForelimb += customAppend(rightForelimb, string.Empty, 76);

            // Left rear limb
            // 83 CF Other notes
            // 84 CG Raw response
            string leftRearLimb = string.Empty;
            leftRearLimb += customAppend(leftRearLimb, string.Empty, 84);
            leftRearLimb += customAppend(leftRearLimb, string.Empty, 83);

            // Right rear limb
            // 85 CH Other notes
            // 86 CI Raw response
            string rightRearLimb = string.Empty;
            rightRearLimb += customAppend(rightRearLimb, string.Empty, 86);
            rightRearLimb += customAppend(rightRearLimb, string.Empty, 85);

            // Extended other notes
            // 87 CJ Raw response
            string other = row.Length > 87 ? row[87]?.Trim()?.EscapeForLatex() : string.Empty;

            // Coccygeal
            // 88 CK Raw response
            // 91 CN Coccygeal
            string coccygeal = string.Empty;
            coccygeal += customAppend(coccygeal, "Coccygeal", 91);
            coccygeal += customAppend(coccygeal, string.Empty, 88);


            // Download the template to memory to do the string replacement
            this.logger.LogInformation($"Downloading {templateLatexBlob.Uri} to memory for template application.");
            MemoryStream templateLatexBlobContents = new();
            _ = await templateLatexBlob.DownloadToAsync(templateLatexBlobContents, cancellationToken);
            templateLatexBlobContents.Position = 0;
            string templateContents = new StreamReader(templateLatexBlobContents).ReadToEnd();
            templateLatexBlobContents = new();

            // Do the string replacement
            templateContents = templateContents
                .Replace("((PatientName))", patientName?.EscapeForLatex())
                .Replace("((OwnerName))", ownerName?.EscapeForLatex())
                .Replace("((DateOfService))", dateOfService.ToString("MM/dd/yyyy"))
                .Replace("((Head))", head)
                .Replace("((Cervical))", cervical)
                .Replace("((Thoracic))", thoracic)
                .Replace("((Ribs))", ribs)
                .Replace("((Lumbar))", lumbar)
                .Replace("((PelvicSacral))", pelvicSacral)
                .Replace("((LeftForelimb))", leftForelimb)
                .Replace("((RightForelimb))", rightForelimb)
                .Replace("((LeftRearLimb))", leftRearLimb)
                .Replace("((RightRearLimb))", rightRearLimb)
                .Replace("((CoccygealOther))", coccygeal)
                .Replace("((OtherNotes))", other)
                ;

            // Upload back to be processed
            this.logger.LogInformation($"Uploading memory for template application to {templateLatexBlob.Uri}.");
            StreamWriter writer = new(templateLatexBlobContents);
            writer.Write(templateContents);
            writer.Flush();
            templateLatexBlobContents.Position = 0;
            _ = await templateLatexBlob.UploadAsync(templateLatexBlobContents, overwrite: true, cancellationToken);

            // Add the email request
            string subject = default;
            if (!string.IsNullOrWhiteSpace(template.SubjectTemplate))
            {
                subject = template.SubjectTemplate
                    .Replace("((DateOfService))", dateOfService.ToString("yyyy-MM-dd"))
                    .Replace("((OwnerName))", ownerName)
                    .Replace("((PatientName))", patientName)
                    ;
            }
            string attachmentName = default;
            if (!string.IsNullOrWhiteSpace(template.AttachmentNameTemplate))
            {
                attachmentName = template.AttachmentNameTemplate
                    .Replace("((DateOfService))", dateOfService.ToString("yyyy-MM-dd"))
                    .Replace("((OwnerName))", ownerName)
                    .Replace("((PatientName))", patientName)
                    .Replace(" ", "-")
                    ;
            }
            _ = nsContext.EmailLatexDocuments.Add(
                new EmailLatexDocument
                {
                    Id = latexDocument.Id,
                    To = template.EmailTo,
                    CC = string.Join(';', ccEmail),
                    Subject = subject,
                    AttachmentName = attachmentName
                });

            // And save it to enable processing
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            // Kick off the pdf generation
            TaskHubClient taskHubClient = await this.sentinals.DurableOrchestrationClient.WaitForCompletionAsync(cancellationToken);
            OrchestrationInstance orchestration = await taskHubClient.CreateOrchestrationInstanceAsync(
                typeof(DurableTaskImpl.Orchestrators.LatexGenerationOrchestrator),
                instanceId: this.op.Telemetry.Context.Operation.Id,
                input: latexDocument.Id);
        }
    }
}
