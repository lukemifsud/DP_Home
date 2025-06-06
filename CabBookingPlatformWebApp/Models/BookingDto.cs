using System.ComponentModel.DataAnnotations;

namespace CabBookingPlatformWebApp.Models
{
    public class BookingDto
    {
        public string UserId { get; set; }

        [Display(Name = "Pick Up Location")]
        public string StartLocation { get; set; }

        [Display(Name = "Drop Off Location")]
        public string EndLocation { get; set; }

        public DateTime BookingDateTime { get; set; } = DateTime.UtcNow;

        [Display(Name = "Cab Type")]
        public string CabType { get; set; }

        public int Passengers { get; set; }
    }
}
