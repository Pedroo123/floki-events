using FlokiEvents.Core.Models;

namespace FlokiEvents.Core.Interface;

public interface IEventQueue
{
    public Task SendMessageAsync(OrderEvent orderEvent);
    public Task<IEnumerable<OrderEvent>> ReceiveMessageAsync();
    public Task DeleteMessageAsync(string receiptHandle);
}