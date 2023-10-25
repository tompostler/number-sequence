using Microsoft.Extensions.Logging;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Tool;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.UI.Views;

public class LoginPage : ContentPage
{
    private readonly Label status;
    private readonly Entry username;
    private readonly Entry password;

    // Dependency Injection does not appear to work properly for the first page we navigate to, so only use DI components on interaction
    public LoginPage()
    {
        this.Title = "Log In";
        var vLayout = new VerticalStackLayout
        {
            Spacing = 20,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        this.Content = vLayout;

        Label instructions = new();
        vLayout.Add(instructions);

        var signin = new Button
        {
            Text = "Sign in",
            MaximumWidthRequest = 200
        };
        signin.Clicked += this.Signin_Clicked;
        vLayout.Add(signin);

        this.status = new Label();
        vLayout.Add(this.status);

        try
        {
            TokenProvider.TokenValue token = TokenProvider.GetTokenValue();
            if (token.ExpirationDate < DateTimeOffset.Now)
            {
                instructions.Text = $"Token expiration is in the past and a new one will need to be created (expired {token.ExpirationDate:u}).";
            }
            else
            {
                instructions.Text = $"Existing login information found:\n{token.ToJsonString(indented: true)}";
                return;
            }
        }
        catch (FileNotFoundException)
        {
            instructions.Text = "Existing login information not found.";
        }
        instructions.Text += "\nSign in with your account information to create a token for this device:";

        this.username = new Entry();
        vLayout.Insert(1, this.username);
        this.password = new Entry() { IsPassword = true };
        vLayout.Insert(2, this.password);
    }

    private async void Signin_Clicked(object sender, EventArgs e)
    {
        NsTcpWtfClient client = new(new LoggerFactory().CreateLogger<NsTcpWtfClient>(), TokenProvider.GetAsync);

        // Create a new token for this device if username/password were provided.
        if (this.username != default)
        {
            this.status.Text = "Creating new token with ns...";
            Contracts.Token token = await client.Token.CreateAsync(
                new()
                {
                    Account = this.username.Text,
                    Key = this.password.Text,
                    Name = Environment.MachineName,
                    ExpirationDate = DateTimeOffset.Now.AddYears(1),
                });
            TokenProvider.Upsert(token.Value);
            TokenProvider.TokenValue savedToken = TokenProvider.GetTokenValue();
            this.status.Text += $"\n\nCreated:\n{savedToken.ToJsonString(indented: true)}\n";
        }

            this.status.Text = "Sending ping request to ns to validate auth...";
        try
        {
            await client.Ping.SendWithAuthAsync();
            this.status.Text += "\nSuccess!";

            await Task.Delay(TimeSpan.FromSeconds(0.5));
            await Shell.Current.GoToAsync("homepage");
        }
        catch (NsTcpWtfClientException ex)
        {
            await this.DisplayAlert(title: "Sign in failed", message: ex.Message, cancel: "OK");
        }
    }

    // Do not allow the back button to do anything.
    protected override bool OnBackButtonPressed() => true;
}
