using Amazon.S3;
using Amazon.S3.Model;
using FlokiEvents.Core.Interface;
using FlokiEvents.Core.Models;
using Microsoft.Extensions.Options;

namespace FlokiEvents.Infrastructure.Services;

public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _client;
    private readonly string _bucketName;

    public S3StorageService(IAmazonS3 client, IOptions<AwsSettings> settings)
    {
        _client = client;
        _bucketName = settings.Value.BucketName;
    }

    public async Task<Stream> UploadAsync(string key, Stream content)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = content
        };

        await _client.PutObjectAsync(request);
        return content;
    }

    public async Task<Stream> DownloadAsync(string key)
    {
        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        var response = await _client.GetObjectAsync(request);
        return response.ResponseStream;
    }
}
