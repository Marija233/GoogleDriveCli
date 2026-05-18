GoogleDriveCli is a .NET command-line application that integrates with the Google Drive API and provides functionality for authentication, file synchronization, search, and upload operations. To build and run the application, the repository should first be cloned, then the dependencies should be restored using dotnet restore, followed by building the project with dotnet build, and running it using dotnet run. 

The client_secret.json file should be placed in the root directory of the project (GoogleDriveCli folder) so that OAuth authentication can be performed. On first execution, the application will prompt the user to log in with a Google account and will store the authentication token locally using a FileDataStore named token_store, allowing automatic authentication in future runs without requiring repeated login. 

The application follows a clean layered architecture where the command layer is responsible for parsing CLI input and executing commands (sync, search, upload), while the service layer encapsulates all Google Drive API interactions and authentication logic, ensuring separation of concerns and maintainability. The sync functionality is implemented using asynchronous programming with parallel execution via Task.WhenAll, allowing multiple file downloads to run concurrently and significantly improving performance when handling large numbers of files. To ensure correctness in a concurrent environment, shared counters for successful and failed downloads are updated using Interlocked.Increment, which provides thread-safe operations and prevents race conditions during parallel execution. 

The sync command downloads all files from Google Drive to the local Downloads folder by running 
sync

The search command searches Google Drive files by name and shows whether each file is already downloaded locally by running
search <query>

Example:
search pdf 

Output example:
Found 20 matching files:
file1.pdf [Downloaded]
file2.pdf [Not Downloaded]

The upload command uploads a local file to Google Drive. The local file path and the target Drive folder path must be provided, by running
upload <local_path> <drive_path>
