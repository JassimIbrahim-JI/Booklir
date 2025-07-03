namespace Booklir.Core.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlContent);
    }
}
