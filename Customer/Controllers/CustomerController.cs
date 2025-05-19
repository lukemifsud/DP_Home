using Customer.DTOs;
using Customer.Models;
using Customer.Services;
using Microsoft.AspNetCore.Mvc;

namespace Customer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        /*
        public IActionResult Index()
        {
            return View();
        }
        */

        private readonly CustomerService _service;

        public CustomerController(CustomerService service)
        {
            _service = service;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterUserDto dto)
        {
            var user = new User
            {
                Name = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = dto.Password,
            };

            var success = await _service.RegisterUserAsync(user);
            if(!success)
            {
                return BadRequest(new { message = "User already exists." });
            }

            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserDto dto)
        {
            var user = await _service.AuthenticateAsync(dto.Email, dto.Password);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials." });

            return Ok(new { user.Id, user.Name, user.LastName, user.Email });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _service.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(new { user.Id, user.Name, user.LastName, user.Email });
        }

        [HttpGet("notifications/{userId}")]
        public async Task<IActionResult> GetNotifications(string userId)
        {
            var notifications = await _service.GetNotificationsAsync(userId);
            return Ok(notifications);
        }


    }
}
