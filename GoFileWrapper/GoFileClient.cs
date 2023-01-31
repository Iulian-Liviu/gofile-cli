// GoFileClient is a client for making API calls to the GoFile service.
// It uses the RestSharp library for making HTTP requests.

using GoFileWrapper.Models;
using Newtonsoft.Json;
using RestSharp;

namespace GoFileWrapper;

public class GoFileClient : IGoFileClient
{
  // URL format for uploading files
  private const string UploadUrlFormat = "https://{0}.gofile.io/uploadFile";

  // URL format for getting account details
  private const string AccountUrlFormat = "https://api.gofile.io/getAccountDetails?token={0}";

  // RestClient instance for making API calls
  private readonly RestClient _restClient;

  // URL for getting available server
  private readonly Uri _serverUrl = new("https://api.gofile.io/getServer");

  // Constructor
  public GoFileClient()
  {
    _restClient = new RestClient();
  }

  // Get available server for uploading files
  public async Task<ServerResponse> GetAvailableServerAsync(CancellationToken token = default)
  {
    try
    {
      // Create a REST request
      var request = new RestRequest(_serverUrl);
      // Execute the request
      var response = await _restClient.ExecuteAsync(request, token);
      // Check if the response is valid
      if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
        return new ServerResponse { Status = "Error - Network problems or invalid response." };
      // Deserialize the response content
      var serverResponse = JsonConvert.DeserializeObject<ServerResponse>(response.Content);
      // Return the response or an error
      return serverResponse ?? new ServerResponse { Status = "Error - Network problems or invalid response." };
    }
    catch (Exception e)
    {
#if DEBUG
      Console.ForegroundColor = ConsoleColor.DarkMagenta;
      Console.WriteLine($"DEBUG : {e}");
      Console.ForegroundColor = ConsoleColor.White;
#endif
      return new ServerResponse { Status = $"Error - {e.Message}" };
    }
  }

  // Upload a file to the GoFile service
  public async Task<UploadResponse> UploadFileAsync(string filePath, string server, CancellationToken token = default,
    string accountToken = "", string folderId = "")
  {
    try
    {
      // Check if the file exists
      if (!File.Exists(filePath)) return new UploadResponse { Status = "Error - File doesn't exist." };

      // Create a REST request
      var request = new RestRequest(string.Format(UploadUrlFormat, server), Method.Post);
      request.AddHeader("Content-Type", "multipart/form-data");
      // Add account token as a query string parameter
      if (accountToken != "")
        request.AddParameter(Parameter.CreateParameter("accountToken", accountToken,
          ParameterType.QueryString));
      // Add folderId as a query string parameter
      if (folderId != "")
        request.AddParameter(Parameter.CreateParameter("folderId", folderId,
          ParameterType.QueryString));
      // Add the file to be uploaded
      request.AddFile(Path.GetFileNameWithoutExtension(filePath), filePath, "application/octet-stream");
// Execute the request
      var response = await _restClient.ExecuteAsync(request, token);
// Check if the response is valid
      if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
        return new UploadResponse { Status = "Error - Network problems or invalid response." };
// Deserialize the response content
      var uploadResponse = JsonConvert.DeserializeObject<UploadResponse>(response.Content);
// Return the response or an error
      return uploadResponse ?? new UploadResponse { Status = "Error - Network problems or invalid response." };
    }
    catch (Exception e)
    {
#if DEBUG
      Console.ForegroundColor = ConsoleColor.DarkMagenta;
      Console.WriteLine($"DEBUG : {e}");
      Console.ForegroundColor = ConsoleColor.White;
#endif
      return new UploadResponse { Status = $"Error - {e.Message}" };
    }
  }
  // Get account details
  public async Task<AccountResponse> GetAccountDetailsAsync(string accountToken, CancellationToken token = default)
  {
    try
    {
      // Create a REST request
      var request = new RestRequest(string.Format(AccountUrlFormat, accountToken));
      // Execute the request
      var response = await _restClient.ExecuteAsync(request, token);
      // Check if the response is valid
      if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
        return new AccountResponse { Status = "Error - Network problems or invalid response." };
      // Deserialize the response content
      var accountResponse = JsonConvert.DeserializeObject<AccountResponse>(response.Content);
      // Return the response or an error
      return accountResponse ?? new AccountResponse { Status = "Error - Network problems or invalid response." };
    }
    catch (Exception e)
    {
#if DEBUG
      Console.ForegroundColor = ConsoleColor.DarkMagenta;
      Console.WriteLine($"DEBUG : {e}");
      Console.ForegroundColor = ConsoleColor.White;
#endif
      return new AccountResponse { Status = $"Error - {e.Message}" };
    }
  }
}