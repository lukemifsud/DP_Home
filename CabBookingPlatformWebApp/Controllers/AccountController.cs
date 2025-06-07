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

            var user = await _customerService.LoginAsync(loginUser);

            if (user != null)
            {
                // Store the logged-in user's email in TempData or session
                TempData["UserEmail"] = user.Email;
                TempData["Message"] = "Login successful!";
                HttpContext.Session.SetString("UserId", user.Id);
                HttpContext.Session.SetString("UserEmail", user.Email);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(loginUser);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        //register section

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserDto registerUser)
        {
            var user = await _customerService.RegisterAsync(registerUser);
            if (user == null)
            {
                ViewBag.Error = "Registration failed.";
                return View();
            }

            // login directly after register
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserId", user.Id);
            return RedirectToAction("Index", "Home");
        }
    }
}
