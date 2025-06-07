using CloudNative.CloudEvents;
using DiscountNotifier.Models;
using Google.Cloud.Firestore;
using Google.Cloud.Functions.Framework;
using Google.Cloud.PubSub.V1;
using Google.Events.Protobuf.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Text;



namespace DiscountNotifier
{
    public class BookingEventFunction : ICloudEventFunction
    {
        private readonly ILogger<BookingEventFunction> _logger;
        private readonly FirestoreDb _firestore;

        public BookingEventFunction(ILogger<BookingEventFunction> logger)
        {
            _logger = logger;
            _firestore = FirestoreDb.Create("festive-athlete-423809-g7");
        }

        public async Task HandleAsync(CloudEvent cloudEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation(" Cloud Function triggered.");
            _logger.LogInformation("Raw cloudEvent.Data: {data}", cloudEvent.Data?.ToString());

            try
            {
                // Step 1: Deserialize the outer envelope
                var envelope = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(cloudEvent.Data?.ToString() ?? "");
                if (envelope == null || !envelope.TryGetValue("message", out JsonElement messageElement))
                {
                    _logger.LogWarning(" Pub/Sub envelope missing 'message' field.");
                    return;
                }

                // Step 2: Decode message object
                var pubsubMessage = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(messageElement.ToString());
                if (pubsubMessage == null || !pubsubMessage.TryGetValue("data", out JsonElement dataElement))
                {
                    _logger.LogWarning(" Pub/Sub 'data' field missing.");
                    return;
                }

                var base64Data = dataElement.GetString();
                if (string.IsNullOrEmpty(base64Data))
                {
                    _logger.LogWarning(" 'data' field is empty.");
                    return;
                }

                var decodedJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Data));
                _logger.LogInformation(" Decoded JSON payload: {json}", decodedJson);

                // Step 3: Extract userId and bookingId
                var payload = JsonSerializer.Deserialize<Dictionary<string, string>>(decodedJson);
                if (payload == null || !payload.TryGetValue("userId", out string userId))
                {
                    _logger.LogWarning(" Payload malformed or missing 'userId'.");
                    return;
                }

                if (!payload.TryGetValue("bookingId", out string bookingId))
                {
                    _logger.LogWarning("⚠ Payload missing 'bookingId'.");
                    return;
                }

                _logger.LogInformation(" Handling booking for user: {userId}, booking: {bookingId}", userId, bookingId);

                // Step 4: Run transactional update
                var statusRef = _firestore.Collection("user-booking-status").Document(userId);
                var processedRef = statusRef.Collection("processed-bookings").Document(bookingId);

                await _firestore.RunTransactionAsync(async transaction =>
                {
                    var alreadyProcessed = await transaction.GetSnapshotAsync(processedRef);
                    if (alreadyProcessed.Exists)
                    {
                        _logger.LogInformation(" Booking {bookingId} already processed. Skipping.", bookingId);
                        return;
                    }

                    var snapshot = await transaction.GetSnapshotAsync(statusRef);
                    var status = snapshot.Exists ? snapshot.ConvertTo<BookingStatus>() : new BookingStatus();

                    if (status.Count >= 3 && status.Notified)
                    {
                        _logger.LogInformation(" User already notified and at max count. Skipping.");
                        return;
                    }

                    status.Count++;

                    if (status.Count == 3 && !status.Notified)
                    {
                        _logger.LogInformation(" Sending discount notification for {userId}", userId);
                        status.Notified = true;

                        var notifRef = _firestore.Collection("notifications").Document();
                        await notifRef.SetAsync(new
                        {
                            UserId = userId,
                            BookingId = bookingId,
                            Message = "🎁 You've earned a discount on your next ride!",
                            Timestamp = Timestamp.GetCurrentTimestamp()
                        });

                    }


                    transaction.Set(statusRef, status);
                    transaction.Set(processedRef, new { timestamp = Timestamp.GetCurrentTimestamp() });

                    _logger.LogInformation(" Booking count updated. Count={0}, Notified={1}", status.Count, status.Notified);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Unhandled exception in function.");
            }
        }
    }
    public class PubSubEnvelope
    {
        public PubSubMessage Message { get; set; }
    }

    public class PubSubMessage
    {
        public string Data { get; set; }
    }
}
