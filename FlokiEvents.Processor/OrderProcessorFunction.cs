using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using FlokiEvents.Core.Interface;
using FlokiEvents.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FlokiEvents.Processor;

public class OrderProcessorFunction
{
    private readonly IOrderRepository _orderRepository;
    private readonly IStorageService _storageService;
    private readonly IEventQueue _eventQueue;

    public OrderProcessorFunction(IOrderRepository orderRepository, IStorageService storageService, IEventQueue eventQueue)
    {
        _orderRepository = orderRepository;
        _storageService = storageService;
        _eventQueue = eventQueue;
    }
    
    public async Task FunctionHandler(SNSEvent snsEvent, ILambdaContext context)
    {
        foreach (var record in snsEvent.Records)
        {
            var orderEvent = JsonSerializer.Deserialize<OrderEvent>(record.Sns.Message);
            if (orderEvent == null) continue;

            context.Logger.LogLine($"Processing order {orderEvent.OrderId}");

            await ProcessOrder(orderEvent);
        }
    }

    public async Task ProcessOrder(OrderEvent orderEvent)
    {
        Console.WriteLine($"[Processor] Processing order {orderEvent.OrderId}");
        
        var json = JsonSerializer.Serialize(orderEvent);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var key = $"processed/{orderEvent.OrderId}/{DateTime.UtcNow:yyyyMMddHHmmss}.json";

        using var stream = new MemoryStream(bytes);
        await _storageService.UploadAsync(key, stream);
        Console.WriteLine($"[Processor] Stored order at S3 key: {key}");

        await _orderRepository.SaveAsync(orderEvent);
        Console.WriteLine($"[Processor] Order saved to DynamoDB");

        await _orderRepository.UpdateStatusAsync(orderEvent.OrderId, OrderStatus.Processed);

        Console.WriteLine($"[Processor] Order {orderEvent.OrderId} processed successfully");
    }
}
