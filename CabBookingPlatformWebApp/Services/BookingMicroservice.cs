using CabBookingPlatformWebApp.Models;
using System.Text.Json;

namespace CabBookingPlatformWebApp.Services
{
    public class BookingMicroservice
    {
        private readonly HttpClient _httpClient;

        public BookingMicroservice(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://apigateway-1081298060984.europe-west1.run.app"); //gateway url
        }

        public async Task<bool> CreateBookingAsync(BookingDto booking)
        {
            Console.WriteLine(JsonSerializer.Serialize(booking));

            var response = await _httpClient.PostAsJsonAsync("/api/booking/create", booking);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<BookingViewModel>> GetCurrentBookingsAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"/api/booking/current/{userId}");
            if (!response.IsSuccessStatusCode)
                return new List<BookingViewModel>();

            return await response.Content.ReadFromJsonAsync<List<BookingViewModel>>();
        }

        public async Task<List<BookingViewModel>> GetPastBookingsAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"/api/booking/past/{userId}");
            if (!response.IsSuccessStatusCode)
                return new List<BookingViewModel>();

            return await response.Content.ReadFromJsonAsync<List<BookingViewModel>>();
        }
    }
}
