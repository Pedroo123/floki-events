using System.Text.Json.Serialization;

namespace FlokiEvents.Core.Models;

public class OrderItem
{
    [JsonPropertyName("itemId")]
    public Guid ItemId { get; set; }
    
    [JsonPropertyName("itemName")]
    public string ItemName { get; set; }
    
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
    
    [JsonPropertyName("price")]
    public double Price { get; set; }
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}