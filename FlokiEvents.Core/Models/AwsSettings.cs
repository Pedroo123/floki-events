namespace FlokiEvents.Core.Models;

public class AwsSettings
{
    public string BucketName { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public string QueueUrl { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public string TopicArn { get; set; } = string.Empty;
    public string DynamoTableName { get; set; } = string.Empty;
}
