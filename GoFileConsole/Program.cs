using System.Reflection;
using CommandLine;
using ConsoleTables;
using GoFileWrapper;
using GoFileWrapper.Models;

[assembly: AssemblyVersion("1.0.0.*")]
[assembly: AssemblyProduct("gofile-cli")]


namespace GoFileConsole;

// ReSharper disable once ClassNeverInstantiated.Global
public class Program
{
    private static readonly GoFileClient Client = new();
    private static readonly CancellationTokenSource CancellationTokenSource = new();
    private static bool _isRunning = true;


    public static async Task Main(string[] args)
    {
        Console.CancelKeyPress += (_, _) =>
        {
            Console.WriteLine("CTRL + C was pressed.");
            //_isRunning = false;

            CancellationTokenSource.Cancel();
            WriteInfo("CANCELED : by USER");
            _isRunning = false;
        };
        await
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
                    var server = await Client.GetAvailableServerAsync(CancellationTokenSource.Token);
                    WriteInfo($"Uploading {uploadArguments.Files.Count()} file.");
                    WriteInfo($"server {server.Data!.Server} : status {server.Status}");
                    if (server.Status!.Contains("ok"))
                    {
                        WriteInfo($"Uploading {Path.GetFileNameWithoutExtension(file)}");
                        if (!string.IsNullOrEmpty(uploadArguments.AccountToken))
                            WriteInfo(
                                $"Uploading {Path.GetFileNameWithoutExtension(file)} on the account token {uploadArguments.AccountToken} .");

                        if (!string.IsNullOrEmpty(uploadArguments.FolderId))
                            WriteInfo(
                                $"Uploading {Path.GetFileNameWithoutExtension(file)} in the folder with id : {uploadArguments.FolderId} .");

                        var fileResponse =
                            await Client.UploadFileAsync(file, server.Data?.Server!, CancellationTokenSource.Token,
                                uploadArguments.AccountToken, uploadArguments.FolderId);
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
            var account = await Client.GetAccountDetailsAsync(accountArguments.Token!, CancellationTokenSource.Token);
            if (account.Status == "ok")
            {
                WriteAccountTable(new[] { account.Data! });
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
        Console.WriteLine("\n");

        ConsoleTable
            .From(datas)
            .Configure(o => { o.NumberAlignment = Alignment.Left; })
            .Write(Format.MarkDown);
        Console.WriteLine("\n");

    }

    private static void WriteAccountTable(AccountData[] datas)
    {
        Console.WriteLine("\n");
        ConsoleTable
            .From(datas)
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

    [Verb("upload", HelpText = "The upload command")]
    private class UploadArguments
    {

        [Option('f', "files", HelpText = "The files that you want to upload.", Required = true)]
        // ReSharper disable once CollectionNeverUpdated.Local
#pragma warning disable CS8618
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public IEnumerable<string> Files { get; set; }

        [Option('t', "token", HelpText = "Adds the file to an account using the account token.")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string AccountToken { get; set; }

        [Option('i', "folder-id", HelpText = "Uploads the files to a folder if specified by the user.")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string FolderId { get; set; }
#pragma warning restore CS8618
    }

    [Verb("account", HelpText = "Get information about an account using an account token.")]
    private class AccountArguments
    {
        [Option('t', "token", Required = true, HelpText = "The token of the account you want to get information.")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string? Token { get; set; }
    }

    #endregion
}