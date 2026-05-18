using GoogleDriveCli.Services;

namespace GoogleDriveCli.Commands;

public class SyncCommand : ICommand
{
    private readonly GoogleDriveService _service;
    public SyncCommand(GoogleDriveService service)
    {
        _service = service;
    }

    public async Task Execute(string[] args)
    {
        await _service.SyncFilesAsync();
    }
}