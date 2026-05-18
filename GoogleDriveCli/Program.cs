using GoogleDriveCli.Commands;
using GoogleDriveCli.Services;

var auth = new AuthService();
var driveService = await auth.AuthenticateAsync();
var googleDriveService = new GoogleDriveService(driveService);

var commands = new Dictionary<string, ICommand>
{
    { "sync", new SyncCommand(googleDriveService) },
    { "search", new SearchCommand(googleDriveService) },
    { "upload", new UploadCommand(googleDriveService) }
};

Console.WriteLine("Google Drive CLI");

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        continue;

    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    var commandName = parts[0].ToLower();
    var commandArgs = parts.Skip(1).ToArray();

    if (commandName == "exit")
        break;

    if (commands.TryGetValue(commandName, out var command))
    {
        await command.Execute(commandArgs);
    }
    else
    {
        Console.WriteLine("Unknown command");
    }
}