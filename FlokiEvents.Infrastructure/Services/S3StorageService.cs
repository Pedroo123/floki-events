using Amazon.S3;
using Amazon.S3.Model;
using FlokiEvents.Core.Interface;

namespace FlokiEvents.Infrastructure.Services;

public class S3StorageService : IStorageService
{
    const string bucketName = "floki-events";
    const string objectKey = "floki-events-object-key.txt";
    
    //TTL for presigned URL, in hours
    private const double timeoutDuration = 12;

    
    
    
    private static string GeneratePresignedURL(IAmazonS3 client,
        string bucketName, string objectKey)
    {
        string urlString = string.Empty;
        try
        {
            var request = new GetPreSignedUrlRequest()
            {
                BucketName = bucketName,
                Key = objectKey,
                Expires = DateTime.UtcNow.AddHours(timeoutDuration),
            };
            urlString = client.GetPreSignedURL(request);
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        return urlString;
    }
}