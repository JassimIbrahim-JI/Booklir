using Booklir.Models;

namespace Booklir.ViewModels.Authentication
{
    public class NotificationViewModel
    {
        public IEnumerable<Notification> Notifications { get; set; }
        public int UnreadCount { get; set; }
    }
}
