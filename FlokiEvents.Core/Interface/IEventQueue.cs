using Amazon.SQS.Model;
using FlokiEvents.Core.Models;

namespace FlokiEvents.Core.Interface;

public interface IEventQueue
{
    public Task<SendMessageResponse> SendMessageAsync(string queueUrl, OrderEvent orderEvent);
    public Task<IEnumerable<OrderEvent>> ReceiveMessageAsync(string queueUrl, int maxMessages);
}