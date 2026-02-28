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
using TcpWtf.NumberSequence.Contracts.Invoicing;
using Unlimitedinf.Utilities.Extensions;

namespace number_sequence.DurableTaskImpl.Activities
{
    public sealed class StatementPostlerPdfGenerationActivity : AsyncTaskActivity<long, string>
    {
        private readonly NsStorage nsStorage;
        private readonly IServiceProvider serviceProvider;
        private readonly Sentinals sentinals;
        private readonly ILogger<StatementPostlerPdfGenerationActivity> logger;
        private readonly TelemetryClient telemetryClient;

        public StatementPostlerPdfGenerationActivity(
            NsStorage nsStorage,
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<StatementPostlerPdfGenerationActivity> logger,
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
                .Include(x => x.Customer)
                .Include(x => x.Invoices)
                    .ThenInclude(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == input && x.ReadyForProcessing && x.ProcessedAt == default, cancellationToken);
            if (statement == default)
            {
                throw new InvalidOperationException("Could not find statment that was ready for processing.");
            }

            // Add the email request
            string statementId = context.OrchestrationInstance.InstanceId.Split('_').First();
            string title = $"Statement #{statementId}";
            string subject = $"[Statement {statementId}] {statement.Customer.Name} - {title}";
            if (subject.Length > 128)
            {
                subject = subject.Substring(0, 128);
            }
            string attachmentName = new($"{statementId}_{statement.Business.Name}_{statement.Customer.Name}_{title}".Select(x => char.IsAsciiLetterOrDigit(x) || x == '_' ? x : '-').ToArray());
            if (attachmentName.Length > 128)
            {
                attachmentName = string.Concat(attachmentName.AsSpan(0, 124), ".pdf");
            }
            StringBuilder additionalBody = new();
            _ = additionalBody.AppendLine($"Statement id: {statementId}");
            _ = additionalBody.AppendLine($"Business name: {statement.Business.Name}");
            _ = additionalBody.AppendLine($"Customer name: {statement.Customer.Name}");
            _ = additionalBody.AppendLine($"Start date: {statement.InvoiceStartDate:MMMM dd, yyyy}");
            _ = additionalBody.AppendLine($"End due: {statement.InvoiceEndDate:MMMM dd, yyyy}");
            _ = additionalBody.AppendLine($"Total billed: ${statement.TotalBilled:N2}");
            _ = additionalBody.AppendLine($"Total paid: ${statement.TotalPaid:N2}");
            _ = additionalBody.AppendLine($"Amount remaining: ${statement.TotalBilled - statement.TotalPaid:N2}");
            _ = additionalBody.AppendLine($"Invoice count: {statement.Invoices.Count:N0}");
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
                string statementTitle = $"Statement #{this.statement.Id.ToString().PadLeft(4, '0')}-{this.statement.ProccessAttempt.ToString().PadLeft(2, '0')}";
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
                                            .Width(30)
                                            .Image(Resources.InvoicePostlerLogo);
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
                                            .Text($"Paid: ${this.statement.TotalPaid:N2}. Billed: ${this.statement.TotalBilled:N2}.");
                                        _ = column.Item()
                                            .Text($"Amount remaining: ${this.statement.TotalBilled - this.statement.TotalPaid:N2}.");
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
                                    columns.RelativeColumn(8);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(3);
                                });

                                // Header, will be repeated on every page
                                table.Header(header =>
                                {
                                    _ = header.Cell()
                                        .Text("Id")
                                        .Bold();
                                    _ = header.Cell()
                                        .Text("Invoice")
                                        .Bold();
                                    _ = header.Cell()
                                        .Text("Created")
                                        .AlignRight()
                                        .Bold();
                                    _ = header.Cell()
                                        .Text("Billed")
                                        .AlignRight()
                                        .Bold();
                                    _ = header.Cell()
                                        .Text("Paid")
                                        .AlignRight()
                                        .Bold();
                                    _ = header.Cell()
                                        .ColumnSpan(4)
                                        .PaddingVertical(4)
                                        .BorderBottom(0.5f)
                                        .BorderColor(Colors.Black);
                                });

                                // Individual invoices. Make sure to get correct width every 'row', else table will misalign.
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

                                    // Line item
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

                                    // Invoice paid date
                                    table.Cell().Element(RightCellStyle).Text(text =>
                                    {
                                        _ = invoice.PaidDate.HasValue
                                            ? text.Span($"{invoice.PaidDate:MMM dd, yyyy}")
                                            : text.Span("-");
                                    });
                                }
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
