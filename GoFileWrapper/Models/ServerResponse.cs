using Newtonsoft.Json;

namespace GoFileWrapper.Models;

public class ServerResponse
{
    [JsonProperty("status")] public string? Status { get; set; }

    [JsonProperty("data")] public ServerData? Data { get; set; }
}

public class ServerData
{
    [JsonProperty("server")] public string? Server { get; set; }
}