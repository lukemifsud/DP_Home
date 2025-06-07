using Google.Cloud.Firestore;
using LocationMicroservice.DTOs;
using LocationMicroservice.Models;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace LocationMicroservice.Services
{
    public class LocationService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        public LocationService(IConfiguration configuration)
        {
            _firestoreDb = FirestoreDb.Create("festive-athlete-423809-g7");
            _configuration = configuration;
            _httpClient = new HttpClient();

        }



        //add favorite location
        public async Task AddLocationAsync(DTOs.FavoriteLocationDTO dto)
        {
            var location = new FavoriteLocation
            {
                UserId = dto.UserId,
                Name = dto.Name,
                Address = dto.Address,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude
            };

            var docRef = _firestoreDb.Collection("locations").Document(location.Id);
            await docRef.SetAsync(location);
        }

        //delete favorite location
        public async Task DeleteLocationAsync(string id)
        {
            await _firestoreDb.Collection("locations").Document(id).DeleteAsync();
        }

        //update favorite location
        public async Task UpdateLocationAsync(string id, DTOs.FavoriteLocationDTO dto)
        {
            var docRef = _firestoreDb.Collection("locations").Document(id);
            await docRef.UpdateAsync(new Dictionary<string, object>
            {
                { "Name", dto.Name },
                { "Address", dto.Address },
                { "Latitude", dto.Latitude },
                { "Longitude", dto.Longitude }
            });
        }

        public async Task<FavoriteLocation?> GetLocationByIdAsync(string id)
        {
            var doc = await _firestoreDb.Collection("locations").Document(id).GetSnapshotAsync();
            return doc.Exists ? doc.ConvertTo<Models.FavoriteLocation>() : null;
        }

        public async Task<List<FavoriteLocationDTO>> GetLocationsByUserAsync(string userId)
        {
            var query = _firestoreDb.Collection("locations").WhereEqualTo("UserId", userId);
            var snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents
                .Select(d =>
                {
                    var location = d.ConvertTo<Models.FavoriteLocation>();
                    location.Id = d.Id; //
                    return location;
                })
                .Select(m => new FavoriteLocationDTO
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    Name = m.Name,
                    Address = m.Address,
                    Latitude = m.Latitude,
                    Longitude = m.Longitude
                })
                .ToList();
        }

        //get weather details 

        public async Task<WeatherDetailsDTO?> GetWeatherAsync(double latitude, double longitude)
        {
            var apiKey = _configuration["RapidApi:Key"];
            var url = $"https://weatherapi-com.p.rapidapi.com/current.json?q={latitude},{longitude}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", "weatherapi-com.p.rapidapi.com");

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new WeatherDetailsDTO
            {
                LocationName = root.GetProperty("location").GetProperty("name").GetString(),
                Region = root.GetProperty("location").GetProperty("region").GetString(),
                Country = root.GetProperty("location").GetProperty("country").GetString(),
                Condition = root.GetProperty("current").GetProperty("condition").GetProperty("text").GetString(),
                TemperatureC = root.GetProperty("current").GetProperty("temp_c").GetDouble(),
                WindKph = root.GetProperty("current").GetProperty("wind_kph").GetDouble(),
                Humidity = root.GetProperty("current").GetProperty("humidity").GetInt32()
            };
        }

    }
}
