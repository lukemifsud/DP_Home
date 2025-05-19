using Booking.DTOs;
using BookingMicroservice.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingMicroservice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly BookingService _service;

        public BookingController(BookingService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking(CreateBooking dto)
        {
            var booking = new Models.Booking
            {
                UserId = dto.UserId,
                StartLocation = dto.StartLocation,
                EndLocation = dto.EndLocation,
                BookingDateTime = dto.BookingDateTime.ToUniversalTime(),
                CabType = dto.CabType,
                Passengers = dto.Passengers
            };

            await _service.AddBookingAsync(booking);
            return Ok(new { message = "Booking created successfully." });
        }

        [HttpGet("current/{userId}")]
        public async Task<IActionResult> GetCurrentBookings(string userId)
        {
            var bookings = await _service.GetBookingsByUserAsync(userId, past: false);
            return Ok(bookings);
        }

        [HttpGet("past/{userId}")]
        public async Task<IActionResult> GetPastBookings(string userId)
        {
            var bookings = await _service.GetBookingsByUserAsync(userId, past: true);
            return Ok(bookings);
        }
    }
}
