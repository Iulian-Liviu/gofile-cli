using System.Reflection;
using System.Data.Common;
using System.Net.NetworkInformation;
using CommandLine;
using ConsoleTables;
using GoFileWrapper;
using GoFileWrapper.Models;
using Newtonsoft.Json.Linq;

[assembly: AssemblyVersion("1.0.0.*")]

namespace GoFileConsole;

public class Program
{
    private static readonly GoFileClient _client = new();
    private static readonly CancellationTokenSource _cancellationTokenSource = new();
    private static bool _isRunning = true;


    public static async Task Main(string[] args)
    {
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            Console.WriteLine("CTRL + C was pressed.");
            //_isRunning = false;

            _cancellationTokenSource.Cancel();
            WriteInfo("CANCELED : by USER");
            _isRunning = false;
        };
        var result = await
            (await Parser.Default
                        .ParseArguments<UploadArguments, AccountArguments>(args)
                        .WithParsedAsync<UploadArguments>(options => UploadFile(options)))
                    .WithParsedAsync<AccountArguments>(options => GetAccountInformation(options));
        
    }

    private static async Task UploadFile(UploadArguments uploadArguments)
    {
        while (_isRunning)
            if (uploadArguments.Files.Any())
            {
                var uploadedFiles = new List<FileData>();

                foreach (var file in uploadArguments.Files)
                {
                    var server = await _client.GetAvailableServerAsync(_cancellationTokenSource.Token);
                    WriteInfo($"Uploading {uploadArguments.Files.Count()} file.");
                    WriteInfo($"server {server.Data!.Server} : status {server.Status}");
                    if (server.Status!.Contains("ok"))
                    {
                        WriteInfo($"Uploading {Path.GetFileNameWithoutExtension(file)}");
                        if (string.IsNullOrEmpty(uploadArguments.AccountToken))
                        {
                            WriteInfo($"Uploading {Path.GetFileNameWithoutExtension(file)} on the account token {uploadArguments.AccountToken} .");

                        }

                        if (string.IsNullOrEmpty(uploadArguments.FolderId))
                        {
                            WriteInfo($"Uploading {Path.GetFileNameWithoutExtension(file)} in the folder with id : {uploadArguments.FolderId} .");

                        }

                        var fileResponse =
                            await _client.UploadFileAsync(file, server.Data?.Server!, _cancellationTokenSource.Token, uploadArguments.AccountToken, uploadArguments.FolderId);
                        if (fileResponse.Status == "ok")
                        {
                            uploadedFiles.Add(fileResponse.Data!);
                            WriteSuccess($"Uploading {Path.GetFileNameWithoutExtension(file)}.");
                        }
                        else
                        {
                            WriteError(fileResponse.Status!);
                        }
                    }
                    else
                    {
                        WriteError(server.Status);
                    }
                }

                if (uploadedFiles.Any())
                {
                    WriteUploadedFilesTable(uploadedFiles.ToArray());
                    WriteInfo($"Uploaded with success {uploadedFiles.Count} files.");

                    _isRunning = false;
                }

                _isRunning = false;
            }
            else
            {
                WriteInfo("No files were passed.");
                _isRunning = false;
            }
    }

    private static async Task GetAccountInformation(AccountArguments accountArguments)
    {
        while (_isRunning)
        {
            var account = await _client.GetAccountInfoAsync(accountArguments.Token!, _cancellationTokenSource.Token);
            if (account.Status == "ok")
            {
                WriteAccountTable(new [] {account.Data!});
                _isRunning = false;
            }
            else
            {
                WriteInfo(account.Status!);
                _isRunning = false;

            }
            _isRunning = false;
        }
    }

    #region Console Writers

    private static void WriteSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"\t[!] SUCCESS : {message} at {DateTime.Today.ToLongTimeString()}.");
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static void WriteInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[!] INFO : {message} at {DateTime.Today.ToLongTimeString()}.");
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static void WriteError(string status)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\t[!] ERROR : {status} at {DateTime.Today.ToLongTimeString()}.");
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static void WriteUploadedFilesTable(FileData[] datas)
    {
        ConsoleTable
            .From<FileData>(datas)
            .Configure(o =>
            {
                o.NumberAlignment = Alignment.Left;
            })
            .Write(Format.MarkDown);

    }
    private static void WriteAccountTable(AccountData[] datas)
    {
        Console.WriteLine("\n");
        ConsoleTable
            .From<AccountData>(datas)
            .Configure(o =>
            {
                o.NumberAlignment = Alignment.Left;
                o.OutputTo = Console.Out;
            })
            .Write(Format.MarkDown);
        Console.WriteLine("\n");

    }

    #endregion

    #region Console Arguments Classes

    [Verb("upload", false, HelpText = "The upload command")]
    private class UploadArguments
    {
        [Option('f', "files", HelpText = "The files that you want to upload.", Required = true)]
        // ReSharper disable once CollectionNeverUpdated.Local
#pragma warning disable CS8618
        public IEnumerable<string> Files { get; set; }

        [Option(shortName:'t', longName:"token", HelpText = "Adds the file to an account using the account token.")]
        public string AccountToken { get; set; }

        [Option(shortName:'i', "folder-id", HelpText = "Uploads the files to a folder if specified by the user.")]
        public string FolderId { get; set; }
#pragma warning restore CS8618
    }

    [Verb("account", HelpText = "Get information about an account using an account token.")]
    private class AccountArguments
    {
        [Option('t', "token", Required = true, HelpText= "The token of the account you want to get information.")]
        public string? Token { get; set; }  
    }
    
    #endregion

}