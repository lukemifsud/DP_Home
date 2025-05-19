using Google.Cloud.Firestore;

namespace PaymentMicroservice.Models
{
    [FirestoreData]
    public class Payment
    {
        [FirestoreProperty] public string Id { get; set; }
        [FirestoreProperty] public string UserId { get; set; }
        [FirestoreProperty] public string BookingId { get; set; }
        [FirestoreProperty] public string CabType { get; set; }
        [FirestoreProperty] public int Passengers { get; set; }
        [FirestoreProperty] public double TotalFare { get; set; }
        [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
