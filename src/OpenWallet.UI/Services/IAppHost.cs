using OpenWallet.Shared.DTOs;

namespace OpenWallet.UI.Services;

public record GeoCoords(double Latitude, double Longitude);

/// <summary>What differs between the web app and the mobile app, beyond sign-in.</summary>
public interface IAppHost
{
    /// <summary>True in the mobile app.</summary>
    bool IsNativeApp { get; }

    /// <summary>The device's location. Throws with a message for the user when it isn't available.</summary>
    Task<GeoCoords> GetCurrentPositionAsync();

    /// <summary>Saves an attachment in the browser, or opens it with another app on the phone.</summary>
    Task OpenAttachmentAsync(int recordId, AttachmentDto attachment);
}
