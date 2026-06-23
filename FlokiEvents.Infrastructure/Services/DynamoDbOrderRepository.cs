using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using FlokiEvents.Core.Interface;
using FlokiEvents.Core.Models;
using Microsoft.Extensions.Options;

namespace FlokiEvents.Infrastructure.Services;

public class DynamoDbOrderRepository : IOrderRepository
{
    private readonly IAmazonDynamoDB _client;
    private readonly string _tableName;

    public DynamoDbOrderRepository(IAmazonDynamoDB client, IOptions<AwsSettings> settings)
    {
        _client = client;
        _tableName = settings.Value.DynamoTableName;
    }

    public async Task<OrderEvent> SaveAsync(OrderEvent orderEvent)
    {
        var document = new Document
        {
            ["OrderId"] = orderEvent.OrderId.ToString(),
            ["OrderDate"] = orderEvent.OrderDate.ToString("O"),
            ["Status"] = orderEvent.Status.ToString(),
            ["CreatedBy"] = orderEvent.CreatedBy,
            ["ItemName"] = orderEvent.OrderItem.ItemName,
            ["Quantity"] = orderEvent.OrderItem.Quantity,
            ["Price"] = orderEvent.OrderItem.Price
        };

        var table = Table.LoadTable(_client, _tableName);
        await table.PutItemAsync(document);

        return orderEvent;
    }

    public async Task<OrderEvent?> GetByIdAsync(Guid id)
    {
        var table = Table.LoadTable(_client, _tableName);
        var document = await table.GetItemAsync(id.ToString());

        if (document == null) return null;

        return new OrderEvent
        {
            OrderId = Guid.Parse(document["OrderId"]),
            OrderDate = DateTime.Parse(document["OrderDate"]),
            Status = Enum.Parse<OrderStatus>(document["Status"]),
            CreatedBy = document["CreatedBy"],
            OrderItem = new OrderItem
            {
                ItemName = document["ItemName"],
                Quantity = int.Parse(document["Quantity"]),
                Price = decimal.Parse(document["Price"])
            }
        };
    }

    public async Task UpdateStatusAsync(Guid id, OrderStatus status)
    {
        var request = new UpdateItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "OrderId", new AttributeValue { S = id.ToString() } }
            },
            UpdateExpression = "SET #s = :status",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#s", "Status" }
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":status", new AttributeValue { S = status.ToString() } }
            }
        };

        await _client.UpdateItemAsync(request);
    }

    public async Task CancelAsync(Guid id)
    {
        await UpdateStatusAsync(id, OrderStatus.Canceled);
    }
}
