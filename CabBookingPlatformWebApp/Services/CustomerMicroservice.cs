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

        public async Task<UserDto?> LoginAsync(LoginUserDto loginUser)
        {
            var jsonContent = JsonSerializer.Serialize(loginUser);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            Console.WriteLine("Sending JSON:");
            Console.WriteLine(jsonContent);

            var response = await _httpClient.PostAsJsonAsync("/api/customer/login", loginUser);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UserDto>();
        }

        public async Task<UserDto?> RegisterAsync(RegisterUserDto registerUser)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/customer/Register", registerUser);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UserDto>();
        }
    }
}
