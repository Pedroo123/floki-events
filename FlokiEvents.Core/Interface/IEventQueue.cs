using FlokiEvents.Core.Models;

namespace FlokiEvents.Core.Interface;

public interface IEventQueue
{
    Task SendMessageAsync(OrderEvent orderEvent);
    Task<IEnumerable<QueueMessage>> ReceiveMessageAsync(int maxMessages);
    Task DeleteMessageAsync(string receiptHandle);
}
