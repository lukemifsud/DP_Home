using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace Customer.Models
{
    [FirestoreData]
    public class Notification
    {
        [FirestoreProperty] public string Id { get; set; } = Guid.NewGuid().ToString();

        [FirestoreProperty, JsonPropertyName("userId")]
        public string UserId { get; set; }

        [FirestoreProperty, JsonPropertyName("bookingId")]
        public string BookingId { get; set; }

        [FirestoreProperty, JsonPropertyName("pickup")]
        public string Pickup { get; set; }

        [FirestoreProperty, JsonPropertyName("dropoff")]
        public string Dropoff { get; set; }

        [FirestoreProperty, JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
