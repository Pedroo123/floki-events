using FlokiEvents.Core.Models;

namespace FlokiEvents.Core.Interface;

public interface IEventQueue
{
    public Task SendMessageAsync(OrderEvent orderEvent);
    public Task ReceiveMessageAsync(IEnumerable<OrderEvent> orderEvents);
    public Task DeleteMessageAsync(string receiptHandle);
}