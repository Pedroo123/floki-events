using System.Text.Json.Serialization;

namespace FlokiEvents.Core.Models;

public class OrderEvent
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("orderId")]
    public int OrderId { get; set; }
    
    [JsonPropertyName("orderItem")]
    public OrderItem OrderItem { get; set; }
    
    [JsonPropertyName("orderDate")]
    public DateTime OrderDate { get; set; }
    
    [JsonPropertyName("status")]
    public OrderStatus Status { get; set; }
    
    [JsonPropertyName("createdBy")]
    public string CreatedBy { get; set; }
}