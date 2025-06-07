using Customer.Data;
using Customer.Models;
using Google.Api;
using Google.Cloud.Firestore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Generators;

namespace Customer.Services
{
    public class CustomerService
    {
        private readonly FirestoreDb _db;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(ILogger<CustomerService> logger)
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "firestore-key4.json");
            _db = FirestoreDb.Create("festive-athlete-423809-g7");
            _logger = logger;
        }

        // Methods: RegisterUser, LoginUser, GetUserDetails, GetNotifications

        public async Task<bool> RegisterUserAsync(User user)
        {
            var existingUser = await GetUserByEmailAsync(user.Email);

            if (existingUser != null) return false;

            user.Id = Guid.NewGuid().ToString();
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            await _db.Collection("users").Document(user.Id).SetAsync(user);
            return true;

        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            var doc = await _db.Collection("users").Document(id).GetSnapshotAsync();
            return doc.Exists ? doc.ConvertTo<User>() : null;
        }

        public async Task<List<Notification>> GetNotificationsAsync(string userId)
        {
            var query = _db.Collection("notifications").WhereEqualTo("UserId", userId);
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.Select(d => d.ConvertTo<Notification>()).ToList();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var query = _db.Collection("users").WhereEqualTo("Email", email);
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Count > 0 ? snapshot.Documents[0].ConvertTo<User>() : null;
        }
        public async Task AddNotificationAsync(Notification notification)
        {
            var docRef = _db
                .Collection("notifications")
                .Document(notification.UserId)
                .Collection("inbox")
                .Document(); // Auto-generate ID
            _logger.LogInformation("Saving notification for user: {UserId}", notification.UserId);

            await docRef.SetAsync(notification);
        }
    }
}
