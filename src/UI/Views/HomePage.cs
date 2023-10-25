using Microsoft.Extensions.Logging;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Tool;

namespace TcpWtf.NumberSequence.UI.Views;

public class HomePage : ContentPage
{
    private readonly NsTcpWtfClient client;

    const int statusLineLimit = 10;
    private readonly List<string> statusTextLines = Enumerable.Repeat(string.Empty, statusLineLimit).ToList();
    private readonly Label status;

    public HomePage()
    {
        this.Title = "Home";
        var gLayout = new Grid
        {
            RowDefinitions =
            {
                new(GridLength.Star),
                new(GridLength.Auto),
            }
        };
        this.Content = gLayout;

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

        addButton("Account");
        addButton("Counts");
        addButton("Invoices");
        addButton("IP");
        addButton("LaTeX Status");
        addButton("Ping");
        addButton("Random");
        addButton("Tokens");

        this.status = new Label()
        {
            BackgroundColor = Colors.Black,
            TextColor = Colors.LightGray,
            FontFamily = "Hack",
            Padding = new(10),
        };
        this.status.FontSize *= 0.6;
        this.UpdateStatus();
        gLayout.Add(this.status, row: 1);

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

        switch (clicked)
        {
            case "ip":
                string ip = await this.client.Ping.GetPublicIpAsync();
                this.UpdateStatus(ip);
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
