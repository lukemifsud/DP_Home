using LocationMicroservice.DTOs;
using LocationMicroservice.Services;
using Microsoft.AspNetCore.Mvc;

namespace LocationMicroservice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly LocationService _locationService;

        public LocationController(LocationService locationService)
        {
            _locationService = locationService;
        }

        //add location
        [HttpPost("add")]
        public async Task<IActionResult> AddLocation([FromBody] FavoriteLocationDTO dto)
        {
            
            await _locationService.AddLocationAsync(dto);
            return Ok(new { message = "Location added successfully." });
        }


        //get user favorite location
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserLocations(string userId)
        {
            var locations = await _locationService.GetLocationsByUserAsync(userId);
            return Ok(locations);
        }


        //delete location
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteLocation(string id)
        {
            await _locationService.DeleteLocationAsync(id);
            return Ok(new { message = "Location deleted successfully." });
        }

        //update location
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateLocation(string id, [FromBody] FavoriteLocationDTO dto)
        {
            await _locationService.UpdateLocationAsync(id, dto);
            return Ok(new { message = "Location updated successfully." });
        }

        //get weather details of location
        [HttpGet("weather/{id}")]
        public async Task<IActionResult> GetWeatherForLocation(string id)
        {
            var location = await _locationService.GetLocationByIdAsync(id);
            if (location == null)
                return NotFound("Location not found");

            var weather = await _locationService.GetWeatherAsync(location.Latitude, location.Longitude);
            return weather != null ? Ok(weather) : StatusCode(500, "Failed to retrieve weather data");
        }
    }
}
