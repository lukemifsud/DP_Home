using Customer.Models;
using Google.Cloud.PubSub.V1;
using System.Text.Json;

namespace Customer.Services
{
    public class NotificationSubscriber : BackgroundService
    {
        private readonly ILogger<NotificationSubscriber> _logger;
        private readonly CustomerService _customerService;

        public NotificationSubscriber(ILogger<NotificationSubscriber> logger, CustomerService customerService)
        {
            _logger = logger;
            _customerService = customerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string projectId = "festive-athlete-423809-g7";
            string subscriptionId = "user-notification-topic-sub";

            SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription(projectId, subscriptionId);
            SubscriberClient subscriber = await SubscriberClient.CreateAsync(subscriptionName);

            _logger.LogInformation("Subscribed to user-notification-topic...");

            await subscriber.StartAsync(async (PubsubMessage message, CancellationToken ct) =>
            {
                try
                {
                    string json = System.Text.Encoding.UTF8.GetString(message.Data.ToByteArray());
                    _logger.LogInformation("📥 Notification received: {json}", json);

                    var notification = JsonSerializer.Deserialize<Notification>(json);

                    if (notification == null)
                    {
                        _logger.LogWarning("Deserialization returned null.");
                    }
                    else
                    {
                        _logger.LogInformation("Deserialized: {UserId}", notification.UserId);
                        //await _customerService.AddNotificationAsync(notification);
                        _logger.LogInformation("Notification saved to Firestore.");
                    }

                    return SubscriberClient.Reply.Ack;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception during deserialization or saving.");
                    return SubscriberClient.Reply.Nack;
                }
            });

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

    }
}
