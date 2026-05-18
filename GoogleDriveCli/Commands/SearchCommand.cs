using GoogleDriveCli.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleDriveCli.Commands;
public class SearchCommand : ICommand
{
    private readonly GoogleDriveService _service;
    public SearchCommand(GoogleDriveService service)
    {
        _service = service;
    }
    public async Task Execute(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide a search query.");
            return;
        }

        string query = string.Join(" ", args);

        await _service.SearchFilesAsync(query);
    }
}
