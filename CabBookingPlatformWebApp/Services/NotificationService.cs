using CabBookingPlatformWebApp.Models;
using Google.Cloud.Firestore;

namespace CabBookingPlatformWebApp.Services
{
    public class NotificationService
    {
        private readonly FirestoreDb _firestoreDb;

        public NotificationService()
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "firestore-key4.json");
            _firestoreDb = FirestoreDb.Create("festive-athlete-423809-g7");
        }

        public async Task<List<Notification>> GetNotificationsAsync(string userId)
        {
            var snapshot = await _firestoreDb.Collection("notifications")
                                             .WhereEqualTo("UserId", userId)
                                             .OrderByDescending("Timestamp")
                                             .GetSnapshotAsync();

            return snapshot.Documents.Select(doc => new Notification
            {
                Id = doc.Id,
                UserId = doc.GetValue<string>("UserId"),
                Timestamp = doc.GetValue<Timestamp>("Timestamp").ToDateTime(),
                Message = doc.TryGetValue<string>("Message", out var msg) ? msg : "",
            }).ToList();
        }
    }
}
