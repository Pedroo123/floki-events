using System.Text.Json;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using FlokiEvents.Core.Interface;
using FlokiEvents.Core.Models;
using Microsoft.Extensions.Options;

namespace FlokiEvents.Infrastructure.Services;

public class SnsPublisher : IEventPublisher
{
    private readonly IAmazonSimpleNotificationService _client;
    private readonly string _topicArn;

    public SnsPublisher(IAmazonSimpleNotificationService client, IOptions<AwsSettings> settings)
    {
        _client = client;
        _topicArn = settings.Value.TopicArn;
    }

    public async Task PublishAsync(OrderEvent message)
    {
        var request = new PublishRequest
        {
            TopicArn = _topicArn,
            Message = JsonSerializer.Serialize(message)
        };

        var response = await _client.PublishAsync(request);
        Console.WriteLine($"Message published: {response.MessageId}");
    }
}
