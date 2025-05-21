using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace MosefakApi.Business.Services.Email
{
    public class EmailSender : IEmailSender // Implements the existing IEmailSender
    {
        private readonly MailSettings _settings;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<MailSettings> settings, ILogger<EmailSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        // This signature MUST match IEmailSender
        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            if (string.IsNullOrEmpty(_settings.Email) || string.IsNullOrEmpty(_settings.Host))
            {
                _logger.LogError("Email settings (sender email or host) are not configured.");
                return;
            }
            // Ensure DisplayName is configured in MailSettings, as it's the only source now.
            if (string.IsNullOrEmpty(_settings.DisplayName))
            {
                _logger.LogError("MailSettings.DisplayName is not configured. This is required for the sender's display name.");
                // Decide if you want to throw an error or fallback to the email address as display name
                // Forcing it to be configured is safer for consistent branding.
                // For now, let's throw or return if it's critical.
                // throw new InvalidOperationException("MailSettings.DisplayName must be configured.");
                // Fallback for now, but configuration is better:
                // _settings.DisplayName = _settings.Email; 
            }

            try
            {
                var message = new MimeMessage();

                // --- KEY CHANGE HERE ---
                // DisplayName now SOLELY comes from _settings.DisplayName
                // The `null!` in your MailSettings class for DisplayName implies it should always be set via configuration.
                message.From.Add(new MailboxAddress(_settings.DisplayName, _settings.Email));

                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;

                var builder = new BodyBuilder { HtmlBody = htmlMessage };
                message.Body = builder.ToMessageBody();

                using var smtpClient = new SmtpClient();

                await smtpClient.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTlsWhenAvailable);
                await smtpClient.AuthenticateAsync(_settings.Email, _settings.Password);
                await smtpClient.SendAsync(message);
                await smtpClient.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to {toEmail} from '{_settings.DisplayName} <{_settings.Email}>' with subject '{subject}'.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail} from {_settings.Email} with subject '{subject}'.");
                // throw; // Consider re-throwing if the caller needs to handle the failure
            }
        }
    }
}