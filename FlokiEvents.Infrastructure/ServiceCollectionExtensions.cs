using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using FlokiEvents.Core.Interface;
using FlokiEvents.Core.Models;
using FlokiEvents.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlokiEvents.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFlokiInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var awsSettings = new AwsSettings();
        configuration.GetSection("AWS").Bind(awsSettings);
        services.Configure<AwsSettings>(configuration.GetSection("AWS"));
        
        var serviceUrl = configuration["Floci:ServiceURL"] ?? "http://localhost:4566";
        var region = RegionEndpoint.GetBySystemName(configuration["Floci:Region"] ?? "us-east-1");
        
        var credentials = new AnonymousAWSCredentials();
        
        services.AddSingleton<IAmazonS3>(sp => new AmazonS3Client(credentials, new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true
        }));

        services.AddSingleton<IAmazonSQS>(sp => new AmazonSQSClient(credentials, new AmazonSQSConfig
        {
            ServiceURL = serviceUrl
        }));

        services.AddSingleton<IAmazonSimpleNotificationService>(sp => new AmazonSimpleNotificationServiceClient(credentials, new AmazonSimpleNotificationServiceConfig
        {
            ServiceURL = serviceUrl
        }));

        services.AddSingleton<IAmazonDynamoDB>(sp => new AmazonDynamoDBClient(credentials, new AmazonDynamoDBConfig
        {
            ServiceURL = serviceUrl
        }));

        // Registra as serivices
        services.AddSingleton<IEventPublisher, SnsPublisher>();
        services.AddSingleton<IEventQueue, SqsQueue>();
        services.AddSingleton<IStorageService, S3StorageService>();
        services.AddSingleton<IOrderRepository, DynamoDbOrderRepository>();
        services.AddSingleton<InfrastructureSeeder>();

        return services;
    }
}
