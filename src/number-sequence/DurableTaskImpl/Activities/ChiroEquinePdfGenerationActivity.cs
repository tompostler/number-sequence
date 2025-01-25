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
    public sealed class ChiroEquinePdfGenerationActivity : AsyncTaskActivity<long, string>
    {
        private readonly GoogleSheetDataAccess googleSheetDataAccess;
        private readonly NsStorage nsStorage;
        private readonly IServiceProvider serviceProvider;
        private readonly Sentinals sentinals;
        private readonly ILogger<ChiroEquinePdfGenerationActivity> logger;
        private readonly TelemetryClient telemetryClient;

        public ChiroEquinePdfGenerationActivity(
            GoogleSheetDataAccess googleSheetDataAccess,
            NsStorage nsStorage,
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<ChiroEquinePdfGenerationActivity> logger,
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
            PdfTemplate template = await nsContext.PdfTemplates.FirstOrDefaultAsync(x => x.Id == NsStorage.C.PT.ChiroEquine, cancellationToken);
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
            ChiroEquinePdfDocument pdf = new();

            // Parse the data row into meaningful replacement values
            // Index, Column label in spreadsheet, Description
            //  0  A Submission timestamp
            // 92 CO Email Address
            string emailSubmitter = row.Length > 92 ? row[92]?.Trim() : string.Empty;
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
            // 93 CP Clinic Abbreviation
            pdf.PatientName = row[1]?.Trim();
            pdf.OwnerName = row[2]?.Trim();
            pdf.DateOfService = DateTimeOffset.Parse(row[3]);
            string[] ccEmail = row[4].Split(new[] { ',', ';' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            string clinicAbbreviation = row.Length > 93 ? row[93]?.Trim() : string.Empty;
            if (!string.IsNullOrWhiteSpace(clinicAbbreviation))
            {
                pdf.OwnerName = clinicAbbreviation + " - " + pdf.OwnerName;
            }

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
            // 30 AE T14
            // 31 AF T15
            // 32 AG T16
            // 33 AH T17
            // 34 AI T18
            // 89 CL Sternum
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
            pdf.Thoracic += customAppend(pdf.Thoracic, "T14", 30);
            pdf.Thoracic += customAppend(pdf.Thoracic, "T15", 31);
            pdf.Thoracic += customAppend(pdf.Thoracic, "T16", 32);
            pdf.Thoracic += customAppend(pdf.Thoracic, "T17", 33);
            pdf.Thoracic += customAppend(pdf.Thoracic, "T18", 34);
            pdf.Thoracic += customAppend(pdf.Thoracic, "Sternum", 89);
            pdf.Thoracic += customAppend(pdf.Thoracic, string.Empty, 16);

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
            pdf.Ribs += customAppend(pdf.Ribs, "R1", 35);
            pdf.Ribs += customAppend(pdf.Ribs, "R2", 36);
            pdf.Ribs += customAppend(pdf.Ribs, "R3", 37);
            pdf.Ribs += customAppend(pdf.Ribs, "R4", 38);
            pdf.Ribs += customAppend(pdf.Ribs, "R5", 39);
            pdf.Ribs += customAppend(pdf.Ribs, "R6", 40);
            pdf.Ribs += customAppend(pdf.Ribs, "R7", 41);
            pdf.Ribs += customAppend(pdf.Ribs, "R8", 42);
            pdf.Ribs += customAppend(pdf.Ribs, "R9", 43);
            pdf.Ribs += customAppend(pdf.Ribs, "R10", 44);
            pdf.Ribs += customAppend(pdf.Ribs, "R11", 45);
            pdf.Ribs += customAppend(pdf.Ribs, "R12", 46);
            pdf.Ribs += customAppend(pdf.Ribs, "R13", 47);
            pdf.Ribs += customAppend(pdf.Ribs, "R14", 48);
            pdf.Ribs += customAppend(pdf.Ribs, "R15", 49);
            pdf.Ribs += customAppend(pdf.Ribs, "R16", 50);
            pdf.Ribs += customAppend(pdf.Ribs, "R17", 51);
            pdf.Ribs += customAppend(pdf.Ribs, "R18", 52);

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
            pdf.Lumbar += customAppend(pdf.Lumbar, "L1", 54);
            pdf.Lumbar += customAppend(pdf.Lumbar, "L2", 55);
            pdf.Lumbar += customAppend(pdf.Lumbar, "L3", 56);
            pdf.Lumbar += customAppend(pdf.Lumbar, "L4", 57);
            pdf.Lumbar += customAppend(pdf.Lumbar, "L5", 58);
            pdf.Lumbar += customAppend(pdf.Lumbar, "L6", 59);
            pdf.Lumbar += customAppend(pdf.Lumbar, "L3/L4 Intertransverse", 60);
            pdf.Lumbar += customAppend(pdf.Lumbar, "L4/L5 Intertransverse", 61);
            pdf.Lumbar += customAppend(pdf.Lumbar, "L5/L6 Intertransverse", 62);
            pdf.Lumbar += customAppend(pdf.Lumbar, string.Empty, 53);

            // Sacrum
            // 63 BL Other notes
            // 64 BM Base
            // 65 BN Apex
            // Pelvic
            // 66 BO Other notes
            // 67 BP Left
            // 68 BQ Right
            // 90 CM Traction
            pdf.PelvicSacral += customAppend(pdf.PelvicSacral, "Base", 64);
            pdf.PelvicSacral += customAppend(pdf.PelvicSacral, "Apex", 65);
            pdf.PelvicSacral += customAppend(pdf.PelvicSacral, "Sacrum", 63);
            pdf.PelvicSacral += customAppend(pdf.PelvicSacral, "Left", 67);
            pdf.PelvicSacral += customAppend(pdf.PelvicSacral, "Right", 68);
            pdf.PelvicSacral += customAppend(pdf.PelvicSacral, string.Empty, 90);
            pdf.PelvicSacral += customAppend(pdf.PelvicSacral, "Pelvis", 66);

            // Left forelimb
            // 69 BR Other notes
            // 70 BS Scapula
            // 71 BT Humorous
            // 72 BU Ulna
            // 73 BV Radius
            // 74 BW Carpus
            // 75 BX Metatarsals/Phalanges
            pdf.LeftForelimb += customAppend(pdf.LeftForelimb, "Scapula", 70);
            pdf.LeftForelimb += customAppend(pdf.LeftForelimb, "Humorous", 71);
            pdf.LeftForelimb += customAppend(pdf.LeftForelimb, "Ulna", 72);
            pdf.LeftForelimb += customAppend(pdf.LeftForelimb, "Radius", 73);
            pdf.LeftForelimb += customAppend(pdf.LeftForelimb, "Carpus", 74);
            pdf.LeftForelimb += customAppend(pdf.LeftForelimb, "Metatarsals/Phalanges", 75);
            pdf.LeftForelimb += customAppend(pdf.LeftForelimb, string.Empty, 69);

            // Right forelimb
            // 76 BY Other notes
            // 77 BZ Scapula
            // 78 CA Humorous
            // 79 CB Ulna
            // 80 CC Radius
            // 81 CD Carpus
            // 82 CE Metatarsals/Phalanges
            pdf.RightForelimb += customAppend(pdf.RightForelimb, "Scapula", 77);
            pdf.RightForelimb += customAppend(pdf.RightForelimb, "Humorous", 78);
            pdf.RightForelimb += customAppend(pdf.RightForelimb, "Ulna", 79);
            pdf.RightForelimb += customAppend(pdf.RightForelimb, "Radius", 80);
            pdf.RightForelimb += customAppend(pdf.RightForelimb, "Carpus", 81);
            pdf.RightForelimb += customAppend(pdf.RightForelimb, "Metatarsals/Phalanges", 82);
            pdf.RightForelimb += customAppend(pdf.RightForelimb, string.Empty, 76);

            // Left rear limb
            // 83 CF Other notes
            // 84 CG Raw response
            pdf.LeftRearLimb += customAppend(pdf.LeftRearLimb, string.Empty, 84);
            pdf.LeftRearLimb += customAppend(pdf.LeftRearLimb, string.Empty, 83);

            // Right rear limb
            // 85 CH Other notes
            // 86 CI Raw response
            pdf.RightRearLimb += customAppend(pdf.RightRearLimb, string.Empty, 86);
            pdf.RightRearLimb += customAppend(pdf.RightRearLimb, string.Empty, 85);

            // Extended other notes
            // 87 CJ Raw response
            pdf.Other = row.Length > 87 ? row[87]?.Trim() : string.Empty;

            // Coccygeal
            // 88 CK Raw response
            // 91 CN Coccygeal
            pdf.Coccygeal += customAppend(pdf.Coccygeal, "Coccygeal", 91);
            pdf.Coccygeal += customAppend(pdf.Coccygeal, string.Empty, 88);


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

        private sealed class ChiroEquinePdfDocument : IDocument
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
                                .Image(Resources.ChiroEquineDiagram);

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
