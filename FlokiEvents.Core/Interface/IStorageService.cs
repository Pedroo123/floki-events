namespace FlokiEvents.Core.Interface;

public interface IStorageService
{
    public Task UploadAsync(string key, Stream content);
    public Task DownloadAsync(string key);
}