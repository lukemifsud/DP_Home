using Google.Cloud.Firestore;
using BookingMicroservice.Models;

namespace BookingMicroservice.Services
{
    public class BookingService
    {
        private readonly FirestoreDb _db;

        public BookingService()
        {
            _db = FirestoreDb.Create("festive-athlete-423809-g7");
        }

        //add collection to firestore db
        public async Task AddBookingAsync(Models.Booking booking)
        {
            await _db.Collection("bookings").Document(booking.Id).SetAsync(booking);
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
