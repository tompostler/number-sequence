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
using System.Text.Json;

namespace number_sequence.DurableTaskImpl.Activities
{
    public sealed class ChiroCaninePdfGenerationActivity : AsyncTaskActivity<string, string>
    {
        private readonly NsStorage nsStorage;
        private readonly IServiceProvider serviceProvider;
        private readonly Sentinals sentinals;
        private readonly ILogger<ChiroCaninePdfGenerationActivity> logger;
        private readonly TelemetryClient telemetryClient;

        public ChiroCaninePdfGenerationActivity(
            NsStorage nsStorage,
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<ChiroCaninePdfGenerationActivity> logger,
            TelemetryClient telemetryClient)
        {
            this.nsStorage = nsStorage;
            this.serviceProvider = serviceProvider;
            this.sentinals = sentinals;
            this.logger = logger;
            this.telemetryClient = telemetryClient;
        }

        protected override async Task<string> ExecuteAsync(TaskContext context, string rowId)
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

            // Fetch the record and check for double-processing.
            ChiroRecord record = await nsContext.ChiroRecords.SingleAsync(x => x.RowId == rowId, cancellationToken);
            if (record.ProcessedAt.HasValue)
            {
                throw new InvalidOperationException($"ChiroRecord {record.RowId} was already processed at {record.ProcessedAt:u}");
            }

            ChiroInput chiroInput = JsonSerializer.Deserialize<ChiroInput>(record.InputJson);
            this.logger.LogInformation($"Processing ChiroRecord {record.RowId} for {chiroInput.PatientName} / {chiroInput.OwnerName}.");

            // Build the owner name (clinic prefix applied here before PDF generation).
            string ownerName = chiroInput.OwnerName;
            if (!string.IsNullOrWhiteSpace(chiroInput.ClinicAbbreviation))
            {
                ownerName = chiroInput.ClinicAbbreviation + " - " + ownerName;
            }

            // Generate the PDF
            ChiroCaninePdfDocument pdf = new(chiroInput, ownerName);
            MemoryStream ms = new();
            using (IDisposable disposable = this.logger.BeginScope("Generating PDF"))
            {
                Settings.License = LicenseType.Community;
                pdf.GeneratePdf(ms);
                ms.Position = 0;
            }

            // Add the email request
            string subject = $"[Chiro - Canine] {ownerName} - {chiroInput.PatientName}";
            string attachmentName = new(
                $"{chiroInput.DateOfService:yyyy-MM-dd}_{ownerName}_{chiroInput.PatientName}"
                .Select(x => (char.IsLetterOrDigit(x) || x == '-' || x == '_') ? x : '-')
                .ToArray());
            EmailDocument emailDocument = new()
            {
                Id = context.OrchestrationInstance.InstanceId,
                To = chiroInput.ToEmail,
                CC = string.Join(';', chiroInput.CcEmails),
                Subject = subject,
                AttachmentName = attachmentName,
            };
            _ = nsContext.EmailDocuments.Add(emailDocument);

            // Optionally add the batch email request.
            if (!string.IsNullOrWhiteSpace(chiroInput.ClinicAbbreviation))
            {
                _ = nsContext.ChiroEmailBatches.Add(new()
                {
                    Id = context.OrchestrationInstance.InstanceId,
                    ClinicAbbreviation = chiroInput.ClinicAbbreviation,
                    AttachmentName = attachmentName,
                });
            }

            // Put it in storage
            Azure.Storage.Blobs.BlobClient pdfBlobClient = this.nsStorage.GetBlobClient(emailDocument);
            this.logger.LogInformation($"Uploading to {pdfBlobClient.Uri.AbsoluteUri.Split('?').First()}");
            _ = await pdfBlobClient.UploadAsync(ms, overwrite: true, cancellationToken);

            record.ProcessedAt = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);
            return default;
        }

        private sealed class ChiroCaninePdfDocument : IDocument
        {
            public string PatientName { get; }
            public string OwnerName { get; }
            public DateTimeOffset DateOfService { get; }

            public string Head { get; } = string.Empty;
            public string Cervical { get; } = string.Empty;
            public string Thoracic { get; } = string.Empty;
            public string Ribs { get; } = string.Empty;
            public string Lumbar { get; } = string.Empty;
            public string PelvicSacral { get; } = string.Empty;
            public string LeftForelimb { get; } = string.Empty;
            public string RightForelimb { get; } = string.Empty;
            public string LeftRearLimb { get; } = string.Empty;
            public string RightRearLimb { get; } = string.Empty;
            public string Coccygeal { get; } = string.Empty;
            public string Other { get; } = string.Empty;

            public ChiroCaninePdfDocument(ChiroInput input, string ownerName)
            {
                static string append(string existing, string prefix, string value) =>
                    !string.IsNullOrWhiteSpace(value)
                        ? (string.IsNullOrWhiteSpace(existing) ? string.Empty : ", ") + $"{prefix} {value}".Trim()
                        : string.Empty;

                this.PatientName = input.PatientName;
                this.OwnerName = ownerName;
                this.DateOfService = input.DateOfService;

                string head = string.Empty;
                head += append(head, "Occiput", input.HeadOcciput);
                head += append(head, "TMJ", input.HeadTmj);
                head += append(head, string.Empty, input.HeadNotes);
                this.Head = head;

                string cervical = string.Empty;
                for (int i = 0; i < input.Cervical.Length; i++)
                {
                    cervical += append(cervical, $"C{i + 1}", input.Cervical[i]);
                }

                cervical += append(cervical, string.Empty, input.CervicalNotes);
                this.Cervical = cervical;

                string thoracic = string.Empty;
                for (int i = 0; i < input.Thoracic.Length; i++)
                {
                    thoracic += append(thoracic, $"T{i + 1}", input.Thoracic[i]);
                }

                thoracic += append(thoracic, "Sternum", input.Sternum);
                thoracic += append(thoracic, string.Empty, input.ThoracicNotes);
                this.Thoracic = thoracic;

                string ribs = string.Empty;
                for (int i = 0; i < input.Ribs.Length; i++)
                {
                    ribs += append(ribs, $"R{i + 1}", input.Ribs[i]);
                }

                this.Ribs = ribs;

                string lumbar = string.Empty;
                for (int i = 0; i < input.Lumbar.Length; i++)
                {
                    lumbar += append(lumbar, $"L{i + 1}", input.Lumbar[i]);
                }

                if (input.LumbarIntertransverse != null)
                {
                    lumbar += append(lumbar, "L3/L4 Intertransverse", input.LumbarIntertransverse[0]);
                    lumbar += append(lumbar, "L4/L5 Intertransverse", input.LumbarIntertransverse[1]);
                    lumbar += append(lumbar, "L5/L6 Intertransverse", input.LumbarIntertransverse[2]);
                }
                lumbar += append(lumbar, string.Empty, input.LumbarNotes);
                this.Lumbar = lumbar;

                string pelvicSacral = string.Empty;
                pelvicSacral += append(pelvicSacral, "Base", input.SacrumBase);
                pelvicSacral += append(pelvicSacral, "Apex", input.SacrumApex);
                pelvicSacral += append(pelvicSacral, "Sacrum", input.SacrumNotes);
                pelvicSacral += append(pelvicSacral, "Left", input.PelvicLeft);
                pelvicSacral += append(pelvicSacral, "Right", input.PelvicRight);
                pelvicSacral += append(pelvicSacral, string.Empty, input.PelvicTraction);
                pelvicSacral += append(pelvicSacral, "Pelvis", input.PelvicNotes);
                this.PelvicSacral = pelvicSacral;

                string leftForelimb = string.Empty;
                leftForelimb += append(leftForelimb, "Scapula", input.LeftForelimbScapula);
                leftForelimb += append(leftForelimb, "Humorous", input.LeftForelimbHumerus);
                leftForelimb += append(leftForelimb, "Ulna", input.LeftForelimbUlna);
                leftForelimb += append(leftForelimb, "Radius", input.LeftForelimbRadius);
                leftForelimb += append(leftForelimb, "Carpus", input.LeftForelimbCarpus);
                leftForelimb += append(leftForelimb, "Metatarsals/Phalanges", input.LeftForelimbMetatarsalsPhalanges);
                leftForelimb += append(leftForelimb, string.Empty, input.LeftForelimbNotes);
                this.LeftForelimb = leftForelimb;

                string rightForelimb = string.Empty;
                rightForelimb += append(rightForelimb, "Scapula", input.RightForelimbScapula);
                rightForelimb += append(rightForelimb, "Humorous", input.RightForelimbHumerus);
                rightForelimb += append(rightForelimb, "Ulna", input.RightForelimbUlna);
                rightForelimb += append(rightForelimb, "Radius", input.RightForelimbRadius);
                rightForelimb += append(rightForelimb, "Carpus", input.RightForelimbCarpus);
                rightForelimb += append(rightForelimb, "Metatarsals/Phalanges", input.RightForelimbMetatarsalsPhalanges);
                rightForelimb += append(rightForelimb, string.Empty, input.RightForelimbNotes);
                this.RightForelimb = rightForelimb;

                string leftRearLimb = string.Empty;
                leftRearLimb += append(leftRearLimb, string.Empty, input.LeftRearLimb);
                leftRearLimb += append(leftRearLimb, string.Empty, input.LeftRearLimbNotes);
                this.LeftRearLimb = leftRearLimb;

                string rightRearLimb = string.Empty;
                rightRearLimb += append(rightRearLimb, string.Empty, input.RightRearLimb);
                rightRearLimb += append(rightRearLimb, string.Empty, input.RightRearLimbNotes);
                this.RightRearLimb = rightRearLimb;

                string coccygeal = string.Empty;
                coccygeal += append(coccygeal, "Coccygeal", input.Coccygeal);
                coccygeal += append(coccygeal, string.Empty, input.CoccygealNotes);
                this.Coccygeal = coccygeal;

                this.Other = input.Other;
            }

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
                            // 2 columns in the header for the patient/client info and the clinic info.
                            column.Item().Row(row =>
                            {
                                // Paitent/client info.
                                row.RelativeItem().Column(column =>
                                {
                                    _ = column.Item()
                                        .Text($"Patient Name: {this.PatientName}");
                                    _ = column.Item()
                                        .Text($"Owner Name: {this.OwnerName}");
                                    _ = column.Item()
                                        .Text($"Date of Service: {this.DateOfService:MM/dd/yyyy}");
                                });

                                // Clinic logo/info.
                                row.RelativeItem().Column(column =>
                                {
                                    _ = column.Item()
                                        .AlignRight()
                                        .Height(baseFontSize * 4f)
                                        .Image(Resources.ChiroLogo);
                                });
                            });

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
