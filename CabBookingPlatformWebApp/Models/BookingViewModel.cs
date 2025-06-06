namespace CabBookingPlatformWebApp.Models
{
    public class BookingViewModel
    {
        public string StartLocation { get; set; }
        public string EndLocation { get; set; }
        public DateTime BookingDateTime { get; set; }
        public string CabType { get; set; }
        public int Passengers { get; set; }
    }
}
