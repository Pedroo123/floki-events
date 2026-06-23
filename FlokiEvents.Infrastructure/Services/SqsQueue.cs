using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using FlokiEvents.Core.Interface;
using FlokiEvents.Core.Models;
using Microsoft.Extensions.Options;

namespace FlokiEvents.Infrastructure.Services;

public class SqsQueue : IEventQueue
{
    private readonly IAmazonSQS _client;
    private readonly string _queueUrl;

    public SqsQueue(IAmazonSQS client, IOptions<AwsSettings> settings)
    {
        _client = client;
        _queueUrl = settings.Value.QueueUrl;
    }

    public async Task SendMessageAsync(OrderEvent orderEvent)
    {
        var request = new SendMessageRequest
        {
            QueueUrl = _queueUrl,
            MessageBody = JsonSerializer.Serialize(orderEvent)
        };

        await _client.SendMessageAsync(request);
    }

    public async Task<IEnumerable<QueueMessage>> ReceiveMessageAsync(int maxMessages)
    {
        var request = new ReceiveMessageRequest
        {
            QueueUrl = _queueUrl,
            MaxNumberOfMessages = maxMessages,
            WaitTimeSeconds = 10
        };

        var response = await _client.ReceiveMessageAsync(request);

        return response.Messages.Select(m => new QueueMessage
        {
            OrderEvent = JsonSerializer.Deserialize<OrderEvent>(m.Body) ?? new OrderEvent(),
            ReceiptHandle = m.ReceiptHandle
        });
    }

    public async Task DeleteMessageAsync(string receiptHandle)
    {
        var request = new DeleteMessageRequest
        {
            QueueUrl = _queueUrl,
            ReceiptHandle = receiptHandle
        };

        await _client.DeleteMessageAsync(request);
    }
}
