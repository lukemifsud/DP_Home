using CabBookingPlatformWebApp.Models;
using CabBookingPlatformWebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace CabBookingPlatformWebApp.Controllers
{
    public class AccountController : Controller
    {

        private readonly CustomerMicroservice _customerService;

        public AccountController(CustomerMicroservice customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginUserDto loginUser)
        {
            if (!ModelState.IsValid)
                return View(loginUser);

            bool success = await _customerService.LoginAsync(loginUser);

            if (success)
            {
                // Store the logged-in user's email in TempData or session
                TempData["UserEmail"] = loginUser.Email;
                TempData["Message"] = "Login successful!";
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(loginUser);
        }
    }
}
