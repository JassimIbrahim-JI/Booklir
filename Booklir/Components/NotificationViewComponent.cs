using Booklir.Core.Interfaces;
using Booklir.Models;
using Booklir.ViewModels.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Booklir.Components
{
    public class NotificationViewComponent : ViewComponent
    {
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _accessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationViewComponent(
            INotificationService notificationService,
            IHttpContextAccessor accessor,
            UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _accessor = accessor;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Content(string.Empty);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Content(string.Empty);   // <— guard against null

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var allNotifications = await _notificationService.GetRecentNotifactionsAsync();
            var filtered = isAdmin
                ? allNotifications.Where(n => n.Type == "BookingAlert")
                : allNotifications.Where(n => n.Type == "Booking");

            var vm = new NotificationViewModel
            {
                Notifications = filtered.ToList(),
                UnreadCount = filtered.Count(n => !n.IsRead)
            };

            return View(vm);
        }
    }

}
