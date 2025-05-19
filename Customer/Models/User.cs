using Google.Cloud.Firestore;
using System.Globalization;

namespace Customer.Models
{
    [FirestoreData]
    public class User
    {
        [FirestoreProperty] public string Id { get; set; } = Guid.NewGuid().ToString();
        [FirestoreProperty] public string Name { get; set; }
        [FirestoreProperty] public string LastName { get; set; }
        [FirestoreProperty] public string Email { get; set; }
        [FirestoreProperty] public string PasswordHash { get; set; }

    }
}
