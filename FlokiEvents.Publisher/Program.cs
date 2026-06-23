using FlokiEvents.Core.Interface;
using FlokiEvents.Core.Models;
using FlokiEvents.Infrastructure;
using FlokiEvents.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FlokiEvents.Publisher;

public class PublisherWorker : BackgroundService
{
    private readonly ILogger<PublisherWorker> _logger;
    private readonly IEventQueue _eventQueue;
    private readonly IEventPublisher _eventPublisher;
    private readonly int _intervalSeconds;
    private readonly Random _random = new();

    private static readonly string[] SampleProducts =
    {
        "Wireless Mouse", "USB-C Hub", "Mechanical Keyboard", "Monitor Stand",
        "Webcam HD", "Noise Cancelling Headphones", "Desk Lamp LED", "Laptop Stand",
        "External SSD 1TB", "Bluetooth Speaker"
    };

    private static readonly string[] SampleUsers =
    {
        "pedro.branks", "ana.silva", "carlos.souza", "maria.oliveira", "joao.santos"
    };

    public PublisherWorker(
        ILogger<PublisherWorker> logger,
        IEventQueue eventQueue,
        IEventPublisher eventPublisher,
        IConfiguration configuration)
    {
        _logger = logger;
        _eventQueue = eventQueue;
        _eventPublisher = eventPublisher;
        _intervalSeconds = configuration.GetValue<int>("Publisher:IntervalSeconds", 60);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Test Publisher started. Sending a message every {Interval}s...", _intervalSeconds);

        int messageCount = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                messageCount++;
                var orderEvent = GenerateTestOrder(messageCount);

                await _eventQueue.SendMessageAsync(orderEvent);
                _logger.LogInformation(
                    "[{Count}] Sent order {OrderId} to SQS (product={Product}, user={User}, price={Price:C})",
                    messageCount, orderEvent.OrderId, orderEvent.OrderItem.ItemName,
                    orderEvent.CreatedBy, orderEvent.OrderItem.Price);

                await _eventPublisher.PublishAsync(orderEvent);
                _logger.LogInformation(
                    "[{Count}] Published order {OrderId} to SNS",
                    messageCount, orderEvent.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message #{Count}", messageCount);
            }

            await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken);
        }
    }

    private OrderEvent GenerateTestOrder(int count)
    {
        var now = DateTime.UtcNow;

        return new OrderEvent
        {
            OrderId = Guid.NewGuid(),
            OrderDate = now,
            Status = OrderStatus.Created,
            CreatedBy = SampleUsers[_random.Next(SampleUsers.Length)],
            OrderItem = new OrderItem
            {
                ItemId = Guid.NewGuid(),
                ItemName = $"{SampleProducts[_random.Next(SampleProducts.Length)]} (#{count})",
                Quantity = _random.Next(1, 5),
                Price = Math.Round((decimal)(_random.NextDouble() * 200 + 10), 2),
                CreatedAt = now,
                UpdatedAt = now
            }
        };
    }
}


public class SeederHostedService : IHostedService
{
    private readonly InfrastructureSeeder _seeder;
    private readonly ILogger<SeederHostedService> _logger;

    public SeederHostedService(InfrastructureSeeder seeder, ILogger<SeederHostedService> logger)
    {
        _seeder = seeder;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running infrastructure seeder...");
        await _seeder.SeedAsync();
        _logger.LogInformation("Infrastructure seeding complete.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((ctx, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false);
                config.AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true);
            })
            .ConfigureServices((ctx, services) =>
            {
                //Roda o seeder primeiro, depois o publisher, para preparar as publicações
                services.AddFlokiInfrastructure(ctx.Configuration);
                services.AddHostedService<SeederHostedService>();
                services.AddHostedService<PublisherWorker>();      
            })
            .Build();

        await host.RunAsync();
    }
}
