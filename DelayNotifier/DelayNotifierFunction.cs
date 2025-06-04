using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DelayNotifier
{
    public class DelayNotifierFunction : ICloudEventFunction
    {
        private readonly ILogger<DelayNotifierFunction> _logger;

        public DelayNotifierFunction(ILogger<DelayNotifierFunction> logger)
        {
            _logger = logger;
        }

        public async Task HandleAsync(CloudEvent cloudEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation("DelayNotifier triggered.");
            _logger.LogInformation("Raw data: {data}", cloudEvent.Data?.ToString());

            try
            {
                //parse 
                var outer = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(cloudEvent.Data?.ToString() ?? "");
                if (outer == null || !outer.TryGetValue("message", out JsonElement messageElement))
                {
                    _logger.LogWarning("Missing message field");
                    return;
                }

                //extract data
                var messageDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(messageElement.ToString());
                if (!messageDict.TryGetValue("data", out JsonElement dataElement))
                {
                    _logger.LogWarning("Missing 'data' field.");
                    return;
                }

                var base64Data = dataElement.GetString();
                if(string.IsNullOrEmpty(base64Data))
                {
                    _logger.LogWarning("Empty data");
                }

                //decode json
                string jsonPayload = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Data));
                _logger.LogInformation("Parsed payload: {payload}", jsonPayload);

                //3 minute wait
                _logger.LogInformation("Waiting 3 minutes...");
                await Task.Delay(TimeSpan.FromMinutes(3), cancellationToken);

                //publish to pubsub topic
                PublisherClient publisher = await PublisherClient.CreateAsync(
                    TopicName.Parse("projects/festive-athlete-423809-g7/topics/user-notification-topic"));

                await publisher.PublishAsync(new PubsubMessage
                {
                    Data = ByteString.CopyFromUtf8(jsonPayload)
                });

                _logger.LogInformation("Notification published to user-notification-topic.");


            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error in DelayNotifierFunction.");
            }
        }
    }
}
