using FlokiEvents.Core.Models;

namespace FlokiEvents.Core.Interface;

public interface IOrderRepository
{
    Task<OrderEvent> SaveAsync(OrderEvent orderEvent);
    Task<OrderEvent?> GetByIdAsync(Guid id);
    Task UpdateStatusAsync(Guid id, OrderStatus status);
    Task CancelAsync(Guid id);
}
