using GoFileWrapper.Models;

namespace GoFileWrapper;

public interface IGoFileClient
{
    public Task<ServerResponse> GetAvailableServerAsync(CancellationToken token);

    public Task<UploadResponse> UploadFileAsync(string filePath, string server, CancellationToken token,
        string accountToken = "", string folderId = "");
    public Task<AccountResponse> GetAccountInfoAsync(string accountToken, CancellationToken token);
}