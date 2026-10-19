namespace OpenWallet.Mobile.Core.Lock;

/// <summary>
/// Whether the app's content may be shown. The app starts locked, locks again after
/// <see cref="GracePeriod"/> in the background, and is hidden whenever it isn't on screen.
/// </summary>
public sealed class AppLockState(TimeProvider timeProvider)
{
    public static readonly TimeSpan GracePeriod = TimeSpan.FromMinutes(2);

    DateTimeOffset? backgroundedAt;

    /// <summary>Unlocking (biometrics or the device PIN) is needed before the content is shown.</summary>
    public bool IsLocked { get; private set; } = true;

    public bool IsInBackground => backgroundedAt != null;

    /// <summary>Whether the content must be covered: locked, or not on screen.</summary>
    public bool IsHidden => IsLocked || IsInBackground;

    /// <summary>Whether to ask the user to unlock now: locked and on screen.</summary>
    public bool NeedsUnlock => IsLocked && !IsInBackground;

    public event Action? Changed;

    public void Lock()
    {
        if (IsLocked) return;
        IsLocked = true;
        Changed?.Invoke();
    }

    public void Unlock()
    {
        if (!IsLocked) return;
        IsLocked = false;
        Changed?.Invoke();
    }

    public void OnBackground()
    {
        if (backgroundedAt != null) return;
        backgroundedAt = timeProvider.GetUtcNow();
        Changed?.Invoke();
    }

    public void OnForeground()
    {
        if (backgroundedAt is not DateTimeOffset since) return;
        if (timeProvider.GetUtcNow() - since >= GracePeriod)
            IsLocked = true;
        backgroundedAt = null;
        Changed?.Invoke();
    }
}
