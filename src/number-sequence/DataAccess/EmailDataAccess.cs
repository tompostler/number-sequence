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
            sb.AppendLine("Sending message:");
            sb.AppendLine($"TO:  {string.Join(';', message.To.Select(t => t.Address))}");
            sb.AppendLine($"CC:  {string.Join(';', message.CC.Select(t => t.Address))}");
            sb.AppendLine($"Bcc: {string.Join(';', message.Bcc.Select(t => t.Address))}");
            sb.AppendLine($"Subject: {message.Subject}");
            sb.AppendLine($"Attachments: {message.Attachments.Count}");
            sb.AppendLine("Body:");
            sb.AppendLine(message.Body);
            this.logger.LogInformation(sb.ToString());

            await smtpClient.SendMailAsync(message, cancellationToken);
        }
    }
}
