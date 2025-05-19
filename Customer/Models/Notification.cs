using Google.Cloud.Firestore;

namespace Customer.Models
{
    [FirestoreData]
    public class Notification
    {
        [FirestoreProperty] public string Id { get; set; } = Guid.NewGuid().ToString();
        [FirestoreProperty] public string UserId { get; set; }
        [FirestoreProperty] public string Message { get; set; }
        [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
