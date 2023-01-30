using BetterConsoles.Tables;
using BetterConsoles.Tables.Configuration;
using CommandLine;
using GoFileWrapper;
using GoFileWrapper.Models;

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
        var parser = new Parser(config => config.HelpWriter = Console.Out);
        parser.Settings.AutoHelp = true;

        await parser.ParseArguments<UploadArguments>(args)
            .WithParsedAsync(arguments => UploadFile(arguments));
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
                        WriteInfo($"Uploading {Path.GetFileNameWithoutExtension(file)}.");
                        var fileResponse =
                            await _client.UploadFileAsync(file, server.Data?.Server!, _cancellationTokenSource.Token);
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
                    WriteTable(uploadedFiles.ToArray());
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

    private static void WriteSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"\t[!] SUCCESS : {message}");
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static void WriteInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[!] INFO : {message}");
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static void WriteError(string status)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\t[!] ERROR : {status}");
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static void WriteTable(FileData[] datas)
    {
        var table = new Table(TableConfig.Simple()).From(datas);
        Console.WriteLine(table.ToString());
    }

    [Verb("upload", true, HelpText = "The upload command")]
    private class UploadArguments
    {
        [Option('f', "files", HelpText = "The files that you want to upload.", Required = true)]
        // ReSharper disable once CollectionNeverUpdated.Local
#pragma warning disable CS8618
        public IEnumerable<string> Files { get; set; }
#pragma warning restore CS8618
    }
}