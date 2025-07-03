namespace Booklir.Infrastructure.Settings
{
    public class EmailSettings
    {
        public string SendGridApiKey { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; } = "Booklir";
        public string AdminEmail { get; set; }
    }
}
