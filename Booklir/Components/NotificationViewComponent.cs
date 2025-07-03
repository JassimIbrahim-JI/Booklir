using Booklir.Core.Interfaces;
using Booklir.ViewModels.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Booklir.Components
{
    public class NotificationViewComponent : ViewComponent
    {
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _accessor;

        public NotificationViewComponent(INotificationService notificationService, IHttpContextAccessor accessor)
        {
            _notificationService = notificationService;
            _accessor = accessor;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Content(string.Empty);

            var vm = new NotificationViewModel
            {
                Notifications = await _notificationService.GetRecentNotifactionsAsync(),
                UnreadCount = await _notificationService.GetUnReadNotifactionsCountAsync()
            };

            return View(vm);
        }
    }
}
