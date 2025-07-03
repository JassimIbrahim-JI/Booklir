using Booklir.Core.Interfaces;
using Booklir.Infrastructure.Data;
using Booklir.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Booklir.Core.Service
{
    public class NotificationService : INotificationService
    {
        private readonly BookingDbContext _dbContext;
        private readonly IHttpContextAccessor _accessor;
        public NotificationService(BookingDbContext dbContext, IHttpContextAccessor accessor)
        {
            _dbContext = dbContext;
            _accessor = accessor;
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await _dbContext.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task CreateNotificationAsync(string userId, string message, string url = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                RelatedUrl = url,
                CreatedAt = DateTime.Now
            };

            _dbContext.Notifications.Add(notification);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Notification>> GetRecentNotifactionsAsync(int count = 5)
        {
           var userId = _accessor!.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //Or FindFirstValue(ClaimTypes.NameIdentifier)
            //var userId = _accessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            //var userId = _accessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userId == null) return Enumerable.Empty<Notification>();

            var notifications = await _dbContext.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync();

            return notifications;
        }

        public async Task<int> GetUnReadNotifactionsCountAsync()
        {
            var userId = _accessor!.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);

            return await _dbContext.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();

        }

        public async Task<Notification> MarkAsReadAsync(int notificationId)
        {

            var userId = _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return null;

              var notification = await _dbContext.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _dbContext.SaveChangesAsync();
            }

            return notification;
        }
    }
}
