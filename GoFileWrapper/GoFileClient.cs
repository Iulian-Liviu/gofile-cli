using GoFileWrapper.Models;
using Newtonsoft.Json;
using RestSharp;

namespace GoFileWrapper;

public class GoFileClient : IGoFileClient
{
    private const string UploadUrlFormat = "https://{0}.gofile.io/uploadFile";
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
            if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
            {
                var serverResponse = JsonConvert.DeserializeObject<ServerResponse>(response.Content);
                if (serverResponse == null)
                    return new ServerResponse { Status = "Error - Network problems or invalid response." };
                return serverResponse;
            }

            return new ServerResponse { Status = "Error - Network problems or invalid response." };
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
        string accountToken = "")
    {
        try
        {
            if (!File.Exists(filePath)) return new UploadResponse { Status = "Error - File doesn't exist." };

            var request = new RestRequest(string.Format(UploadUrlFormat, server), Method.Post);
            request.AddHeader("Content-Type", "multipart/form-data");
            if (accountToken != "")
                request.AddParameter(Parameter.CreateParameter("accountToken", accountToken,
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
}