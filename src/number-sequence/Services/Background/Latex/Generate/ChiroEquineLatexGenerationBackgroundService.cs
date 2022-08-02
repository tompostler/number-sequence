using Azure.Storage.Blobs;
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

namespace number_sequence.Services.Background.Latex.Generate
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

        protected override TimeSpan Interval => TimeSpan.FromMinutes(5);

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
            string id = default;
            LatexDocument latexDocument = default;
            for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
            {
                row = data[rowIndex].Select(x => x as string).ToArray();
                id = string.Join('|', row).GetSHA256();
                latexDocument = await nsContext.LatexDocuments.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (latexDocument != default)
                {
                    this.logger.LogInformation($"Data row {id} ({rowIndex}) was inserted for processing at {latexDocument.CreatedDate:u} and processed {latexDocument.ProcessedAt:u}");
                    continue;
                }
                else
                {
                    this.logger.LogInformation($"Data row {id} ({rowIndex}) is new. Setting up for processing.");
                    break;
                }
            }

            // Create the new record for generating the document
            latexDocument = new()
            {
                Id = id
            };
            _ = nsContext.LatexDocuments.Add(latexDocument);

            // Copy the template blob(s) to the working directory
            BlobClient templateLatexBlob = default;
            await foreach (BlobClient templateBlob in this.nsStorage.EnumerateAllBlobsForLatexTemplateAsync(NsStorage.C.LTBP.ChiroEquine, cancellationToken))
            {
                string targetPath = $"{NsStorage.C.LBP.Input}/{templateBlob.Name.Substring((NsStorage.C.LTBP.ChiroEquine + "/").Length).Replace("template", id)}";
                BlobClient targetBlob = this.nsStorage.GetBlobClientForLatexJob(id, targetPath);
                this.logger.LogInformation($"Copying {templateBlob.Uri} to {targetBlob.Uri}");
                _ = await targetBlob.SyncCopyFromUriAsync(
                    templateBlob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(this.Interval * 3)),
                    cancellationToken: cancellationToken);

                if (templateBlob.Name.EndsWith("template.tex"))
                {
                    templateLatexBlob = templateBlob;
                }
            }

            static string customAppend(string existing, string prefix, string value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return (string.IsNullOrWhiteSpace(existing) ? string.Empty : ", ")
                        + $"{prefix} {value}".Trim();
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
            string patientName = row[1];
            string ownerName = row[2];
            var dateOfService = DateTimeOffset.Parse(row[3]);
            string[] ccEmail = row[4].Split(new[] { ',', ';' }, StringSplitOptions.TrimEntries);

            // Head
            //  5  F Other notes
            //  6  G Occiput
            //  7  H TMJ
            string head = string.Empty;
            head += customAppend(head, "Occiput", row[6]);
            head += customAppend(head, "TMJ", row[7]);
            head += customAppend(head, string.Empty, row[5]);

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
            cervical += customAppend(cervical, "C1", row[9]);
            cervical += customAppend(cervical, "C2", row[10]);
            cervical += customAppend(cervical, "C3", row[11]);
            cervical += customAppend(cervical, "C4", row[12]);
            cervical += customAppend(cervical, "C5", row[13]);
            cervical += customAppend(cervical, "C6", row[14]);
            cervical += customAppend(cervical, "C7", row[15]);
            cervical += customAppend(cervical, string.Empty, row[8]);

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
            string thoracic = string.Empty;
            thoracic += customAppend(thoracic, "T1", row[17]);
            thoracic += customAppend(thoracic, "T2", row[18]);
            thoracic += customAppend(thoracic, "T3", row[19]);
            thoracic += customAppend(thoracic, "T4", row[20]);
            thoracic += customAppend(thoracic, "T5", row[21]);
            thoracic += customAppend(thoracic, "T6", row[22]);
            thoracic += customAppend(thoracic, "T7", row[23]);
            thoracic += customAppend(thoracic, "T8", row[24]);
            thoracic += customAppend(thoracic, "T9", row[25]);
            thoracic += customAppend(thoracic, "T10", row[26]);
            thoracic += customAppend(thoracic, "T11", row[27]);
            thoracic += customAppend(thoracic, "T12", row[28]);
            thoracic += customAppend(thoracic, "T13", row[29]);
            thoracic += customAppend(thoracic, "T14", row[30]);
            thoracic += customAppend(thoracic, "T15", row[31]);
            thoracic += customAppend(thoracic, "T16", row[32]);
            thoracic += customAppend(thoracic, "T17", row[33]);
            thoracic += customAppend(thoracic, "T18", row[34]);
            thoracic += customAppend(thoracic, string.Empty, row[16]);

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
            ribs += customAppend(ribs, "R1", row[35]);
            ribs += customAppend(ribs, "R2", row[36]);
            ribs += customAppend(ribs, "R3", row[37]);
            ribs += customAppend(ribs, "R4", row[38]);
            ribs += customAppend(ribs, "R5", row[39]);
            ribs += customAppend(ribs, "R6", row[40]);
            ribs += customAppend(ribs, "R7", row[41]);
            ribs += customAppend(ribs, "R8", row[42]);
            ribs += customAppend(ribs, "R9", row[43]);
            ribs += customAppend(ribs, "R10", row[44]);
            ribs += customAppend(ribs, "R11", row[45]);
            ribs += customAppend(ribs, "R12", row[46]);
            ribs += customAppend(ribs, "R13", row[47]);
            ribs += customAppend(ribs, "R14", row[48]);
            ribs += customAppend(ribs, "R15", row[49]);
            ribs += customAppend(ribs, "R16", row[50]);
            ribs += customAppend(ribs, "R17", row[51]);
            ribs += customAppend(ribs, "R18", row[52]);

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
            lumbar += customAppend(lumbar, "L1", row[54]);
            lumbar += customAppend(lumbar, "L2", row[55]);
            lumbar += customAppend(lumbar, "L3", row[56]);
            lumbar += customAppend(lumbar, "L4", row[57]);
            lumbar += customAppend(lumbar, "L5", row[58]);
            lumbar += customAppend(lumbar, "L6", row[59]);
            lumbar += customAppend(lumbar, "L3/L4 Intertransverse", row[60]);
            lumbar += customAppend(lumbar, "L4/L5 Intertransverse", row[61]);
            lumbar += customAppend(lumbar, "L5/L6 Intertransverse", row[62]);
            lumbar += customAppend(lumbar, string.Empty, row[53]);

            // Sacrum
            // 63 BL Other notes
            // 64 BM Base
            // 65 BN Apex
            // Pelvic
            // 66 BO Other notes
            // 67 BP Left
            // 68 BQ Right
            string pelvicSacral = string.Empty;
            pelvicSacral += customAppend(pelvicSacral, "Base", row[64]);
            pelvicSacral += customAppend(pelvicSacral, "Apex", row[65]);
            pelvicSacral += customAppend(pelvicSacral, "Sacrum", row[63]);
            pelvicSacral += customAppend(pelvicSacral, "Left", row[67]);
            pelvicSacral += customAppend(pelvicSacral, "Right", row[68]);
            pelvicSacral += customAppend(pelvicSacral, "Pelvis", row[66]);

            // Left forelimb
            // 69 BR Other notes
            // 70 BS Scapula
            // 71 BT Humorous
            // 72 BU Ulna
            // 73 BV Radius
            // 74 BW Carpus
            // 75 BX Metatarsals/Phalanges
            string leftForelimb = string.Empty;
            leftForelimb += customAppend(leftForelimb, "Scapula", row[70]);
            leftForelimb += customAppend(leftForelimb, "Humorous", row[71]);
            leftForelimb += customAppend(leftForelimb, "Ulna", row[72]);
            leftForelimb += customAppend(leftForelimb, "Radius", row[73]);
            leftForelimb += customAppend(leftForelimb, "Carpus", row[74]);
            leftForelimb += customAppend(leftForelimb, "Metatarsals/Phalanges", row[75]);
            leftForelimb += customAppend(leftForelimb, string.Empty, row[69]);

            // Right forelimb
            // 76 BY Other notes
            // 77 BZ Scapula
            // 78 CA Humorous
            // 79 CB Ulna
            // 80 CC Radius
            // 81 CD Carpus
            // 82 CE Metatarsals/Phalanges
            string rightForelimb = string.Empty;
            rightForelimb += customAppend(rightForelimb, "Scapula", row[77]);
            rightForelimb += customAppend(rightForelimb, "Humorous", row[78]);
            rightForelimb += customAppend(rightForelimb, "Ulna", row[79]);
            rightForelimb += customAppend(rightForelimb, "Radius", row[80]);
            rightForelimb += customAppend(rightForelimb, "Carpus", row[81]);
            rightForelimb += customAppend(rightForelimb, "Metatarsals/Phalanges", row[82]);
            rightForelimb += customAppend(rightForelimb, string.Empty, row[76]);

            // Left rear limb
            // 83 CF Other notes
            // 84 CG Raw response
            string leftRearLimb = string.Empty;
            leftRearLimb += customAppend(leftRearLimb, string.Empty, row[84]);
            leftRearLimb += customAppend(leftRearLimb, string.Empty, row[83]);

            // Right rear limb
            // 85 CH Other notes
            // 86 CI Raw response
            string rightRearLimb = string.Empty;
            rightRearLimb += customAppend(rightRearLimb, string.Empty, row[86]);
            rightRearLimb += customAppend(rightRearLimb, string.Empty, row[85]);

            // Extended other notes
            // 87 CJ Raw response
            string other = row[87];

            // Coccygeal
            // 88 CK Raw response
            string coccygeal = row[88];


            // Download the template to memory to do the string replacement
            this.logger.LogInformation($"Downloading {templateLatexBlob.Uri} to memory for template application.");
            MemoryStream templateLatexBlobContents = new();
            _ = await templateLatexBlob.DownloadToAsync(templateLatexBlobContents, cancellationToken);
            templateLatexBlobContents.Position = 0;
            string templateContents = new StreamReader(templateLatexBlobContents).ReadToEnd();
            templateLatexBlobContents.Position = 0;

            // Do the string replacement
            templateContents = templateContents
                .Replace("((PatientName))", patientName)
                .Replace("((OwnerName))", ownerName)
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
                    Id = id,
                    To = template.EmailTo,
                    CC = string.Join(';', ccEmail),
                    Subject = subject,
                    AttachmentName = attachmentName
                });

            // And save it to enable processing
            _ = await nsContext.SaveChangesAsync(cancellationToken);
        }
    }
}
