using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using FlokiEvents.Core.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace FlokiEvents.Infrastructure.Services;

public class InfrastructureSeeder
{
    private readonly IAmazonSQS _sqs;
    private readonly IAmazonSimpleNotificationService _sns;
    private readonly IAmazonS3 _s3;
    private readonly IAmazonDynamoDB _dynamo;
    private readonly AwsSettings _settings;
    private readonly ILogger<InfrastructureSeeder> _logger;

    public InfrastructureSeeder(
        IAmazonSQS sqs,
        IAmazonSimpleNotificationService sns,
        IAmazonS3 s3,
        IAmazonDynamoDB dynamo,
        IOptions<AwsSettings> settings,
        ILogger<InfrastructureSeeder> logger)
    {
        _sqs = sqs;
        _sns = sns;
        _s3 = s3;
        _dynamo = dynamo;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Creating infrastructure resources in Floci...");

        await CreateQueueAsync();
        await CreateTopicAsync();
        await CreateBucketAsync();
        await CreateTableAsync();

        _logger.LogInformation("All infrastructure resources created successfully.");
    }

    private async Task CreateQueueAsync()
    {
        try
        {
            var response = await _sqs.CreateQueueAsync(new CreateQueueRequest
            {
                QueueName = _settings.QueueName,
                Attributes = new Dictionary<string, string>
                {
                    { "VisibilityTimeout", "30" },
                    { "MessageRetentionPeriod", "86400" }
                }
            });

            _logger.LogInformation("SQS queue created: {Url}", response.QueueUrl);
        }
        catch (Amazon.SQS.Model.QueueNameExistsException)
        {
            _logger.LogInformation("SQS queue '{Queue}' already exists.", _settings.QueueName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not create SQS queue '{Queue}'.", _settings.QueueName);
        }
    }

    private async Task CreateTopicAsync()
    {
        try
        {
            var response = await _sns.CreateTopicAsync(new CreateTopicRequest
            {
                Name = _settings.TopicName
            });

            _settings.TopicArn = response.TopicArn;
            _logger.LogInformation("SNS topic created: {Arn}", response.TopicArn);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not create SNS topic '{Topic}'.", _settings.TopicName);
        }
    }

    private async Task CreateBucketAsync()
    {
        try
        {
            await _s3.PutBucketAsync(new PutBucketRequest
            {
                BucketName = _settings.BucketName
            });
            _logger.LogInformation("S3 bucket created: {Bucket}", _settings.BucketName);
        }
        catch (Amazon.S3.Model.BucketAlreadyOwnedByYouException)
        {
            _logger.LogInformation("S3 bucket '{Bucket}' already exists.", _settings.BucketName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not create S3 bucket '{Bucket}'.", _settings.BucketName);
        }
    }

    private async Task CreateTableAsync()
    {
        try
        {
            await _dynamo.CreateTableAsync(new CreateTableRequest
            {
                TableName = _settings.DynamoTableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition { AttributeName = "OrderId", AttributeType = "S" }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement { AttributeName = "OrderId", KeyType = KeyType.HASH }
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            });

            _logger.LogInformation("DynamoDB table created: {Table}", _settings.DynamoTableName);

            // Wait for table to become active
            bool isActive = false;
            int retries = 0;
            while (!isActive && retries < 30)
            {
                await Task.Delay(1000);
                var desc = await _dynamo.DescribeTableAsync(_settings.DynamoTableName);
                isActive = desc.Table.TableStatus == TableStatus.ACTIVE;
                retries++;
            }

            if (isActive)
                _logger.LogInformation("DynamoDB table is active.");
            else
                _logger.LogWarning("DynamoDB table creation timed out.");
        }
        catch (ResourceInUseException)
        {
            _logger.LogInformation("DynamoDB table '{Table}' already exists.", _settings.DynamoTableName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not create DynamoDB table '{Table}'.", _settings.DynamoTableName);
        }
    }
}
