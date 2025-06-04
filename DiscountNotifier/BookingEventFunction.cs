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
            _logger.LogInformation("Event Type: {type}", cloudEvent.Type);
            _logger.LogInformation("Raw CloudEvent.Data: {data}", cloudEvent.Data?.ToString());

            if (cloudEvent.Data is not JsonElement jsonElement)
            {
                _logger.LogWarning("CloudEvent.Data is not a valid JsonElement.");
                return;
            }

            try
            {
                // Step 1: Deserialize Data as string -> JSON
                var outer = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(cloudEvent.Data?.ToString() ?? "");
                if (outer == null || !outer.ContainsKey("message"))
                {
                    _logger.LogWarning(" Outer envelope is missing 'message' field.");
                    return;
                }

                // Step 2: Deserialize to PubSubEnvelope
                var envelope = outer["message"].Deserialize<PubSubMessage>();
                if (envelope?.Data == null)
                {
                    _logger.LogWarning("Pub/Sub envelope or data missing.");
                    return;
                }

                var decodedJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(envelope.Data));
                _logger.LogInformation("Decoded payload: {decodedJson}", decodedJson);

                var payload = JsonSerializer.Deserialize<Dictionary<string, string>>(decodedJson);
                if (payload is null || !payload.ContainsKey("userId"))
                {
                    _logger.LogWarning(" Payload missing or malformed.");
                    return;
                }

                string userId = payload["userId"];
                _logger.LogInformation(" Updating booking count for: {userId}", userId);

                var docRef = _firestore.Collection("user-booking-status").Document(userId);
                var snapshot = await docRef.GetSnapshotAsync();

                var status = snapshot.Exists
                    ? snapshot.ConvertTo<BookingStatus>()
                    : new BookingStatus();

                status.Count++;

                if (status.Count == 3 && !status.Notified)
                {
                    _logger.LogInformation($" Discount activated for {userId}");
                    status.Notified = true;
                }

                await docRef.SetAsync(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in DiscountNotifier.");
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
