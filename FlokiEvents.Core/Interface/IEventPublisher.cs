using FlokiEvents.Core.Models;

namespace FlokiEvents.Core.Interface;

public interface IEventPublisher
{
    public Task PublishAsync(OrderEvent orderEvent);
}