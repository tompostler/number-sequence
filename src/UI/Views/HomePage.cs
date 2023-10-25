using Microsoft.Extensions.Logging;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Tool;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.UI.Views;

public class HomePage : ContentPage
{
    private readonly NsTcpWtfClient client;

    private readonly Editor body;

    const int statusLineLimit = 10;
    private readonly List<string> statusTextLines = Enumerable.Repeat(string.Empty, statusLineLimit).ToList();
    private readonly Editor status;

    /// <summary>
    /// Consists of a grid with three rows:
    /// 1. FlexLayout of all the primary button navigation.
    /// 2. Body Entry for simple display data.
    /// 3. Status logging Entry snapped to the bottom.
    /// </summary>
    public HomePage()
    {
        this.Title = "Home";

        // Overall page grid
        var gLayout = new Grid
        {
            RowDefinitions =
            {
                new(GridLength.Auto),
                new(GridLength.Star),
                new(GridLength.Auto),
            }
        };
        this.Content = gLayout;

        // First row of buttons
        var fLayout = new FlexLayout
        {
            AlignContent = Microsoft.Maui.Layouts.FlexAlignContent.Start,
            AlignItems = Microsoft.Maui.Layouts.FlexAlignItems.Start,
            Direction = Microsoft.Maui.Layouts.FlexDirection.Row,
            Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap,
        };
        gLayout.Add(fLayout);

        void addButton(string text)
        {
            Button button = new()
            {
                Text = text,

                MinimumWidthRequest = 150,
                MaximumWidthRequest = 150,
            };
            button.Clicked += this.Button_Clicked;
            fLayout.Add(button);
        }

        addButton("Account (show)");
        addButton("Count (list)");
        addButton("Invoice (list)");
        addButton("IP");
        addButton("LaTeX Status");
        addButton("Ping");
        addButton("Ping (authed)");
        addButton("Ping (roled)");
        addButton("Random");
        addButton("Token (show)");

        // Middle row of table
        this.body = new()
        {
            BackgroundColor = Colors.LightGray,
            FontFamily = "Hack",
            IsReadOnly = true,
        };
        gLayout.Add(this.body, row: 1);

        // Last row of status
        this.status = new()
        {
            BackgroundColor = Colors.Black,
            TextColor = Colors.LightGray,
            FontFamily = "Hack",
            IsReadOnly = true,
        };
        this.status.FontSize *= 0.75;
        this.UpdateStatus();
        gLayout.Add(this.status, row: 2);

        this.client = new NsTcpWtfClient(new ClientListLogger(this.UpdateStatus), TokenProvider.GetAsync);
    }

    private void UpdateStatus(string newLine = default)
    {
        if (newLine != default)
        {
            this.statusTextLines.Add(newLine);
        }

        while (this.statusTextLines.Count > statusLineLimit)
        {
            this.statusTextLines.RemoveAt(0);
        }
        this.status.Text = string.Join('\n', this.statusTextLines);
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        string clicked = new((sender as Button).Text.ToLower().Where(x => char.IsLetter(x)).ToArray());
        this.UpdateStatus($"Clicked {clicked}");

        // Wipe any data displayed in the body.
        this.body.Text = null;
        int bufferWidth = (int)(this.Width / this.body.FontSize * 1.6);

        switch (clicked)
        {
            case "accountshow":
                this.body.Text = (await this.client.Account.GetAsync(TokenProvider.GetAccount())).ToJsonString(indented: true);
                break;

            case "countlist":
                List<Count> counts = await this.client.Count.ListAsync();
                this.body.Text = Output.WriteTable(
                    counts,
                    bufferWidth,
                    nameof(Count.Name),
                    nameof(Count.Value),
                    nameof(Count.CreatedDate),
                    nameof(Count.ModifiedDate));
                break;

            case "ip":
                this.body.Text = await this.client.Ping.GetPublicIpAsync();
                break;

            case "invoicelist":
                List<Contracts.Invoicing.Invoice> invoices = await this.client.Invoice.GetAsync();
                this.body.Text = Output.WriteTable(
                    invoices.Select(x => new
                    {
                        x.Id,
                        x.Title,
                        BusinessName = x.Business.Name,
                        CustomerName = x.Customer.Name,
                        x.DueDate,
                        x.PaidDate,
                        x.Total,
                        x.CreatedDate,
                        x.ModifiedDate,
                        x.ProcessedAt,
                        x.ProccessAttempt,
                        x.ReprocessRegularly,
                    }),
                    bufferWidth,
                    nameof(Contracts.Invoicing.Invoice.Id),
                    nameof(Contracts.Invoicing.Invoice.Title),
                    nameof(Contracts.Invoicing.Invoice.Business) + nameof(Contracts.Invoicing.Invoice.Business.Name),
                    nameof(Contracts.Invoicing.Invoice.Customer) + nameof(Contracts.Invoicing.Invoice.Customer.Name),
                    nameof(Contracts.Invoicing.Invoice.DueDate),
                    nameof(Contracts.Invoicing.Invoice.PaidDate),
                    nameof(Contracts.Invoicing.Invoice.Total),
                    nameof(Contracts.Invoicing.Invoice.CreatedDate),
                    nameof(Contracts.Invoicing.Invoice.ModifiedDate),
                    nameof(Contracts.Invoicing.Invoice.ProcessedAt),
                    nameof(Contracts.Invoicing.Invoice.ProccessAttempt),
                    nameof(Contracts.Invoicing.Invoice.ReprocessRegularly));
                break;

            case "latexstatus":
                LatexStatus latexStatus = await this.client.LatexStatus.GetAsync();
                this.body.Text = nameof(LatexStatus.LatexTemplateSpreadsheetRows);
                this.body.Text += '\n';
                this.body.Text += Output.WriteTable(
                    latexStatus.LatexTemplateSpreadsheetRows,
                    bufferWidth,
                    nameof(LatexStatus.LatexTemplateSpreadsheetRow.RowId),
                    nameof(LatexStatus.LatexTemplateSpreadsheetRow.LatexDocumentId),
                    nameof(LatexStatus.LatexTemplateSpreadsheetRow.CreatedDate));
                this.body.Text += '\n';
                this.body.Text += nameof(LatexStatus.LatexDocuments);
                this.body.Text += '\n';
                this.body.Text += Output.WriteTable(
                    latexStatus.LatexDocuments,
                    bufferWidth,
                    nameof(LatexStatus.LatexDocument.Id),
                    nameof(LatexStatus.LatexDocument.CreatedDate),
                    nameof(LatexStatus.LatexDocument.ProcessedAt),
                    nameof(LatexStatus.LatexDocument.Delay),
                    nameof(LatexStatus.LatexDocument.Successful));
                this.body.Text += '\n';
                this.body.Text += nameof(LatexStatus.EmailLatexDocuments);
                this.body.Text += '\n';
                this.body.Text += Output.WriteTable(
                    latexStatus.EmailLatexDocuments,
                    bufferWidth,
                    nameof(LatexStatus.EmailLatexDocument.Id),
                    nameof(LatexStatus.EmailLatexDocument.Subject),
                    nameof(LatexStatus.EmailLatexDocument.AttachmentName),
                    nameof(LatexStatus.EmailLatexDocument.CreatedDate),
                    nameof(LatexStatus.EmailLatexDocument.ProcessedAt),
                    nameof(LatexStatus.EmailLatexDocument.Delay));
                break;

            case "ping":
                await this.client.Ping.SendAsync();
                break;

            case "pingauthed":
                await this.client.Ping.SendWithAuthAsync();
                break;

            case "pingroled":
                await this.client.Ping.SendWithAuthToRoleAsync();
                break;

            case "tokenshow":
                this.body.Text = TokenProvider.GetValue().ToJsonString(indented: true);
                break;

            default:
                await this.DisplayAlert(title: "Not Implemented", message: "The button you clicked is not implemented.", cancel: "OK");
                break;
        }
    }

    private sealed class ClientListLogger : ILogger<NsTcpWtfClient>
    {
        private readonly Action<string> addLine;

        public ClientListLogger(Action<string> addLine)
        {
            this.addLine = addLine;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => throw new NotImplementedException();
        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel < LogLevel.Information)
            {
                return;
            }

            // If an exception is passed in here, it is currently ignored by the LoggerExtensions default formatter
            // Line 509: https://github.com/aspnet/Logging/blob/dev/src/Microsoft.Extensions.Logging.Abstractions/LoggerExtensions.cs
            // Because people may expect it to be logged, go ahead and log it here as an error (but as a concatenation to the current message)
            string message = formatter(state, exception);
            if (exception != null)
            {
                message += '\n' + exception.ToString();
            }

            string prefix = logLevel switch
            {
                LogLevel.Trace => "TRAC",
                LogLevel.Debug => "DEBG",
                LogLevel.Information => "INFO",
                LogLevel.Warning => "WARN",
                LogLevel.Error => "ERRO",
                LogLevel.Critical => "CRIT",
                _ => logLevel.ToString().ToUpper()
            };
            message = $"[NS {prefix}] {message}";

            this.addLine(message);
        }
    }

    // Do not allow the back button to do anything.
    protected override bool OnBackButtonPressed() => true;
}
