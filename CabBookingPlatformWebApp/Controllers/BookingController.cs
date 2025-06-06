using CabBookingPlatformWebApp.Models;
using CabBookingPlatformWebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace CabBookingPlatformWebApp.Controllers
{
    public class BookingController : Controller
    {
        private readonly BookingMicroservice _bookingService;

        public BookingController(BookingMicroservice bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(BookingDto booking)
        {
            booking.UserId = HttpContext.Session.GetString("UserId");

            var success = await _bookingService.CreateBookingAsync(booking);
            if (!success)
            {
                ViewBag.Error = "Failed to create booking.";
                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Current()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var bookings = await _bookingService.GetCurrentBookingsAsync(userId);
            return View(bookings);
        }

        public async Task<IActionResult> Past()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var bookings = await _bookingService.GetPastBookingsAsync(userId);
            return View(bookings);
        }


        public IActionResult Index()
        {
            return View();
        }
    }
}
