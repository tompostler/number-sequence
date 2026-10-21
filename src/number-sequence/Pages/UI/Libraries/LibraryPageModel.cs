using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Pages.UI.Libraries
{
    /// <summary>
    /// Shared plumbing for the library pages. Actions post, then redirect back with the outcome in
    /// <see cref="StatusMessage"/> or <see cref="ErrorMessage"/>, so a phone's back button and reload never resubmit.
    /// </summary>
    public abstract class LibraryPageModel : PageModel
    {
        protected LibraryPageModel(NsTcpWtfClient nsClient)
        {
            this.NsClient = nsClient;
        }

        protected NsTcpWtfClient NsClient { get; }

        [TempData]
        public string StatusMessage { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// An error for a page rendered in this same request, such as a form shown again with what was typed. Not
        /// <see cref="ErrorMessage"/>: a [TempData] property set without a redirect is still saved, and shows up a second
        /// time on the next page.
        /// </summary>
        public string FormError { get; protected set; }

        public Library Library { get; protected set; }

        public bool CanEdit => this.Library?.CallerPermission >= LibraryPermission.Editor;

        public bool IsOwner => this.Library?.CallerPermission == LibraryPermission.Owner;

        /// <summary>
        /// Loads <see cref="Library"/>, or returns the result to send instead when it isn't visible to the caller.
        /// </summary>
        protected async Task<IActionResult> LoadLibraryAsync(long libraryId, CancellationToken cancellationToken)
        {
            try
            {
                this.Library = await this.NsClient.Library.GetLibraryAsync(libraryId, cancellationToken);
                return null;
            }
            catch (NsTcpWtfClientException)
            {
                return this.NotFound();
            }
        }

        /// <summary>
        /// Runs an api call for a posted action and redirects to <paramref name="redirectTo"/>, carrying either
        /// <paramref name="success"/> or the server's reason for refusing.
        /// </summary>
        protected async Task<IActionResult> RunAndRedirectAsync(Func<Task> action, string success, string redirectTo)
        {
            try
            {
                await action();
                this.StatusMessage = success;
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ErrorText(ex);
            }
            return this.Redirect(redirectTo);
        }

        /// <summary>
        /// The server's own reason from a failed call, rather than the client's whole message with status code, operation
        /// id and raw body. The library api answers refusals in plain text, and model validation as problem details.
        /// </summary>
        public static string ErrorText(NsTcpWtfClientException ex)
        {
            const string bodyMarker = " Body:";
            int bodyAt = ex.Message.IndexOf(bodyMarker, StringComparison.Ordinal);
            if (bodyAt < 0)
            {
                return ex.Message;
            }

            string body = ex.Message[(bodyAt + bodyMarker.Length)..].Trim();
            try
            {
                using JsonDocument doc = JsonDocument.Parse(body);
                JsonElement root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.String)
                {
                    return root.GetString();
                }
                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("errors", out JsonElement errors) && errors.ValueKind == JsonValueKind.Object)
                    {
                        return string.Join(" ", errors.EnumerateObject().SelectMany(x => x.Value.EnumerateArray()).Select(x => x.GetString()));
                    }
                    if (root.TryGetProperty("title", out JsonElement title) && title.ValueKind == JsonValueKind.String)
                    {
                        return title.GetString();
                    }
                }
            }
            catch (JsonException) { }
            return body;
        }
    }
}
