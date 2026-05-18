using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace GoogleDriveCli.Services;

public class AuthService
{
    public async Task<DriveService> AuthenticateAsync()
    {
        using var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read);

        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            GoogleClientSecrets.FromStream(stream).Secrets,
            new[] { DriveService.Scope.Drive },
            "user",
            CancellationToken.None,
            new FileDataStore("token_store", true)
        );

        return new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "GoogleDriveCli"
        });
    }
}