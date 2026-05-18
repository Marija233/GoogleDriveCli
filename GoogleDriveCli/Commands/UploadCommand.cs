using GoogleDriveCli.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleDriveCli.Commands;

public class UploadCommand : ICommand
{
    private readonly GoogleDriveService _service;

    public UploadCommand(GoogleDriveService service)
    {
        _service = service;
    }

    public async Task Execute(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: upload [local_path] [drive_folder_path]");
            return;
        }

        var localPath = args[0];
        var drivePath = args[1];

        await _service.UploadFileAsync(localPath, drivePath);
    }
}