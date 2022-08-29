using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace number_sequence.DataAccess
{
    public sealed class EmailDataAccess
    {
        private readonly Options.Email emailOptions;
        private readonly ILogger<EmailDataAccess> logger;

        public EmailDataAccess(IOptions<Options.Email> emailOptions, ILogger<EmailDataAccess> logger)
        {
            this.emailOptions = emailOptions.Value;
            this.logger = logger;
        }

        public async Task SendEmailAsync(MailMessage message, CancellationToken cancellationToken)
        {
            using var smtpClient = new SmtpClient(this.emailOptions.Server, this.emailOptions.Port);
            smtpClient.EnableSsl = true;
            smtpClient.Credentials = new NetworkCredential(this.emailOptions.Username, this.emailOptions.Password);

            var sb = new StringBuilder();
            _ = sb.AppendLine("Sending message:");
            _ = sb.AppendLine($"TO:  {string.Join(';', message.To.Select(t => t.Address))}");
            _ = sb.AppendLine($"CC:  {string.Join(';', message.CC.Select(t => t.Address))}");
            _ = sb.AppendLine($"Bcc: {string.Join(';', message.Bcc.Select(t => t.Address))}");
            _ = sb.AppendLine($"Subject: {message.Subject}");
            _ = sb.AppendLine($"Attachments: {message.Attachments.Count}");
            _ = sb.AppendLine("Body:");
            _ = sb.AppendLine(message.Body);
            this.logger.LogInformation(sb.ToString());

            // Set the from (and Bcc to self as that's the easiest way to track sent)
            message.From = new MailAddress(this.emailOptions.Username);
            message.Bcc.Add(new MailAddress(this.emailOptions.Username));

            await smtpClient.SendMailAsync(message, cancellationToken);
        }
    }
}
