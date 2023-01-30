using Newtonsoft.Json;

namespace GoFileWrapper.Models;

public class UploadResponse
{
    [JsonProperty("status")] public string? Status { get; set; }

    [JsonProperty("data")] public FileData? Data { get; set; }
}

public class FileData
{
    [JsonProperty("downloadPage")] public Uri? DownloadPage { get; set; }

    [JsonProperty("code")] public string? Code { get; set; }

    [JsonProperty("parentFolder")] public Guid ParentFolder { get; set; }

    [JsonProperty("fileId")] public Guid FileId { get; set; }

    [JsonProperty("fileName")] public string? FileName { get; set; }

    [JsonProperty("md5")] public string? Md5 { get; set; }
}