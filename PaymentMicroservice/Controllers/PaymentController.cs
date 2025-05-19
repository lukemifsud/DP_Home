using Microsoft.AspNetCore.Mvc;
using PaymentMicroservice.DTOs;
using PaymentMicroservice.Models;
using PaymentMicroservice.Services;

namespace PaymentMicroservice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService service)
        {
            _paymentService = service;
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> CalculatePayment(CalculatePayment dto)
        {
            double total = await _paymentService.CalculateFarePriceAsync(dto.CabType, dto.Passengers, dto.BookingDateTime, dto.StartLocation, dto.EndLocation);

            var payment = new Payment
            {
                UserId = dto.UserId,
                BookingId = dto.BookingId,
                CabType = dto.CabType,
                Passengers = dto.Passengers,
                TotalFare = total
            };

            var paymentId = await _paymentService.SavePaymentAsync(payment);

            return Ok(new
            {
                paymentId,
                total,
                breakdown = new
                {
                    cabType = dto.CabType,
                    passengerCount = dto.Passengers,
                    bookingDate = dto.BookingDateTime
                }
            });
        }
    }
}
