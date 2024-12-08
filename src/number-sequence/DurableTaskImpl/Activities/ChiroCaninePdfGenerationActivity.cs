using DurableTask.Core;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Models;
using number_sequence.Utilities;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Unlimitedinf.Utilities.Extensions;

namespace number_sequence.DurableTaskImpl.Activities
{
    public sealed class ChiroCaninePdfGenerationActivity : AsyncTaskActivity<long, string>
    {
        private readonly GoogleSheetDataAccess googleSheetDataAccess;
        private readonly NsStorage nsStorage;
        private readonly IServiceProvider serviceProvider;
        private readonly Sentinals sentinals;
        private readonly ILogger<ChiroCaninePdfGenerationActivity> logger;
        private readonly TelemetryClient telemetryClient;

        public ChiroCaninePdfGenerationActivity(
            GoogleSheetDataAccess googleSheetDataAccess,
            NsStorage nsStorage,
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<ChiroCaninePdfGenerationActivity> logger,
            TelemetryClient telemetryClient)
        {
            this.googleSheetDataAccess = googleSheetDataAccess;
            this.nsStorage = nsStorage;
            this.serviceProvider = serviceProvider;
            this.sentinals = sentinals;
            this.logger = logger;
            this.telemetryClient = telemetryClient;
        }

        protected override async Task<string> ExecuteAsync(TaskContext context, long input)
        {
            // Basic setup
            using IOperationHolder<RequestTelemetry> op = this.telemetryClient.StartOperation<RequestTelemetry>(
                this.GetType().FullName,
                operationId: context.OrchestrationInstance.ExecutionId,
                parentOperationId: context.OrchestrationInstance.InstanceId);
            using CancellationTokenSource cts = new(TimeSpan.FromMinutes(5));
            CancellationToken cancellationToken = cts.Token;
            await this.sentinals.DBMigration.WaitForCompletionAsync(cancellationToken);

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            // Get the template information.
            PdfTemplate template = await nsContext.PdfTemplates.FirstOrDefaultAsync(x => x.Id == NsStorage.C.PT.ChiroCanine, cancellationToken);
            if (template == default)
            {
                throw new InvalidOperationException("No template defined.");
            }

            // Get the data from the spreadsheet. The first row is the headers (or a previously processed row).
            string spreadsheetRange = template.SpreadsheetRange.Replace("1", input.ToString());
            this.logger.LogInformation($"Skipping {input} known rows and querying range {spreadsheetRange}");
            IList<IList<object>> data = await this.googleSheetDataAccess.GetAsync(template.SpreadsheetId, spreadsheetRange, cancellationToken);
            IList<object> headers = data[0];
            data = data.Skip(1).ToList();

            // Only on reset or initial deployment, no data.
            if (!data.Any())
            {
                throw new InvalidOperationException("No rows of data.");
            }

            // See if we've already processed it.
            string[] row = data.First().Select(x => x as string).ToArray();
            string id = string.Join('|', row).ComputeSHA256();
            PdfTemplateSpreadsheetRow pdfTemplateRow = await nsContext.PdfTemplateSpreadsheetRows
                .SingleOrDefaultAsync(x => x.SpreadsheetId == template.SpreadsheetId && x.RowId == id, cancellationToken);
            if (pdfTemplateRow != default)
            {
                throw new InvalidOperationException($"Data row {id} ({input + 1}) was inserted for processing at {pdfTemplateRow.CreatedDate:u} and processed {pdfTemplateRow.ProcessedAt:u}");
            }
            else
            {
                this.logger.LogInformation($"Data row {id} ({input + 1}) is new. Setting up for processing.");
                pdfTemplateRow = new()
                {
                    SpreadsheetId = template.SpreadsheetId,
                    RowId = id,
                    DocumentId = context.OrchestrationInstance.InstanceId,
                    ProcessedAt = DateTimeOffset.UtcNow
                };
                _ = nsContext.PdfTemplateSpreadsheetRows.Add(pdfTemplateRow);
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
                            + $"{prefix} {row[index]}".Trim();
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

            // Create and populate the data.
            ChiroCaninePdfDocument pdf = new();

            // Parse the data row into meaningful replacement values
            // Index, Column label in spreadsheet, Description
            //  0  A Submission timestamp
            // 80 CC Email Address
            string emailSubmitter = row.Length > 80 ? row[80]?.Trim() : string.Empty;
            if (!string.IsNullOrWhiteSpace(emailSubmitter) && !string.IsNullOrWhiteSpace(template.AllowedSubmitterEmails))
            {
                // Historically, old rows did not have the email captured.
                // So only check for allowed submitters if both have a value.
                if (template.AllowedSubmitterEmails.Contains(emailSubmitter))
                {
                    this.logger.LogInformation($"{emailSubmitter} is allowed to use this form.");
                }
                else
                {
                    this.logger.LogWarning($"{emailSubmitter} is not allowed to use this form.");
                    _ = await nsContext.SaveChangesAsync(cancellationToken);
                    return default;
                }
            }

            // Intake info
            //  1  B Patient Name
            //  2  C Owner Name
            //  3  D Date of Service
            //  4  E CC email(s)
            pdf.PatientName = row[1]?.Trim();
            pdf.OwnerName = row[2]?.Trim();
            pdf.DateOfService = DateTimeOffset.Parse(row[3]);
            string[] ccEmail = row[4].Split(new[] { ',', ';' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            // Head
            //  5  F Other notes
            //  6  G Occiput
            //  7  H TMJ
            pdf.Head += customAppend(pdf.Head, "Occiput", 6);
            pdf.Head += customAppend(pdf.Head, "TMJ", 7);
            pdf.Head += customAppend(pdf.Head, string.Empty, 5);

            // Cervical
            //  8  I Other Notes
            //  9  J C1
            // 10  K C2
            // 11  L C3
            // 12  M C4
            // 13  N C5
            // 14  O C6
            // 15  P C7
            pdf.Cervical += customAppend(pdf.Cervical, "C1", 9);
            pdf.Cervical += customAppend(pdf.Cervical, "C2", 10);
            pdf.Cervical += customAppend(pdf.Cervical, "C3", 11);
            pdf.Cervical += customAppend(pdf.Cervical, "C4", 12);
            pdf.Cervical += customAppend(pdf.Cervical, "C5", 13);
            pdf.Cervical += customAppend(pdf.Cervical, "C6", 14);
            pdf.Cervical += customAppend(pdf.Cervical, "C7", 15);
            pdf.Cervical += customAppend(pdf.Cervical, string.Empty, 8);

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
            pdf.Thoracic += customAppend(pdf.Thoracic, "T1", 17);
            pdf.Thoracic += customAppend(pdf.Thoracic, "T2", 18);
            pdf.Thoracic += customAppend(pdf.Thoracic, "T3", 19);
            pdf.Thoracic += customAppend(pdf.Thoracic, "T4", 20);
            pdf.Thoracic += customAppend(pdf.Thoracic, "T5", 21);
            pdf.Thoracic += customAppend(pdf.Thoracic, "T6", 22);
            pdf.Thoracic += customAppend(pdf.Thoracic, "T7", 23);
            pdf.Thoracic += customAppend(pdf.Thoracic, "T8", 24);
            pdf.Thoracic += customAppend(pdf.Thoracic, "T9", 25);
            pdf.Thoracic += customAppend(pdf.Thoracic, "T10", 26);
            pdf.Thoracic += customAppend(pdf.Thoracic, "T11", 27);
            pdf.Thoracic += customAppend(pdf.Thoracic, "T12", 28);
            pdf.Thoracic += customAppend(pdf.Thoracic, "T13", 29);
            pdf.Thoracic += customAppend(pdf.Thoracic, "Sternum", 43);
            pdf.Thoracic += customAppend(pdf.Thoracic, string.Empty, 16);

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
            pdf.Ribs += customAppend(pdf.Ribs, "R1", 30);
            pdf.Ribs += customAppend(pdf.Ribs, "R2", 31);
            pdf.Ribs += customAppend(pdf.Ribs, "R3", 32);
            pdf.Ribs += customAppend(pdf.Ribs, "R4", 33);
            pdf.Ribs += customAppend(pdf.Ribs, "R5", 34);
            pdf.Ribs += customAppend(pdf.Ribs, "R6", 35);
            pdf.Ribs += customAppend(pdf.Ribs, "R7", 36);
            pdf.Ribs += customAppend(pdf.Ribs, "R8", 37);
            pdf.Ribs += customAppend(pdf.Ribs, "R9", 38);
            pdf.Ribs += customAppend(pdf.Ribs, "R10", 39);
            pdf.Ribs += customAppend(pdf.Ribs, "R11", 40);
            pdf.Ribs += customAppend(pdf.Ribs, "R12", 41);
            pdf.Ribs += customAppend(pdf.Ribs, "R13", 42);

            // Lumbar
            // 44 AS Other notes
            // 45 AT L1
            // 46 AU L2
            // 47 AV L3
            // 48 AW L4
            // 49 AX L5
            // 50 AY L6
            // 51 AZ L7
            pdf.Lumbar += customAppend(pdf.Lumbar, "L1", 45);
            pdf.Lumbar += customAppend(pdf.Lumbar, "L2", 46);
            pdf.Lumbar += customAppend(pdf.Lumbar, "L3", 47);
            pdf.Lumbar += customAppend(pdf.Lumbar, "L4", 48);
            pdf.Lumbar += customAppend(pdf.Lumbar, "L5", 49);
            pdf.Lumbar += customAppend(pdf.Lumbar, "L6", 50);
            pdf.Lumbar += customAppend(pdf.Lumbar, "L7", 51);
            pdf.Lumbar += customAppend(pdf.Lumbar, string.Empty, 44);

            // Sacrum
            // 52 BA Other notes
            // 53 BB Base
            // 54 BC Apex
            // Pelvic
            // 55 BD Other notes
            // 56 BE Left
            // 57 BF Right
            // 58 BG Traction
            pdf.PelvicSacral += customAppend(pdf.PelvicSacral, "Base", 53);
            pdf.PelvicSacral += customAppend(pdf.PelvicSacral, "Apex", 54);
            pdf.PelvicSacral += customAppend(pdf.PelvicSacral, "Sacrum", 52);
            pdf.PelvicSacral += customAppend(pdf.PelvicSacral, "Left", 56);
            pdf.PelvicSacral += customAppend(pdf.PelvicSacral, "Right", 57);
            pdf.PelvicSacral += customAppend(pdf.PelvicSacral, string.Empty, 58);
            pdf.PelvicSacral += customAppend(pdf.PelvicSacral, "Pelvis", 55);

            // Left forelimb
            // 59 BH Other notes
            // 60 BI Scapula
            // 61 BJ Humorous
            // 62 BK Ulna
            // 63 BL Radius
            // 64 BM Carpus
            // 65 BN Metatarsals/Phalanges
            pdf.LeftForelimb += customAppend(pdf.LeftForelimb, "Scapula", 60);
            pdf.LeftForelimb += customAppend(pdf.LeftForelimb, "Humorous", 61);
            pdf.LeftForelimb += customAppend(pdf.LeftForelimb, "Ulna", 62);
            pdf.LeftForelimb += customAppend(pdf.LeftForelimb, "Radius", 63);
            pdf.LeftForelimb += customAppend(pdf.LeftForelimb, "Carpus", 64);
            pdf.LeftForelimb += customAppend(pdf.LeftForelimb, "Metatarsals/Phalanges", 65);
            pdf.LeftForelimb += customAppend(pdf.LeftForelimb, string.Empty, 59);

            // Right forelimb
            // 66 BO Other notes
            // 67 BP Scapula
            // 68 BQ Humorous
            // 69 BR Ulna
            // 70 BS Radius
            // 71 BT Carpus
            // 72 BU Metatarsals/Phalanges
            pdf.RightForelimb += customAppend(pdf.RightForelimb, "Scapula", 67);
            pdf.RightForelimb += customAppend(pdf.RightForelimb, "Humorous", 68);
            pdf.RightForelimb += customAppend(pdf.RightForelimb, "Ulna", 69);
            pdf.RightForelimb += customAppend(pdf.RightForelimb, "Radius", 70);
            pdf.RightForelimb += customAppend(pdf.RightForelimb, "Carpus", 71);
            pdf.RightForelimb += customAppend(pdf.RightForelimb, "Metatarsals/Phalanges", 72);
            pdf.RightForelimb += customAppend(pdf.RightForelimb, string.Empty, 66);

            // Left rear limb
            // 73 BV Other notes
            // 74 BW Raw response
            pdf.LeftRearLimb += customAppend(pdf.LeftRearLimb, string.Empty, 74);
            pdf.LeftRearLimb += customAppend(pdf.LeftRearLimb, string.Empty, 73);

            // Right rear limb
            // 75 BX Other notes
            // 76 BY Raw response
            pdf.RightRearLimb += customAppend(pdf.RightRearLimb, string.Empty, 76);
            pdf.RightRearLimb += customAppend(pdf.RightRearLimb, string.Empty, 75);

            // Coccygeal
            // 77 BZ Other notes
            // 78 CA Coccygeal
            pdf.Coccygeal += customAppend(pdf.Coccygeal, "Coccygeal", 78);
            pdf.Coccygeal += customAppend(pdf.Coccygeal, string.Empty, 77);

            // Extended other notes
            // 79 CB Raw response
            pdf.Other = row.Length > 79 ? row[79]?.Trim() : string.Empty;

            // Generate the PDF
            MemoryStream ms = new();
            using (IDisposable disposable = this.logger.BeginScope("Generating PDF"))
            {
                Settings.License = LicenseType.Community;
                pdf.GeneratePdf(ms);
                ms.Position = 0;
            }

            // Add the email request
            string subject = default;
            if (!string.IsNullOrWhiteSpace(template.SubjectTemplate))
            {
                subject = template.SubjectTemplate
                    .Replace("((DateOfService))", pdf.DateOfService.ToString("yyyy-MM-dd"))
                    .Replace("((OwnerName))", pdf.OwnerName)
                    .Replace("((PatientName))", pdf.PatientName)
                    ;
            }
            string attachmentName = default;
            if (!string.IsNullOrWhiteSpace(template.AttachmentNameTemplate))
            {
                attachmentName = template.AttachmentNameTemplate
                    .Replace("((DateOfService))", pdf.DateOfService.ToString("yyyy-MM-dd"))
                    .Replace("((OwnerName))", pdf.OwnerName)
                    .Replace("((PatientName))", pdf.PatientName)
                    .Replace(" ", "-")
                    ;
            }
            EmailDocument emailDocument = new()
            {
                Id = context.OrchestrationInstance.InstanceId,
                To = template.EmailTo,
                CC = string.Join(';', ccEmail),
                Subject = subject,
                AttachmentName = attachmentName
            };
            _ = nsContext.EmailDocuments.Add(emailDocument);

            // Put it in storage
            Azure.Storage.Blobs.BlobClient pdfBlobClient = this.nsStorage.GetBlobClient(emailDocument);
            this.logger.LogInformation($"Uploading to {pdfBlobClient.Uri.AbsoluteUri.Split('?').First()}");
            _ = await pdfBlobClient.UploadAsync(ms, cancellationToken);

            _ = await nsContext.SaveChangesAsync(cancellationToken);
            return default;
        }

        private sealed class ChiroCaninePdfDocument : IDocument
        {
            public string PatientName { get; set; }
            public string OwnerName { get; set; }
            public DateTimeOffset DateOfService { get; set; }

            public string Head { get; set; } = string.Empty;
            public string Cervical { get; set; } = string.Empty;
            public string Thoracic { get; set; } = string.Empty;
            public string Ribs { get; set; } = string.Empty;
            public string Lumbar { get; set; } = string.Empty;
            public string PelvicSacral { get; set; } = string.Empty;
            public string LeftForelimb { get; set; } = string.Empty;
            public string RightForelimb { get; set; } = string.Empty;
            public string LeftRearLimb { get; set; } = string.Empty;
            public string RightRearLimb { get; set; } = string.Empty;
            public string Coccygeal { get; set; } = string.Empty;
            public string Other { get; set; } = string.Empty;

            public void Compose(IDocumentContainer container)
            {
                const float baseFontSize = 10;

                _ = container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.MarginTop(0.4f, Unit.Inch);
                    page.MarginBottom(0.25f, Unit.Inch);
                    page.MarginHorizontal(0.5f, Unit.Inch);

                    page.DefaultTextStyle(TextStyle.Default.FontSize(baseFontSize).FontFamily("CMU Serif"));

                    // Header, repeated every page.
                    page.Header().Element(container =>
                    {
                        container.Column(column =>
                        {
                            _ = column.Item()
                                .Text($"Patient Name: {this.PatientName}");
                            _ = column.Item()
                                .Text($"Owner Name: {this.OwnerName}");
                            _ = column.Item()
                                .Text($"Date of Service: {this.DateOfService:MM/dd/yyyy}");
                            _ = column.Item()
                                .PaddingVertical(2)
                                .LineHorizontal(0.25f);
                        });
                    });

                    // Main content, can span as many pages as necessary.
                    page.Content().Element(container =>
                    {
                        container.PaddingVertical(7).Column(column =>
                        {
                            _ = column.Item()
                                .PaddingBottom(7)
                                .AlignCenter()
                                .MaxWidth(5, Unit.Inch)
                                .Image(Resources.ChiroCanineDiagram);

                            // Table of records
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(18);
                                    columns.RelativeColumn(82);
                                });

                                static IContainer CellStyle(IContainer container)
                                    => container.PaddingVertical(2);

                                _ = table.Cell().Element(CellStyle).Text("Head:");
                                _ = table.Cell().Element(CellStyle).Text(this.Head);

                                _ = table.Cell().Element(CellStyle).Text("Cervical:");
                                _ = table.Cell().Element(CellStyle).Text(this.Cervical);

                                _ = table.Cell().Element(CellStyle).Text("Thoracic:");
                                _ = table.Cell().Element(CellStyle).Text(this.Thoracic);

                                _ = table.Cell().Element(CellStyle).Text("Ribs:");
                                _ = table.Cell().Element(CellStyle).Text(this.Ribs);

                                _ = table.Cell().Element(CellStyle).Text("Lumbar:");
                                _ = table.Cell().Element(CellStyle).Text(this.Lumbar);

                                _ = table.Cell().Element(CellStyle).Text("Pelvic / Sacral:");
                                _ = table.Cell().Element(CellStyle).Text(this.PelvicSacral);

                                _ = table.Cell().Element(CellStyle).Text("Left Forelimb:");
                                _ = table.Cell().Element(CellStyle).Text(this.LeftForelimb);

                                _ = table.Cell().Element(CellStyle).Text("Right Forelimb:");
                                _ = table.Cell().Element(CellStyle).Text(this.RightForelimb);

                                _ = table.Cell().Element(CellStyle).Text("Left Rear Limb:");
                                _ = table.Cell().Element(CellStyle).Text(this.LeftRearLimb);

                                _ = table.Cell().Element(CellStyle).Text("Right Rear Limb:");
                                _ = table.Cell().Element(CellStyle).Text(this.RightRearLimb);

                                _ = table.Cell().Element(CellStyle).Text("Coccygeal / Other extremities:");
                                _ = table.Cell().Element(CellStyle).Text(this.Coccygeal);

                                _ = table.Cell().Element(CellStyle).Text("Other notes:");
                                _ = table.Cell().Element(CellStyle).Text(this.Other);
                            });
                        });
                    });
                });
            }
        }
    }
}
