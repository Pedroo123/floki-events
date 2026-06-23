using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using FlokiEvents.Core.Interface;

namespace FlokiEvents.Infrastructure.Services;

public class S3StorageService : IStorageService
{
    const string bucketName = "floki-events";
    const string objectKey = "floki-events-object-key.txt";
    
    //TTL for presigned URL, in hours
    private const double timeoutDuration = 12;

    public static async Task<Stream> UploadAsync(TransferUtility transferUtil,
        string localPath,
        Stream content)
    {
        if (File.Exists(localPath))
        {
            try
            {
                await transferUtil.UploadAsync(new TransferUtilityUploadRequest
                {
                    BucketName = bucketName,
                    Key = objectKey,
                    FilePath = localPath,
                });
                
                var contentBytes = ConvertStreamToByte(content);
                var buffer = content.WriteAsync(contentBytes);

                return buffer;

            } catch (AmazonS3Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
    }
    
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

    private static byte[] ConvertStreamToByte(Stream input)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }
}