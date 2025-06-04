using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using System.Text.Json;

namespace BookingMicroservice.Services
{
    public class PubSubService
    {
        private readonly string _projectId;
        private readonly string _topicId;

        public PubSubService(IConfiguration configuration)
        {
            _projectId = configuration["PubSub:ProjectId"];
            _topicId = configuration["PubSub:TopicId"];
        }

        public async Task PublishBookingEventAsync(string userId, string bookingId)
        {
            var topicName = TopicName.FromProjectTopic(_projectId, _topicId);
            var publisher = await PublisherClient.CreateAsync(topicName);

            var payload = new
            {
                userId = userId,
                bookingId = bookingId,
                timestamp = DateTime.UtcNow
            };

            string json = JsonSerializer.Serialize(payload);
            var message = new PubsubMessage
            {
                Data = ByteString.CopyFromUtf8(json)
            };

            await publisher.PublishAsync(message);
        }
    }
}
