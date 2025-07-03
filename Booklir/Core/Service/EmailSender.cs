using Booklir.Core.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Mail;

namespace Booklir.Core.Service
{
    public class EmailSender : IEmailSender
    {
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailSender(IConfiguration config)
        {
            _apiKey = config["EmailSettings:SendGridApiKey"];
            _fromEmail = config["EmailSettings:FromEmail"];
            _fromName = config["EmailSettings:FromName"];
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent: htmlContent);
            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Body.ReadAsStringAsync();
                throw new Exception($"SendGrid failed: {error}");
            }
        }
    }
}
