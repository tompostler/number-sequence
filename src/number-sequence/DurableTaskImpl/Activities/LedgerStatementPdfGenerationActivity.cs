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
using System.Text;
using TcpWtf.NumberSequence.Contracts.Ledger;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace number_sequence.DurableTaskImpl.Activities
{
    public sealed class LedgerStatementPdfGenerationActivity : AsyncTaskActivity<long, string>
    {
        private readonly NsStorage nsStorage;
        private readonly IServiceProvider serviceProvider;
        private readonly Sentinals sentinals;
        private readonly ILogger<LedgerStatementPdfGenerationActivity> logger;
        private readonly TelemetryClient telemetryClient;

        public LedgerStatementPdfGenerationActivity(
            NsStorage nsStorage,
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<LedgerStatementPdfGenerationActivity> logger,
            TelemetryClient telemetryClient)
        {
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

            // Check if there's any statements ready to be processed
            Statement statement = await nsContext.Statements
                .Include(x => x.Business)
                    .ThenInclude(x => x.Logo)
                .Include(x => x.Customer)
                .Include(x => x.Invoices)
                    .ThenInclude(x => x.Lines)
                .Include(x => x.Invoices)
                    .ThenInclude(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == input && x.ReadyForProcessing && x.ProcessedAt == default, cancellationToken);
            if (statement == default)
            {
                throw new InvalidOperationException("Could not find statment that was ready for processing.");
            }

            // Add the email request
            string friendlyId = statement.FriendlyId;
            string title = $"Statement #{friendlyId}";
            string subject = $"[Statement {friendlyId}] {statement.Customer.Name} - {title}";
            if (subject.Length > 128)
            {
                subject = subject.Substring(0, 128);
            }
            string attachmentName = new($"S{friendlyId}_{statement.Business.Name}_{statement.Customer.Name}_{title}".Select(x => char.IsAsciiLetterOrDigit(x) || x == '_' ? x : '-').ToArray());
            if (attachmentName.Length > 128)
            {
                attachmentName = string.Concat(attachmentName.AsSpan(0, 124), ".pdf");
            }
            StringBuilder additionalBody = new();
            _ = additionalBody.AppendLine($"Statement id: {friendlyId}.");
            _ = additionalBody.AppendLine($"Business name: {statement.Business.Name}.");
            _ = additionalBody.AppendLine($"Customer name: {statement.Customer.Name}.");
            _ = additionalBody.AppendLine($"Invoices by {(statement.SearchByDueDate ? "due" : "created")} date.");
            _ = additionalBody.AppendLine($"Start date: {statement.InvoiceStartDate:MMMM dd, yyyy}.");
            _ = additionalBody.AppendLine($"End due: {statement.InvoiceEndDate:MMMM dd, yyyy}.");
            _ = additionalBody.AppendLine($"Billed: ${statement.TotalBilled:N2}. Paid: ${statement.TotalPaid:N2}. Balance: ${statement.TotalBilled - statement.TotalPaid:N2}.");
            _ = additionalBody.AppendLine($"Invoice count: {statement.Invoices.Count:N0}.");
            EmailDocument emailDocument = new()
            {
                Id = context.OrchestrationInstance.InstanceId,
                To = statement.Business.Contact,
                Subject = subject,
                AttachmentName = attachmentName,
                AdditionalBody = additionalBody.ToString()
            };
            _ = nsContext.EmailDocuments.Add(emailDocument);
            this.logger.LogInformation($"Created email record: {emailDocument.ToJsonString()}");

            // Generate the PDF
            MemoryStream ms = new();
            using (IDisposable disposable = this.logger.BeginScope("Generating PDF"))
            {
                Settings.License = LicenseType.Community;
                StatementPdfDocument pdfDocument = new(statement);
                pdfDocument.GeneratePdf(ms);
                ms.Position = 0;
            }

            // Put it in storage
            Azure.Storage.Blobs.BlobClient pdfBlobClient = this.nsStorage.GetBlobClient(emailDocument);
            this.logger.LogInformation($"Uploading to {pdfBlobClient.Uri.AbsoluteUri.Split('?').First()}");
            _ = await pdfBlobClient.UploadAsync(ms, overwrite: true, cancellationToken);

            // And save it to enable processing
            statement.ProcessedAt = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);
            return default;
        }

        private sealed class StatementPdfDocument : IDocument
        {
            private readonly Statement statement;

            public StatementPdfDocument(Statement statement)
            {
                this.statement = statement;
            }

            public void Compose(IDocumentContainer container)
            {
                string statementTitle = $"Statement #{this.statement.FriendlyId}";
                const float baseFontSize = 10;

                _ = container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.MarginTop(0.4f, Unit.Inch);
                    page.MarginBottom(0.25f, Unit.Inch);
                    page.MarginHorizontal(0.5f, Unit.Inch);

                    page.DefaultTextStyle(TextStyle.Default.FontSize(baseFontSize));

                    // Header, repeated every page.
                    page.Header().Element(container =>
                    {
                        container.Column(column =>
                        {
                            column.Item()
                                .DefaultTextStyle(TextStyle.Default.FontSize(baseFontSize * .8f))
                                .Row(row =>
                                {
                                    // Logo
                                    row.RelativeItem().Column(column =>
                                    {
                                        _ = column.Item()
                                            .MaxHeight((baseFontSize + 1) * 4 * .8f)
                                            .AlignLeft()
                                            .AlignMiddle()
                                            .Image(this.statement.Business.Logo?.Data ?? new IdenticonGenerator() { Grayscale = true }.GeneratePng(this.statement.Business.Id.ToString(), 128))
                                            .FitArea();
                                    });

                                    // Business info
                                    row.RelativeItem().Column(column =>
                                    {
                                        _ = column.Item()
                                            .AlignCenter()
                                            .Text(this.statement.Business.Name)
                                            .Bold();
                                        _ = column.Item()
                                            .AlignCenter()
                                            .Text(this.statement.Business.AddressLine1);
                                        _ = column.Item()
                                            .AlignCenter()
                                            .Text(this.statement.Business.AddressLine2);
                                        _ = column.Item()
                                            .AlignCenter()
                                            .Text(this.statement.Business.Contact);
                                    });

                                    // Invoice info
                                    row.RelativeItem().Column(column =>
                                    {
                                        _ = column.Item()
                                            .AlignRight()
                                            .Text(statementTitle)
                                            .Bold();
                                        column.Item()
                                            .AlignRight()
                                            .Text(text =>
                                            {
                                                _ = text.Span("Issue date: ")
                                                    .Bold();
                                                _ = text.Span(DateTime.Now.ToString("MMMM dd, yyyy"));
                                            });
                                        column.Item()
                                            .AlignRight()
                                            .Text(text =>
                                            {
                                                _ = text.Span("Start date: ")
                                                    .Bold();
                                                _ = text.Span(this.statement.InvoiceStartDate.ToString("MMMM dd, yyyy"));
                                            });
                                        column.Item()
                                            .AlignRight()
                                            .Text(text =>
                                            {
                                                _ = text.Span("End date: ")
                                                    .Bold();
                                                _ = text.Span(this.statement.InvoiceEndDate.ToString("MMMM dd, yyyy"));
                                            });
                                    });
                                });

                            // Line separating header from content
                            column.Item()
                                .PaddingVertical(2)
                                .LineHorizontal(0.5f)
                                .LineColor(Colors.Grey.Darken3);
                        });
                    });

                    // Main content, can span as many pages as necessary.
                    page.Content().Element(container =>
                    {
                        container.PaddingVertical(10).Column(column =>
                        {
                            // Statement title
                            _ = column.Item()
                                .Text(statementTitle)
                                .FontSize(baseFontSize * 2)
                                .Bold();

                            // Statement summary. 3-column 'table' at the top under the title and description.
                            column.Item()
                                .PaddingVertical(30)
                                .PaddingHorizontal(15)
                                .DefaultTextStyle(TextStyle.Default.FontSize(baseFontSize * .95f))
                                .Row(row =>
                                {
                                    // Bill To
                                    row.RelativeItem().Column(column =>
                                    {
                                        _ = column.Item()
                                            .Text("Bill To")
                                            .Bold();
                                        _ = column.Item()
                                            .Text(this.statement.Customer.Name);
                                        _ = column.Item()
                                            .Text(this.statement.Customer.Contact);
                                        _ = column.Item()
                                            .Text(this.statement.Customer.AddressLine1);
                                        _ = column.Item()
                                            .Text(this.statement.Customer.AddressLine2);
                                    });

                                    // Statement Details
                                    row.RelativeItem().Column(column =>
                                    {
                                        _ = column.Item()
                                            .Text("Statement Details")
                                            .Bold();
                                        _ = column.Item()
                                            .Text($"PDF created {DateTime.Now:MMMM dd, yyyy}.");
                                        _ = column.Item()
                                            .Text($"Invoices by {(this.statement.SearchByDueDate ? "due" : "created")} date.");
                                        _ = column.Item()
                                            .Text($"Billed: ${this.statement.TotalBilled:N2}. Paid: ${this.statement.TotalPaid:N2} Balance: ${this.statement.TotalBilled - this.statement.TotalPaid:N2}.");
                                        _ = column.Item()
                                            .Text($"Invoice count: {this.statement.Invoices.Count:N0}.");
                                    });
                                });

                            // Statement lines
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(9);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                // Header, will be repeated on every page
                                table.Header(header =>
                                {
                                    _ = header.Cell()
                                        .Text("#")
                                        .Bold();
                                    _ = header.Cell()
                                        .Text("Description")
                                        .Bold();
                                    _ = header.Cell()
                                        .Text("Date")
                                        .AlignRight()
                                        .Bold();
                                    _ = header.Cell()
                                        .Text("Amount")
                                        .AlignRight()
                                        .Bold();
                                    _ = header.Cell()
                                        .ColumnSpan(4)
                                        .PaddingVertical(4)
                                        .BorderBottom(0.5f)
                                        .BorderColor(Colors.Black);
                                });

                                // Individual invoices with payment sub-rows. Make sure to get correct width every 'row', else table will misalign.
                                foreach (Invoice invoice in this.statement.Invoices)
                                {
                                    static IContainer CellStyle(IContainer container)
                                        => container.PaddingVertical(3);
                                    static IContainer RightCellStyle(IContainer container)
                                        => container.PaddingVertical(3).AlignRight();

                                    // Invoice id
                                    _ = table.Cell().Element(CellStyle).Text(invoice.Id.ToString())
                                        .FontColor(Colors.Grey.Medium)
                                        .FontSize(baseFontSize * 0.9f)
                                        .LetterSpacing(-0.05f);

                                    // Invoice description
                                    table.Cell().Element(CellStyle).Text(text =>
                                    {
                                        _ = text.Span("Invoice");
                                        _ = !string.IsNullOrWhiteSpace(invoice.Title)
                                            ? text.Line($" \"{invoice.Title}\" (id {invoice.Id:0000})")
                                            : text.Line($" id {invoice.Id:0000}");
                                        _ = text.Span(invoice.Description)
                                            .FontColor(Colors.Grey.Medium)
                                            .FontSize(baseFontSize * .9f)
                                            .Italic();
                                    });

                                    // Invoice created date
                                    _ = table.Cell().Element(RightCellStyle).Text($"{invoice.CreatedDate:MMM dd, yyyy}");

                                    // Invoice amount billed
                                    _ = table.Cell().Element(RightCellStyle).Text($"$ {invoice.Total:N2}");

                                    // Payment rows (one per payment, appearing after their invoice)
                                    foreach (InvoicePayment payment in invoice.Payments ?? [])
                                    {
                                        _ = table.Cell().Element(CellStyle).Text($"P{payment.Id}")
                                            .FontColor(Colors.Grey.Medium)
                                            .FontSize(baseFontSize * 0.9f)
                                            .LetterSpacing(-0.05f);

                                        table.Cell().Element(CellStyle).Text(text =>
                                        {
                                            _ = text.Line("Payment");
                                            if (!string.IsNullOrWhiteSpace(payment.Details))
                                            {
                                                _ = text.Span(payment.Details)
                                                    .FontColor(Colors.Grey.Medium)
                                                    .FontSize(baseFontSize * .9f)
                                                    .Italic();
                                            }
                                        });

                                        _ = table.Cell().Element(RightCellStyle).Text($"{payment.PaymentDate:MMM dd, yyyy}");

                                        table.Cell().Element(RightCellStyle).Text(text =>
                                        {
                                            _ = text.Span("$ ( ");
                                            _ = text.Span(payment.Amount.ToString("N2"));
                                            _ = text.Span(")");
                                        });
                                    }
                                }
                            });

                            // Statement totals
                            _ = column.Item().PaddingVertical(10).LineHorizontal(0.5f);

                            column.Item()
                                .DefaultTextStyle(TextStyle.Default.FontSize(baseFontSize * 1.2f))
                                .Row(row =>
                                {
                                    row.RelativeItem(10);
                                    row.RelativeItem(2).AlignRight().Column(column =>
                                    {
                                        _ = column.Item().Text("Total Billed");
                                        _ = column.Item().Text("Total Paid");
                                        _ = column.Item().Text("Balance").Bold();
                                    });
                                    row.RelativeItem(2).AlignRight().Column(column =>
                                    {
                                        _ = column.Item().Text($"${this.statement.TotalBilled:N2}");
                                        _ = column.Item().Text($"${this.statement.TotalPaid:N2}");
                                        _ = column.Item().Text($"${this.statement.TotalBilled - this.statement.TotalPaid:N2}").Bold();
                                    });
                                });

                        });
                    });

                    // Footer, repeated each page.
                    page.Footer().Element(container =>
                    {
                        container.Column(column =>
                        {
                            column.Item()
                                .DefaultTextStyle(TextStyle.Default.FontSize(baseFontSize * .8f))
                                .Row(row =>
                                {
                                    // Title
                                    row.RelativeItem().Column(column =>
                                    {
                                        _ = column.Item()
                                            .Text(statementTitle);
                                    });

                                    // Page of Page
                                    row.RelativeItem().Column(column =>
                                    {
                                        column.Item().Text(text =>
                                        {
                                            text.AlignRight();
                                            _ = text.CurrentPageNumber();
                                            _ = text.Span(" of ");
                                            _ = text.TotalPages();
                                        });
                                    });
                                });
                        });
                    });
                });
            }
        }
    }
}
