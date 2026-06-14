using FlokiEvents.Core.Models;

namespace FlokiEvents.Core.Interface;

public interface IOrderRepository
{
    public Task SaveAsync(OrderEvent orderEvent);
    public Task GetByIdAsync(string id);
    public Task UpdateAsync(string id, OrderStatus status);
    public Task DeleteAsync(string id);
}