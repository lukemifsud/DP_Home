using Google.Cloud.Firestore;

namespace BookingMicroservice.Models
{
    [FirestoreData]
    public class Booking
    {
        [FirestoreProperty] public string Id { get; set; } = Guid.NewGuid().ToString();
        [FirestoreProperty] public string UserId { get; set; }
        [FirestoreProperty] public string StartLocation { get; set; }
        [FirestoreProperty] public string EndLocation { get; set; }
        [FirestoreProperty] public DateTime BookingDateTime { get; set; } = DateTime.UtcNow;
        [FirestoreProperty] public string CabType { get; set; }  // Economic, Premium, Executive
        [FirestoreProperty] public int Passengers { get; set; }
        [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
