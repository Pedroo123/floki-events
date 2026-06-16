using System.Text.Json;
using Amazon.Runtime.Internal;
using Amazon.SQS;
using Amazon.SQS.Model;
using FlokiEvents.Core.Interface;
using FlokiEvents.Core.Models;

namespace FlokiEvents.Infrastructure.Services;

public class SqsQueue : IEventQueue
{
    private const string QueueName = "floki-events";
    
    public async Task<SendMessageResponse> SendMessageAsync(string queueUrl,
        OrderEvent orderEvent)
    {
        var config = new AmazonSQSConfig()
        {
            ServiceURL = "http://localhost:4566"
        };
        
        var client = new AmazonSQSClient(config);

        var messageAttributes = new AutoConstructedDictionary<string, MessageAttributeValue>();
        var sendMessageRequest = new SendMessageRequest
        {
            DelaySeconds = 10,
            MessageAttributes = messageAttributes,
            QueueUrl = queueUrl,
            MessageBody = JsonSerializer.Serialize(orderEvent)
        };
        
        var response = await  client.SendMessageAsync(sendMessageRequest);
        return response;
    }
    
    
    public async Task<CreateQueueResponse> CreateQueue()
    {
        var config = new AmazonSQSConfig()
        {
            ServiceURL = "http://localhost:4566"
        };
        
        var client = new AmazonSQSClient(config);
        
        var request = new CreateQueueRequest
        {
            QueueName =  QueueName,
            Attributes = new Dictionary<string, string>
            {
                { "DelaySeconds", "5" },
                { "MaxNumberOfMessages", "100" },
                { "MaxNumberOfFailures", "1" },
                { "MaxNumberOfRetries", "1" },
                { "MessageRetentionPeriod", "1" },
            }
        };
        
        var response = await client.CreateQueueAsync(request );
        return response;
    }

    public async Task<IEnumerable<OrderEvent>> ReceiveMessageAsync(string queueUrl, int maxMessages)
    {
        var config = new AmazonSQSConfig()
        {
            ServiceURL = "http://localhost:4566"
        };
        
        var client = new AmazonSQSClient(config);

        var messageResponse = await client.ReceiveMessageAsync(
            new ReceiveMessageRequest()
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = maxMessages,
                WaitTimeSeconds = 1,
            });
        return messageResponse.Messages.Select(x => JsonSerializer.Deserialize<OrderEvent>(x.Body));
    }
}