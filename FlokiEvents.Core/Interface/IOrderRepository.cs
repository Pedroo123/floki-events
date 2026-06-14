using FlokiEvents.Core.Models;

namespace FlokiEvents.Core.Interface;

public interface IOrderRepository
{
    public Task<OrderEvent> SaveAsync(OrderEvent orderEvent);
    public Task GetByIdAsync(string id);
    public Task UpdateStatusAsync(string id, OrderStatus status);
    public Task CancelAsync(string id);
}