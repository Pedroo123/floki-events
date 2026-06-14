namespace FlokiEvents.Core.Interface;

public interface IStorageService
{
    public Task<Stream> UploadAsync(string key, Stream content);
    public Task<Stream> DownloadAsync(string key);
}