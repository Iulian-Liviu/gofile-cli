using GoFileWrapper.Models;
using Newtonsoft.Json;
using RestSharp;

namespace GoFileWrapper;

public class GoFileClient : IGoFileClient
{
    private const string UploadUrlFormat = "https://{0}.gofile.io/uploadFile";
    private const string AccountUrlFormat = "https://api.gofile.io/getAccountDetails?token={0}";
    private readonly RestClient _restClient;
    private readonly Uri _serverUrl = new("https://api.gofile.io/getServer");

    public GoFileClient()
    {
        _restClient = new RestClient();
    }

    public async Task<ServerResponse> GetAvailableServerAsync(CancellationToken token = default)
    {
        try
        {
            var request = new RestRequest(_serverUrl);
            var response = await _restClient.ExecuteAsync(request, token);
            if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
                return new ServerResponse { Status = "Error - Network problems or invalid response." };
            var serverResponse = JsonConvert.DeserializeObject<ServerResponse>(response.Content);
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

    public async Task<UploadResponse> UploadFileAsync(string filePath, string server, CancellationToken token = default,
        string accountToken = "", string folderId = "")
    {
        try
        {
            if (!File.Exists(filePath)) return new UploadResponse { Status = "Error - File doesn't exist." };

            var request = new RestRequest(string.Format(UploadUrlFormat, server), Method.Post);
            request.AddHeader("Content-Type", "multipart/form-data");
            if (accountToken != "")
                request.AddParameter(Parameter.CreateParameter("accountToken", accountToken,
                    ParameterType.QueryString));
            if (folderId != "")
                request.AddParameter(Parameter.CreateParameter("folderId", folderId,
                    ParameterType.QueryString));
            request.AddFile(Path.GetFileNameWithoutExtension(filePath), filePath);
            request.AlwaysMultipartFormData = true;
            var response = await _restClient.PostAsync<UploadResponse>(request, token);

            return response ?? new UploadResponse { Status = "Error - Error on upload, something went wrong." };
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

    public async Task<AccountResponse> GetAccountInfoAsync(string accountToken, CancellationToken token)
    {
        try
        {
            if (string.IsNullOrEmpty(accountToken)) return new AccountResponse { Status = "Error - Empty token." };
            var request = new RestRequest(string.Format(AccountUrlFormat, accountToken));
            var response = await _restClient.ExecuteAsync(request, token);
            if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
                return new AccountResponse { Status = "Error - Network problems or invalid response." };
            var accountResponse = JsonConvert.DeserializeObject<AccountResponse>(response.Content);
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