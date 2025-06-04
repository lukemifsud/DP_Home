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
            _logger.LogInformation("🚀 Cloud Function triggered.");
            _logger.LogInformation("Raw cloudEvent.Data: {data}", cloudEvent.Data?.ToString());

            try
            {
                // Step 1: Deserialize the outer envelope (which contains "message")
                var envelope = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(cloudEvent.Data?.ToString() ?? "");

                if (envelope == null || !envelope.TryGetValue("message", out JsonElement messageElement))
                {
                    _logger.LogWarning("⚠️ Pub/Sub envelope missing 'message' field.");
                    return;
                }

                // Step 2: Deserialize the "message" object which contains "data"
                var pubsubMessage = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(messageElement.ToString());

                if (pubsubMessage == null || !pubsubMessage.TryGetValue("data", out JsonElement dataElement))
                {
                    _logger.LogWarning("⚠️ Pub/Sub 'data' field missing.");
                    return;
                }

                // Step 3: Decode the base64-encoded data
                var base64Data = dataElement.GetString();
                if (string.IsNullOrEmpty(base64Data))
                {
                    _logger.LogWarning("⚠️ 'data' field is empty.");
                    return;
                }

                var decodedJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Data));
                _logger.LogInformation("✅ Decoded JSON payload: {json}", decodedJson);

                // Step 4: Parse the decoded JSON
                var payload = JsonSerializer.Deserialize<Dictionary<string, string>>(decodedJson);

                if (payload == null || !payload.TryGetValue("userId", out string userId))
                {
                    _logger.LogWarning("⚠️ Payload malformed or missing 'userId'.");
                    return;
                }

                _logger.LogInformation("🔁 Handling booking for user: {userId}", userId);

                // Step 5: Firestore update
                var docRef = _firestore.Collection("user-booking-status").Document(userId);
                var snapshot = await docRef.GetSnapshotAsync();

                var status = snapshot.Exists
                    ? snapshot.ConvertTo<BookingStatus>()
                    : new BookingStatus();

                status.Count++;

                if (status.Count == 3 && !status.Notified)
                {
                    _logger.LogInformation("🎉 Sending discount notification for {userId}", userId);
                    status.Notified = true;
                }

                await docRef.SetAsync(status);
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
