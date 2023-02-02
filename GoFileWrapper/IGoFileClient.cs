// This is an interface for the GoFileClient class that provides the contract for the API calls to the GoFile service.
// It defines three methods: GetAvailableServerAsync, UploadFileAsync, and GetAccountDetailsAsync.

using GoFileWrapper.Models;

namespace GoFileWrapper;

public interface IGoFileClient
{
// Get available server for uploading files
    Task<ServerResponse> GetAvailableServerAsync(CancellationToken token = default);

// Upload a file to the GoFile service
    Task<UploadResponse> UploadFileAsync(string filePath, string server,
        string accountToken = "", string folderId = "", CancellationToken token = default);

// Get account details for the given token
    Task<AccountResponse> GetAccountDetailsAsync(string accountToken, CancellationToken token);
}