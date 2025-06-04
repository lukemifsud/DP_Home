using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscountNotifier.Models
{
    [FirestoreData]
    public class BookingStatus
    {
        [FirestoreProperty]
        public int Count { get; set; }

        [FirestoreProperty]
        public bool Notified { get; set; } = false;
    }
}
