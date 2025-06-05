using CabBookingPlatformWebApp.Models;
using System.Text;
using System.Text.Json;

namespace CabBookingPlatformWebApp.Services
{
    public class CustomerMicroservice
    {
        private readonly HttpClient _httpClient;

        public CustomerMicroservice(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://apigateway-1081298060984.europe-west1.run.app"); //gateway url
        }

        public async Task<bool> LoginAsync(LoginUserDto loginUser)
        {
            var jsonContent = JsonSerializer.Serialize(loginUser);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsJsonAsync("/api/customer/login", content);

            return response.IsSuccessStatusCode;
        }
    }
}
