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
    public sealed class InvoicePostlerPdfGenerationActivity : AsyncTaskActivity<long, string>
    {
        private readonly NsStorage nsStorage;
        private readonly IServiceProvider serviceProvider;
        private readonly Sentinals sentinals;
        private readonly ILogger<InvoicePostlerPdfGenerationActivity> logger;
        private readonly TelemetryClient telemetryClient;

        public InvoicePostlerPdfGenerationActivity(
            NsStorage nsStorage,
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<InvoicePostlerPdfGenerationActivity> logger,
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

            // Check if there's any invoices ready to be processed
            Invoice invoice = await nsContext.Invoices
                .Include(x => x.Business)
                .Include(x => x.Customer)
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == input && (x.ReadyForProcessing || x.ReprocessRegularly) && x.ProcessedAt == default, cancellationToken);
            if (invoice == default)
            {
                throw new InvalidOperationException("Could not find invoice that was ready for processing.");
            }

            // Add the email request
            string invoiceId = context.OrchestrationInstance.InstanceId.Split('_').First();
            string title = string.IsNullOrEmpty(invoice.Title) ? $"Invoice #{invoiceId}" : invoice.Title;
            string subject = $"[Invoice {invoiceId}] {invoice.Customer.Name} - {title}";
            if (subject.Length > 128)
            {
                subject = subject.Substring(0, 128);
            }
            string attachmentName = new($"{invoiceId}_{invoice.Business.Name}_{invoice.Customer.Name}_{title}".Select(x => char.IsAsciiLetterOrDigit(x) || x == '_' ? x : '-').ToArray());
            if (attachmentName.Length > 128)
            {
                attachmentName = string.Concat(attachmentName.AsSpan(0, 124), ".pdf");
            }
            StringBuilder additionalBody = new();
            _ = additionalBody.AppendLine($"Invoice id: {invoiceId}");
            _ = additionalBody.AppendLine($"Business name: {invoice.Business.Name}");
            _ = additionalBody.AppendLine($"Customer name: {invoice.Customer.Name}");
            _ = additionalBody.AppendLine($"Due date: {invoice.DueDate:MMMM dd, yyyy}");
            _ = additionalBody.AppendLine($"Total due: $ {invoice.Total:N2}");
            _ = additionalBody.AppendLine($"Line count: {invoice.Lines.Count}");
            EmailDocument emailDocument = new()
            {
                Id = context.OrchestrationInstance.InstanceId,
                To = invoice.Business.Contact,
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
                InvoicePdfDocument pdfDocument = new(invoice);
                pdfDocument.GeneratePdf(ms);
                ms.Position = 0;
            }

            // Put it in storage
            Azure.Storage.Blobs.BlobClient pdfBlobClient = this.nsStorage.GetBlobClient(emailDocument);
            this.logger.LogInformation($"Uploading to {pdfBlobClient.Uri.AbsoluteUri.Split('?').First()}");
            _ = await pdfBlobClient.UploadAsync(ms, overwrite: true, cancellationToken);

            // And save it to enable processing
            invoice.ProcessedAt = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);
            return default;
        }

        private sealed class InvoicePdfDocument : IDocument
        {
            private readonly Invoice invoice;

            public InvoicePdfDocument(Invoice invoice)
            {
                this.invoice = invoice;
            }

            public void Compose(IDocumentContainer container)
            {
                string invoiceIdFormat = $"Invoice #{this.invoice.Id.ToString().PadLeft(4, '0')}-{this.invoice.ProccessAttempt.ToString().PadLeft(2, '0')}";
                string invoiceTitle = string.IsNullOrWhiteSpace(this.invoice.Title)
                    ? invoiceIdFormat
                    : this.invoice.Title;
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
                                            .Image(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAFAAAABmBAMAAACuHAsuAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAwUExURQEBARkZGScnJzc3N0ZGRlVVVWZmZnl5eYiIiJqamqenp7e3t8nJydjY2Onp6f///yYeumgAAAAJcEhZcwAADsIAAA7CARUoSoAAAANMSURBVFjD7ddNaNNQHADwNOvH5jZopuBh69gqHkTZKqgXcR+QOTxZcBUUtcoO3lxhSlGR7TgPut3cBrO7KAhqxYOOuZnJLoLMinhR7IqKkzGbh5s2s23yTJaXLGk+XkRlF/+Fkr7+eH3/95L3fyWgw/hHUHhiiKm5eRNYIIzhqtl76aUTKAV5LOMMEsSWm/ox3vZbSfK6PuvFkKVM6KdnQfmCCm4TX5pf8AD9PLbJzQfmWSmyE92q3KWHM3LrYeWz8FwZjSulgyslUBx3A+qyTgdzBqiOuwxo4U8jhP1I9mhh3gSuIliphQUTCE/LjW4tLJrBT6hLBgcL64Nch34TqCxDExaOq9lg4EcZ+rAwp6aNgWgZSICDaC5cGRwU0I2ZwsKQQ6hMJIOF4Q2HKYcQP48oaxdwOI9l0OHKeKDDtS7HQvRsVjq9H/GPAnoM8Q/XA2VhcFDeAaTNxx7ycmMFxMEfai4YOCTDBA4W5DZpj7KHz+QOAziY96uTYwv5sLrQtrB4RnbEoC0UHivVowKawv1SsZ4cvrAbdUeQTAnkzeum6xxU4eKIFDdMHeEZHRlV4Jd4fMAQw8pFPH5R89PDA1dT2pmG+VOwJGTYQPm9mkYAuWpzGEy/DyXVNq7FGor3clRty9XbwZkovLWPgXz3oWKv94oI80eOw8K184weAj6c+Ey2l8PXbv8ditwpwlhnWx9H1iRLk/HA2KAQZvoTiyAXAFx1vhxyVZybLcn66NmksBnCV9GhE0Ae4/e69JKPqzSOEfLiBC03LxBbwRpccVGUV5OTMkYRijvRh1b4tq1HhrUvpuZMIWzPwPG+LPzWvAZzVVDImMOZHRMkiF2ORTnfNFfNU2MPmwxwj/RWCBEn4SzhzhQbNnEB8aqMWQ1oocCyaen0BJbuAjY7+ZRl3937ep9lJ6fZ7Nq5SoFvaDpC012NVDDYSG2PiNeRSEeE7hCvOrpomj6o6VGMJflGbWGNoRkjtKwKJcn8hxsGW/GQ+D3YjIXo8FnvFAawEJXwKixc1W3tNhDVei8WLqMiAHAQnY9R+bGGyomSqMXAWbWwJO2g8Gi9HLnHrKHQq61Brk5gBUv/QDJ/Dil9pCyTSeti3jIZJ/G3IYS/APYaOa+TCWSrAAAAAElFTkSuQmCC"));
                                    });

                                    // Business info
                                    row.RelativeItem().Column(column =>
                                    {
                                        _ = column.Item()
                                            .AlignCenter()
                                            .Text(this.invoice.Business.Name)
                                            .Bold();
                                        _ = column.Item()
                                            .AlignCenter()
                                            .Text(this.invoice.Business.AddressLine1);
                                        _ = column.Item()
                                            .AlignCenter()
                                            .Text(this.invoice.Business.AddressLine2);
                                        _ = column.Item()
                                            .AlignCenter()
                                            .Text(this.invoice.Business.Contact);
                                    });

                                    // Invoice info
                                    row.RelativeItem().Column(column =>
                                    {
                                        _ = column.Item()
                                            .AlignRight()
                                            .Text(invoiceIdFormat)
                                            .Bold();
                                        _ = column.Item()
                                            .Text(string.Empty);
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
                                                _ = text.Span("Due date: ")
                                                    .Bold();
                                                _ = text.Span(this.invoice.DueDate.ToString("MMMM dd, yyyy"));
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
                            // Invoice title
                            _ = column.Item()
                                .Text(invoiceTitle)
                                .FontSize(baseFontSize * 2)
                                .Bold();

                            // Invoice description
                            _ = column.Item()
                                .Text(this.invoice.Description);

                            // Invoice summary. 3-column 'table' at the top under the title and description.
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
                                            .Text(this.invoice.Customer.Name);
                                        _ = column.Item()
                                            .Text(this.invoice.Customer.Contact);
                                        _ = column.Item()
                                            .Text(this.invoice.Customer.AddressLine1);
                                        _ = column.Item()
                                            .Text(this.invoice.Customer.AddressLine2);
                                    });

                                    // Invoice Details
                                    row.RelativeItem().Column(column =>
                                    {
                                        _ = column.Item()
                                            .Text("Invoice Details")
                                            .Bold();
                                        _ = column.Item()
                                            .Text($"PDF created {DateTime.Now:MMMM dd, yyyy}.");
                                        _ = column.Item()
                                            .Text($"Total: ${this.invoice.Total:N2}.");
                                        _ = column.Item()
                                            .Text($"Line count: {this.invoice.Lines.Count:N0}.");
                                    });

                                    // Payment
                                    row.RelativeItem().Column(column =>
                                    {
                                        _ = column.Item()
                                            .Text("Payment")
                                            .Bold();
                                        _ = column.Item()
                                            .Text($"Due {this.invoice.DueDate:MMMM dd, yyyy}.");
                                        _ = column.Item()
                                            .Text(string.Empty)
                                            .FontSize(baseFontSize * .5f);
                                        _ = column.Item()
                                            .Text("Accepted forms of payment:");
                                        _ = column.Item()
                                            .Text("Cash, Check (payable to Tom Postler), or Cash Equivalents (PayPal, Google Pay, Venmo, Zelle, etc.). Credit cards may be accepted. Inquire if questions.")
                                            .FontSize(baseFontSize * .8f);
                                    });
                                });

                            // Invoice lines
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(9);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                // Header, will be repeated on every page
                                table.Header(header =>
                                {
                                    _ = header.Cell()
                                        .Text("Id")
                                        .Bold();
                                    _ = header.Cell()
                                        .Text("Item")
                                        .Bold();
                                    _ = header.Cell()
                                        .Text("Quantity")
                                        .AlignRight()
                                        .Bold();
                                    _ = header.Cell()
                                        .Text("Price")
                                        .AlignRight()
                                        .Bold();
                                    _ = header.Cell()
                                        .Text("Amount")
                                        .AlignRight()
                                        .Bold();
                                    _ = header.Cell()
                                        .ColumnSpan(5)
                                        .PaddingVertical(4)
                                        .BorderBottom(0.5f)
                                        .BorderColor(Colors.Black);
                                });

                                // Individual lines. Make sure to get correct width every 'row', else table will misalign.
                                foreach (InvoiceLine line in this.invoice.Lines)
                                {
                                    static IContainer CellStyle(IContainer container)
                                        => container.PaddingVertical(3);
                                    static IContainer RightCellStyle(IContainer container)
                                        => container.PaddingVertical(3).AlignRight();

                                    // Line id
                                    _ = table.Cell().Element(CellStyle).Text(line.Id.ToString()).FontColor(Colors.Grey.Medium);

                                    // Line item
                                    table.Cell().Element(CellStyle).Text(text =>
                                    {
                                        _ = text.Line(line.Title);
                                        _ = text.Span(line.Description)
                                            .FontColor(Colors.Grey.Medium)
                                            .Italic();
                                    });

                                    // Line quantity
                                    if (string.IsNullOrWhiteSpace(line.Unit))
                                    {
                                        _ = table.Cell().Element(RightCellStyle).Text($"{line.Quantity:N2}");
                                    }
                                    else
                                    {
                                        // We have a unit and should format the quantity differently
                                        _ = table.Cell().Element(RightCellStyle).Text($"{line.Quantity:N2} {line.Unit}");
                                    }

                                    // Line price
                                    table.Cell().Element(RightCellStyle).Text(text =>
                                    {
                                        _ = text.Span(line.Price.ToString("N2"));
                                        if (!string.IsNullOrWhiteSpace(line.Unit))
                                        {
                                            _ = text.Span("/");
                                            _ = text.Span(line.Unit);
                                        }
                                    });

                                    // Line amount
                                    table.Cell().Element(RightCellStyle).Text(text =>
                                    {
                                        _ = text.Span("$ ");
                                        if (line.Price < 0)
                                        {
                                            _ = text.Span("(");
                                        }
                                        _ = text.Span(Math.Abs(line.Quantity * line.Price).ToString("N2"));
                                        if (line.Price < 0)
                                        {
                                            _ = text.Span(")");
                                        }
                                    });
                                }
                            });

                            _ = column.Item().PaddingVertical(10).LineHorizontal(0.5f);

                            // Total due
                            column.Item()
                                .DefaultTextStyle(TextStyle.Default.FontSize(baseFontSize * 1.5f).Bold())
                                .Row(row =>
                                {
                                    row.RelativeItem().Column(column =>
                                    {
                                        _ = column.Item().Text("Total Due");
                                    });
                                    row.RelativeItem().AlignRight().Column(column =>
                                    {
                                        _ = column.Item().Text($"${this.invoice.Total:N2}");
                                    });
                                });

                            // Terms
                            column.Item()
                                .DefaultTextStyle(TextStyle.Default.FontSize(baseFontSize * .8f))
                                .ExtendVertical()
                                .AlignBottom()
                                .Text(text =>
                                {
                                    _ = text.Span("Invoice numbering").Bold();
                                    _ = text.Span(" is of the form id-version.");
                                    _ = text.Span(" For any given invoice id, there may be multiple versions as additional lines are added or other modifications are necessary.");
                                    _ = text.Span(" Unless otherwise noted, the largest numerical version for a specific id is to be considered the official invoice and all prior versions discarded.");
                                    _ = text.Line(string.Empty);

                                    _ = text.Span("Cash discounts and credit surcharges").Bold();
                                    _ = text.Span(" will not be added.");
                                    _ = text.Span(" Cash or cash equivalent payment methods still require handling to be processed.");
                                    _ = text.Span(" Credit processing incurs direct fees roughly equivalent to cash handling");
                                    _ = text.Line(string.Empty);

                                    _ = text.Span("Finance charges").Bold();
                                    _ = text.Span(" will accrue on unpaid, overdue invoices.");
                                    _ = text.Span(" The rate of 2% per month, which is an approximate 24% APR (annual percentage rate), will be applied to any remaining balance on the 1st of every month following the due date until the invoice is paid in full.");
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
                                            .Text(invoiceTitle);
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
