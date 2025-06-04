using Google.Cloud.Firestore;
using BookingMicroservice.Models;

namespace BookingMicroservice.Services
{
    public class BookingService
    {
        private readonly FirestoreDb _db;
        private readonly PubSubService _pubSubService;

        public BookingService(IConfiguration configuration, PubSubService pubSubService)
        {
            _db = FirestoreDb.Create("festive-athlete-423809-g7");
            _pubSubService = pubSubService;
        }

        //add collection to firestore db
        public async Task AddBookingAsync(Models.Booking booking)
        {
            await _db.Collection("bookings").Document(booking.Id).SetAsync(booking);
            await _pubSubService.PublishBookingEventAsync(booking.UserId, booking.Id);

        }

        public async Task<List<Models.Booking>> GetBookingsByUserAsync(string userId, bool past)
        {
            var now = DateTime.UtcNow;
            var query = _db.Collection("bookings")
                           .WhereEqualTo("UserId", userId);
            //.WhereGreaterThanOrEqualTo("BookingDateTime", past ? DateTime.MinValue : now);

            if (past)
            {
                query = query.WhereLessThan("BookingDateTime", now); // past
            }
            else
            {
                query = query.WhereGreaterThanOrEqualTo("BookingDateTime", now); //current
            }

            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<Models.Booking>()).ToList();
        }

    }
}
