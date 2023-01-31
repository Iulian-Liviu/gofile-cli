namespace GoFileWrapper.Models
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class AccountResponse
    {
        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("data")]
        public AccountData? Data { get; set; }
    }

    public class AccountData
    {
        [JsonProperty("token")]
        public string? Token { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("tier")]
        public string? Tier { get; set; }

        [JsonProperty("rootFolder")]
        public Guid RootFolder { get; set; }

        [JsonProperty("foldersCount")]
        public long FoldersCount { get; set; }

        [JsonProperty("filesCount")]
        public long FilesCount { get; set; }

        [JsonProperty("totalSize")]
        public long TotalSize { get; set; }

        [JsonProperty("totalDownloadCount")]
        public long TotalDownloadCount { get; set; }
    }
}