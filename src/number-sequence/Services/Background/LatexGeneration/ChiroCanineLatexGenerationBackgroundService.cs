using Azure.Storage.Blobs;
using DurableTask.Core;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using number_sequence.DataAccess;
using number_sequence.Models;
using number_sequence.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unlimitedinf.Utilities.Extensions;

namespace number_sequence.Services.Background.LatexGeneration
{
    public sealed class ChiroCanineLatexGenerationBackgroundService : SqlSynchronizedBackgroundService
    {
        private readonly GoogleSheetDataAccess googleSheetDataAccess;
        private readonly NsStorage nsStorage;

        public ChiroCanineLatexGenerationBackgroundService(
            GoogleSheetDataAccess googleSheetDataAccess,
            NsStorage nsStorage,
            IOptions<Options.Google> googleOptions,
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<ChiroCanineLatexGenerationBackgroundService> logger,
            TelemetryClient telemetryClient)
            : base(serviceProvider, sentinals, logger, telemetryClient)
        {
            this.googleSheetDataAccess = googleSheetDataAccess;
            this.nsStorage = nsStorage;
        }

        protected override TimeSpan Interval => TimeSpan.FromMinutes(5);

        protected override async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            // Get the template information
            LatexTemplate template = await nsContext.LatexTemplates.FirstOrDefaultAsync(x => x.Id == NsStorage.C.LTBP.ChrioCanine, cancellationToken);
            if (template == default)
            {
                this.logger.LogInformation("No template defined.");
                return;
            }

            // Get the data from the spreadsheet. The first row is the headers
            IList<IList<object>> data = await this.googleSheetDataAccess.GetAsync(template.SpreadsheetId, template.SpreadsheetRange, cancellationToken);
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
                string id = string.Join('|', row).GetSHA256();
                latexTemplateRow = await nsContext.LatexTemplateSpreadsheetRows
                    .SingleOrDefaultAsync(x => x.SpreadsheetId == template.SpreadsheetId && x.RowId == id, cancellationToken);

                if (latexTemplateRow != default)
                {
                    this.logger.LogInformation($"Data row {id} ({rowIndex}) was inserted for processing at {latexTemplateRow.CreatedDate:u} and processed {latexTemplateRow.ProcessedAt:u}");
                    continue;
                }
                else
                {
                    this.logger.LogInformation($"Data row {id} ({rowIndex}) is new. Setting up for processing.");
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
                    templateBlob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(this.Interval * 3)),
                    cancellationToken: cancellationToken);

                if (templateBlob.Name.EndsWith("template.tex"))
                {
                    templateLatexBlob = targetBlob;
                }
            }

            string customAppend(string existing, string prefix, int index)
            {
                try
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
                catch
                {
                    this.logger.LogInformation(new { existing, prefix, index, rowLength = row.Length, row = row.ToJsonString() }.ToJsonString());
                    throw;
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
            // 43 AR Sternum
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
            thoracic += customAppend(thoracic, "Sternum", 43);
            thoracic += customAppend(thoracic, string.Empty, 16);

            // Ribs
            // 30 AE R1
            // 31 AF R2
            // 32 AG R3
            // 33 AH R4
            // 34 AI R5
            // 35 AJ R6
            // 36 AK R7
            // 37 AL R8
            // 38 AM R9
            // 39 AN R10
            // 40 AO R11
            // 41 AP R12
            // 42 AQ R13
            string ribs = string.Empty;
            ribs += customAppend(ribs, "R1", 30);
            ribs += customAppend(ribs, "R2", 31);
            ribs += customAppend(ribs, "R3", 32);
            ribs += customAppend(ribs, "R4", 33);
            ribs += customAppend(ribs, "R5", 34);
            ribs += customAppend(ribs, "R6", 35);
            ribs += customAppend(ribs, "R7", 36);
            ribs += customAppend(ribs, "R8", 37);
            ribs += customAppend(ribs, "R9", 38);
            ribs += customAppend(ribs, "R10", 39);
            ribs += customAppend(ribs, "R11", 40);
            ribs += customAppend(ribs, "R12", 41);
            ribs += customAppend(ribs, "R13", 42);

            // Lumbar
            // 44 AS Other notes
            // 45 AT L1
            // 46 AU L2
            // 47 AV L3
            // 48 AW L4
            // 49 AX L5
            // 50 AY L6
            // 51 AZ L7
            string lumbar = string.Empty;
            lumbar += customAppend(lumbar, "L1", 45);
            lumbar += customAppend(lumbar, "L2", 46);
            lumbar += customAppend(lumbar, "L3", 47);
            lumbar += customAppend(lumbar, "L4", 48);
            lumbar += customAppend(lumbar, "L5", 49);
            lumbar += customAppend(lumbar, "L6", 50);
            lumbar += customAppend(lumbar, "L7", 51);
            lumbar += customAppend(lumbar, string.Empty, 44);

            // Sacrum
            // 52 BA Other notes
            // 53 BB Base
            // 54 BC Apex
            // Pelvic
            // 55 BD Other notes
            // 56 BE Left
            // 57 BF Right
            // 58 BG Traction
            string pelvicSacral = string.Empty;
            pelvicSacral += customAppend(pelvicSacral, "Base", 53);
            pelvicSacral += customAppend(pelvicSacral, "Apex", 54);
            pelvicSacral += customAppend(pelvicSacral, "Sacrum", 52);
            pelvicSacral += customAppend(pelvicSacral, "Left", 56);
            pelvicSacral += customAppend(pelvicSacral, "Right", 57);
            pelvicSacral += customAppend(pelvicSacral, string.Empty, 58);
            pelvicSacral += customAppend(pelvicSacral, "Pelvis", 55);

            // Left forelimb
            // 59 BH Other notes
            // 60 BI Scapula
            // 61 BJ Humorous
            // 62 BK Ulna
            // 63 BL Radius
            // 64 BM Carpus
            // 65 BN Metatarsals/Phalanges
            string leftForelimb = string.Empty;
            leftForelimb += customAppend(leftForelimb, "Scapula", 60);
            leftForelimb += customAppend(leftForelimb, "Humorous", 61);
            leftForelimb += customAppend(leftForelimb, "Ulna", 62);
            leftForelimb += customAppend(leftForelimb, "Radius", 63);
            leftForelimb += customAppend(leftForelimb, "Carpus", 64);
            leftForelimb += customAppend(leftForelimb, "Metatarsals/Phalanges", 65);
            leftForelimb += customAppend(leftForelimb, string.Empty, 59);

            // Right forelimb
            // 66 BO Other notes
            // 67 BP Scapula
            // 68 BQ Humorous
            // 69 BR Ulna
            // 70 BS Radius
            // 71 BT Carpus
            // 72 BU Metatarsals/Phalanges
            string rightForelimb = string.Empty;
            rightForelimb += customAppend(rightForelimb, "Scapula", 67);
            rightForelimb += customAppend(rightForelimb, "Humorous", 68);
            rightForelimb += customAppend(rightForelimb, "Ulna", 69);
            rightForelimb += customAppend(rightForelimb, "Radius", 70);
            rightForelimb += customAppend(rightForelimb, "Carpus", 71);
            rightForelimb += customAppend(rightForelimb, "Metatarsals/Phalanges", 72);
            rightForelimb += customAppend(rightForelimb, string.Empty, 66);

            // Left rear limb
            // 73 BV Other notes
            // 74 BW Raw response
            string leftRearLimb = string.Empty;
            leftRearLimb += customAppend(leftRearLimb, string.Empty, 74);
            leftRearLimb += customAppend(leftRearLimb, string.Empty, 73);

            // Right rear limb
            // 75 BX Other notes
            // 76 BY Raw response
            string rightRearLimb = string.Empty;
            rightRearLimb += customAppend(rightRearLimb, string.Empty, 76);
            rightRearLimb += customAppend(rightRearLimb, string.Empty, 75);

            // Coccygeal
            // 77 BZ Other notes
            // 78 CA Coccygeal
            string coccygeal = string.Empty;
            coccygeal += customAppend(coccygeal, "Coccygeal", 78);
            coccygeal += customAppend(coccygeal, string.Empty, 77);

            // Extended other notes
            // 79 CB Raw response
            string other = row.Length > 79 ? row[79]?.Trim()?.EscapeForLatex() : string.Empty;


            // Download the template to memory to do the string replacement
            this.logger.LogInformation($"Downloading {templateLatexBlob.Uri} to memory for template application.");
            MemoryStream templateLatexBlobContents = new();
            _ = await templateLatexBlob.DownloadToAsync(templateLatexBlobContents, cancellationToken);
            templateLatexBlobContents.Position = 0;
            string templateContents = new StreamReader(templateLatexBlobContents).ReadToEnd();
            templateLatexBlobContents.Position = 0;

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
