using Google.Apis.Drive.v3;
using Google.Apis.Services;

namespace GoogleDriveCli.Services;

public class GoogleDriveService
{
    private readonly DriveService _driveService;

    public GoogleDriveService(DriveService driveService)
    {
        _driveService = driveService;
    }

    public async Task ListFilesAsync()
    {
        var request = _driveService.Files.List();

        request.PageSize = 10;
        request.Fields = "files(id, name)";

        var result = await request.ExecuteAsync();

        foreach (var file in result.Files)
        {
            Console.WriteLine($"{file.Name} ({file.Id})");
        }
    }

    public async Task SyncFilesAsync()
    {
        var request = _driveService.Files.List();
        request.Fields = "files(id, name, mimeType)";
        var result = await request.ExecuteAsync();

        var files = result.Files;

        var downloadPath = Path.Combine(Environment.CurrentDirectory, "Downloads");
        Directory.CreateDirectory(downloadPath);

        int total = files.Count;
        int success = 0;
        int failed = 0;

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        Console.WriteLine("Building download tasks...");
      
        var tasks = files.Select(async file =>
        {
            Console.WriteLine($"Processing: {file.Name} | {file.MimeType}");

            if (file.MimeType == "application/vnd.google-apps.folder")
            {
                Console.WriteLine($"Skipping folder: {file.Name}");

                Interlocked.Increment(ref failed);

                return;
            }

            if (file.MimeType.StartsWith("application/vnd.google-apps"))
            {
                Console.WriteLine($"Skipping Google file: {file.Name}");

                Interlocked.Increment(ref failed);

                return;
            }

            try
            {
                var filePath = Path.Combine(downloadPath, file.Name);

                var getRequest = _driveService.Files.Get(file.Id);

                using var stream = new MemoryStream();

                await getRequest.DownloadAsync(stream);

                await File.WriteAllBytesAsync(filePath, stream.ToArray());

                Console.WriteLine($"Downloaded: {file.Name}");

                Interlocked.Increment(ref success);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAILED: {file.Name} -> {ex.Message}");

                Interlocked.Increment(ref failed);
            }
        });
        Console.WriteLine("Starting parallel download...");
        await Task.WhenAll(tasks);
        Console.WriteLine("Finished Task.WhenAll");
 
        stopwatch.Stop();

        Console.WriteLine("===== SYNC COMPLETE =====");
        Console.WriteLine($"Total files: {total}");
        Console.WriteLine($"Successful: {success}");
        Console.WriteLine($"Failed: {failed}");
        Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
    }
    public async Task SearchFilesAsync(string query)
    {
        var request = _driveService.Files.List();

        request.Q = $"name contains '{query}'";

        request.Fields = "files(id, name)";

        var result = await request.ExecuteAsync();

        if (result.Files.Count == 0)
        {
            Console.WriteLine("No matching files found.");
            return;
        }

        Console.WriteLine($"Found {result.Files.Count} matching files:");

        var downloadFolder = Path.Combine(AppContext.BaseDirectory, "Downloads");

        foreach (var file in result.Files)
        {
            string status;

            var localPath = Path.Combine(downloadFolder, file.Name);

            if (File.Exists(localPath))
                status = "[Downloaded]";
            else
                status = "[Not Downloaded]";

            Console.WriteLine($"{file.Name} {status} ({file.Id})");
        }
    }

    public async Task UploadFileAsync(string localPath, string drivePath)
    {
        if (!File.Exists(localPath))
        {
            Console.WriteLine("Local file not found.");
            return;
        }

        var folders = drivePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        string parentId = "root";

        foreach (var folder in folders)
        {
            parentId = await GetOrCreateFolder(folder, parentId);
        }
        
        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = Path.GetFileName(localPath),
            Parents = new List<string> { parentId }
        };

        using var stream = new FileStream(localPath, FileMode.Open);

        var uploadRequest = _driveService.Files.Create(fileMetadata, stream, "application/octet-stream");
        uploadRequest.Fields = "id";

        await uploadRequest.UploadAsync();

        Console.WriteLine("Upload completed successfully.");
    }

    private async Task<string> GetOrCreateFolder(string folderName, string parentId = "root")
    {
        var request = _driveService.Files.List();

        request.Q = $"mimeType='application/vnd.google-apps.folder' and name='{folderName}' and '{parentId}' in parents";

        request.Fields = "files(id, name)";

        var result = await request.ExecuteAsync();

        var folder = result.Files.FirstOrDefault();

        if (folder != null)
            return folder.Id;

        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = folderName,
            MimeType = "application/vnd.google-apps.folder",
            Parents = new List<string> { parentId }
        };

        var createRequest = _driveService.Files.Create(fileMetadata);
        createRequest.Fields = "id";

        var created = await createRequest.ExecuteAsync();

        return created.Id;
    }
}