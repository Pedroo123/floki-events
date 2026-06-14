using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using FlokiEvents.Core.Interface;
using FlokiEvents.Core.Models;

namespace FlokiEvents.Infrastructure.Services;

public class SnsPublisher : IEventPublisher
{
    public async Task PublishAsync(OrderEvent message)
    {
        var config = new AmazonSimpleNotificationServiceConfig
        {
            ServiceURL = "http://localhost:4566"
        };
        
        var client = new AmazonSimpleNotificationServiceClient(config);
        
        var request = new PublishRequest
        {
            TopicArn = await CreateSNSTopicAsync(client),
            Message = JsonSerializer.Serialize(message),
        };
        
        var response = await client.PublishAsync(request);
        Console.WriteLine($"Message published: {response}");
    }
    
    private static async Task<string> CreateSNSTopicAsync(IAmazonSimpleNotificationService client)
    {
        var request = new CreateTopicRequest
        {
            Name = "ExampleSNSTopicName"
        };
    
        var response = await client.CreateTopicAsync(request);
        return response.TopicArn;
    }
}