namespace LocationMicroservice.DTOs
{
    public class WeatherDetailsDTO
    {
        public string LocationName { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string Condition { get; set; }
        public double TemperatureC { get; set; }
        public double WindKph { get; set; }
        public int Humidity { get; set; }
    }
}
