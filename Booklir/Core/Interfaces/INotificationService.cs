using Booklir.Models;
using Microsoft.EntityFrameworkCore;

namespace Booklir.Core.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId);
        Task CreateNotificationAsync(string userId, string message, string url = null); 
        Task<IEnumerable<Notification>> GetRecentNotifactionsAsync(int count = 5);
        Task<int> GetUnReadNotifactionsCountAsync();
        Task<Notification> MarkAsReadAsync(int notificationId);
    }
}
