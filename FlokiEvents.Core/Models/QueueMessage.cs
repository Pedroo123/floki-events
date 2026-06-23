namespace FlokiEvents.Core.Models;

public class QueueMessage
{
    public OrderEvent OrderEvent { get; set; } = null!;
    public string ReceiptHandle { get; set; } = string.Empty;
}
