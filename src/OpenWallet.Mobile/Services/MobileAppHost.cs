using OpenWallet.Shared.DTOs;
using OpenWallet.UI.Services;

namespace OpenWallet.Mobile.Services;

/// <summary>The mobile app's side of <see cref="IAppHost"/>: the phone's location, and attachments opened with other apps.</summary>
public sealed class MobileAppHost(AppHttp http) : IAppHost
{
    public bool IsNativeApp => true;

    public async Task<GeoCoords> GetCurrentPositionAsync()
    {
        PermissionStatus status = await MainThread.InvokeOnMainThreadAsync(Permissions.RequestAsync<Permissions.LocationWhenInUse>);
        if (status != PermissionStatus.Granted)
            throw new InvalidOperationException("Location access was denied.");

        Location? location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10)));
        return location == null
            ? throw new InvalidOperationException("Couldn't get your location.")
            : new GeoCoords(location.Latitude, location.Longitude);
    }

    /// <summary>Downloads the attachment into the app's cache and offers to open it with another app.</summary>
    public async Task OpenAttachmentAsync(int recordId, AttachmentDto attachment)
    {
        try
        {
            using HttpClient client = http.CreateToServer();
            using HttpResponseMessage response = await client.GetAsync($"api/records/{recordId}/attachments/{attachment.Id}");
            response.EnsureSuccessStatusCode();

            string folder = Path.Combine(FileSystem.CacheDirectory, "attachments", attachment.Id.ToString());
            Directory.CreateDirectory(folder);
            string fileName = Path.GetFileName(attachment.FileName);
            string path = Path.Combine(folder, string.IsNullOrWhiteSpace(fileName) ? "attachment" : fileName);
            await using (FileStream file = File.Create(path))
                await response.Content.CopyToAsync(file);

            await Launcher.Default.OpenAsync(new OpenFileRequest(attachment.FileName, new ReadOnlyFile(path, attachment.ContentType)));
        }
        catch (HttpRequestException)
        {
            await ShowErrorAsync("Couldn't download the attachment. Check your connection and try again.");
        }
    }

    static Task ShowErrorAsync(string message) => MainThread.InvokeOnMainThreadAsync(async () =>
    {
        if (Application.Current?.Windows.FirstOrDefault()?.Page is Page page)
            await page.DisplayAlertAsync("OpenWallet", message, "OK");
    });
}
