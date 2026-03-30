using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Filters;
using number_sequence.Models;
using TcpWtf.NumberSequence.Contracts;
using Unlimitedinf.Utilities.Extensions;

namespace number_sequence.Pages.UI
{
    public sealed class LoginModel : PageModel
    {
        public const string WebTokenName = "web";

        private readonly NsContext nsContext;
        private readonly ILogger<LoginModel> logger;

        public LoginModel(NsContext nsContext, ILogger<LoginModel> logger)
        {
            this.nsContext = nsContext;
            this.logger = logger;
        }

        [BindProperty]
        public string AccountName { get; set; }

        [BindProperty]
        public string AccountKey { get; set; }

        [FromQuery]
        public string ReturnUrl { get; set; }

        public string ErrorMessage { get; private set; }

        public IActionResult OnGet() => this.Page();

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(this.AccountName) || string.IsNullOrWhiteSpace(this.AccountKey))
            {
                this.ErrorMessage = "Account name and key are required.";
                return this.Page();
            }

            TcpWtf.NumberSequence.Contracts.Account account = await this.nsContext.Accounts.SingleOrDefaultAsync(x => x.Name == this.AccountName, cancellationToken);
            if (account == default || this.AccountKey.ComputeSHA256() != account.Key)
            {
                this.ErrorMessage = "Invalid account name or key.";
                return this.Page();
            }

            Token token = await this.nsContext.Tokens.SingleOrDefaultAsync(x => x.Account == account.Name && x.Name == WebTokenName, cancellationToken);

            if (token != default && token.ExpirationDate > DateTimeOffset.UtcNow)
            {
                // Reuse existing valid token.
                this.logger.LogInformation($"Reusing existing web token for account: {account.Name}");
            }
            else
            {
                // Creating new token, ignoring the limit. Web ui is a special token.
                if (token == default)
                {
                    token = new Token
                    {
                        Account = account.Name,
                        AccountTier = account.Tier,
                        Name = WebTokenName,
                    };
                    _ = this.nsContext.Tokens.Add(token);
                }

                token.Key = Guid.NewGuid().ToString();
                token.ExpirationDate = DateTimeOffset.UtcNow.AddDays(90);
                token.Value = TokenValue.CreateFrom(token).ToBase64JsonString();

                _ = await this.nsContext.SaveChangesAsync(cancellationToken);
                this.logger.LogInformation($"Created web token for account: {account.Name}");
            }

            this.Response.Cookies.Append(
                RequiresTokenFilter.TokenCookieName,
                token.Value,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = token.ExpirationDate,
                });

            string destination = !string.IsNullOrEmpty(this.ReturnUrl) && this.ReturnUrl.StartsWith('/') && !this.ReturnUrl.StartsWith("//")
                ? this.ReturnUrl
                : "/ui";
            return this.Redirect(destination);
        }
    }
}
