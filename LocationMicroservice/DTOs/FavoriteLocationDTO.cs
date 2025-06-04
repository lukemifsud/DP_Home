namespace LocationMicroservice.DTOs
{
    public class FavoriteLocationDTO
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
