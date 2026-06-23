using FlokiEvents.Core.Interface;
using FlokiEvents.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FlokiEvents.Processor;

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
                services.AddFlokiInfrastructure(ctx.Configuration);
                services.AddSingleton<OrderProcessorFunction>();
            })
            .Build();

        var processor = host.Services.GetRequiredService<OrderProcessorFunction>();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        var eventQueue = host.Services.GetRequiredService<IEventQueue>();

        logger.LogInformation("Lambda Processor started (local mode). Polling SQS for messages...");

        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        while (!cts.Token.IsCancellationRequested)
        {
            try
            {
                var messages = await eventQueue.ReceiveMessageAsync(10);

                foreach (var message in messages)
                {
                    try
                    {
                        await processor.ProcessOrder(message.OrderEvent);
                        await eventQueue.DeleteMessageAsync(message.ReceiptHandle);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing order {OrderId}", message.OrderEvent.OrderId);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Processor shutting down...");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error polling messages");
                try { await Task.Delay(5000, cts.Token); } catch { break; }
            }
        }
    }
}
