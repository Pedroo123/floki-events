using System.Text.Json;
using FlokiEvents.Core.Interface;
using FlokiEvents.Core.Models;

namespace FlokiEvents.Consumer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IEventQueue _eventQueue;
    private readonly IOrderRepository _orderRepository;
    private readonly IStorageService _storageService;

    public Worker(
        ILogger<Worker> logger,
        IEventQueue eventQueue,
        IOrderRepository orderRepository,
        IStorageService storageService)
    {
        _logger = logger;
        _eventQueue = eventQueue;
        _orderRepository = orderRepository;
        _storageService = storageService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started. Listening for messages...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var messages = await _eventQueue.ReceiveMessageAsync(10);

                foreach (var message in messages)
                {
                    try
                    {
                        _logger.LogInformation("Processing order {OrderId}", message.OrderEvent.OrderId);

                        // DynamoDB
                        await _orderRepository.SaveAsync(message.OrderEvent);
                        _logger.LogInformation("Order {OrderId} saved to DynamoDB", message.OrderEvent.OrderId);

                        // Eventos noS3
                        var json = JsonSerializer.Serialize(message.OrderEvent);
                        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
                        var key = $"orders/{message.OrderEvent.OrderId}/{DateTime.UtcNow:yyyyMMddHHmmss}.json";
                        await _storageService.UploadAsync(key, stream);
                        _logger.LogInformation("Order {OrderId} stored in S3 at {Key}", message.OrderEvent.OrderId, key);
                        
                        await _orderRepository.UpdateStatusAsync(message.OrderEvent.OrderId, OrderStatus.Processed);

                        // Remove da fila
                        await _eventQueue.DeleteMessageAsync(message.ReceiptHandle);
                        _logger.LogInformation("Order {OrderId} processed successfully", message.OrderEvent.OrderId);
                    }
                    catch (Exception ex)
                    {
                        //Mensagem fica até o fim do TTL
                        _logger.LogError(ex, "Error processing order {OrderId}", message.OrderEvent.OrderId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving messages from queue");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
