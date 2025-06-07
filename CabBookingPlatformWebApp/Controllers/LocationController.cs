using CabBookingPlatformWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CabBookingPlatformWebApp.Controllers
{
    public class LocationController : Controller
    {

        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "https://apigateway-1081298060984.europe-west1.run.app/api/Location";

        public LocationController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task<IActionResult> Index()
        {
            string userId = HttpContext.Session.GetString("UserId"); // or however you store it
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/user/{userId}");
            var json = await response.Content.ReadAsStringAsync();
            var locations = JsonSerializer.Deserialize<List<FavoriteLocation>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(locations);
        }

        public IActionResult Add() => View();

        [HttpPost]
        public async Task<IActionResult> AddLocation(FavoriteLocation location)
        {
            location.UserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(location.Id))
            {
                location.Id = Guid.NewGuid().ToString();
            }
            var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}/add", location);
            
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError("", "Failed to add location");
            return View("Add", location); // ✅ explicitly point to the correct view name
        }

        public async Task<IActionResult> EditLocation(string id)
        {
            var locations = await _httpClient.GetFromJsonAsync<List<FavoriteLocation>>($"{_apiBaseUrl}/user/{HttpContext.Session.GetString("UserId")}");
            var target = locations?.FirstOrDefault(l => l.Id == id);
            Console.WriteLine(target.Id);
            return View(target);
        }

        [HttpPost]
        public async Task<IActionResult> EditLocation(FavoriteLocation updated)
        {
            Console.WriteLine($"Updating: {updated.Id} - {updated.Name}");
            var response = await _httpClient.PutAsJsonAsync($"{_apiBaseUrl}/update/{updated.Id}", updated);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteLocation(string id)
        {
            await _httpClient.DeleteAsync($"{_apiBaseUrl}/delete/{id}");
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Weather(string id)
        {
            var weather = await _httpClient.GetFromJsonAsync<WeatherForecast>($"{_apiBaseUrl}/weather/{id}");
            return View(weather);
        }

    }
}
