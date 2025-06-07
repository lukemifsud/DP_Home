using CabBookingPlatformWebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace CabBookingPlatformWebApp.Controllers
{
    public class InboxController : Controller
    {

        private readonly NotificationService _notificationService;

        public InboxController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var notifications = await _notificationService.GetNotificationsAsync(userId);
            return View(notifications);
        }
        
    }
}
