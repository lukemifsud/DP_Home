using Google.Cloud.Firestore;

namespace Booking.DTOs
{
    public class CreateBooking
    {
        public string UserId { get; set; }
        public string StartLocation { get; set; }
        public string EndLocation { get; set; }
        public DateTime BookingDateTime { get; set; }
        public string CabType { get; set; } 
        public int Passengers { get; set; }
       
    }
}

