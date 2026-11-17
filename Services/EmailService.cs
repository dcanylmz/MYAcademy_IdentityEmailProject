using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MYAcademy_IdentityEmailProject.Services
{
    /// <summary>
    /// appsettings.json → "Smtp" bölümü. Şifre koda/repoya yazılmaz:
    /// dotnet user-secrets set "Smtp:Password" "uygulama-sifresi"
    /// </summary>
    public class SmtpSettings
    {
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
        public string FromName { get; set; } = "Mailora";
    }

    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string htmlBody, string? replyToEmail = null, string? replyToName = null);
    }

    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;

        public EmailService(IOptions<SmtpSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody, string? replyToEmail = null, string? replyToName = null)
        {
            if (string.IsNullOrWhiteSpace(_settings.UserName) || string.IsNullOrWhiteSpace(_settings.Password))
                throw new InvalidOperationException("SMTP ayarları eksik. README'deki user-secrets adımlarını uygulayın.");

            var fromAddress = string.IsNullOrWhiteSpace(_settings.FromAddress) ? _settings.UserName : _settings.FromAddress;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, fromAddress));
            message.To.Add(MailboxAddress.Parse(toEmail));
            if (!string.IsNullOrWhiteSpace(replyToEmail))
                message.ReplyTo.Add(new MailboxAddress(replyToName ?? replyToEmail, replyToEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.UserName, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
