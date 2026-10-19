#if ANDROID
using AndroidX.Biometric;
using AndroidX.Core.Content;
using AndroidX.Fragment.App;
#endif

namespace OpenWallet.Mobile.Services;

public enum DeviceUnlockResult
{
    Unlocked,
    Cancelled,

    /// <summary>The device has no screen lock (fingerprint, face, PIN, pattern or password) to check.</summary>
    Unavailable
}

/// <summary>Asks the user to prove it's them with the phone's own screen lock: biometrics, or the PIN as a fallback.</summary>
public sealed class DeviceUnlock
{
#if ANDROID
    const int Authenticators = BiometricManager.Authenticators.BiometricWeak | BiometricManager.Authenticators.DeviceCredential;
#endif

    public Task<DeviceUnlockResult> UnlockAsync() => MainThread.InvokeOnMainThreadAsync(Prompt);

#if ANDROID
    static Task<DeviceUnlockResult> Prompt()
    {
        if (Platform.CurrentActivity is not FragmentActivity activity
            || ContextCompat.GetMainExecutor(activity) is not Java.Util.Concurrent.IExecutor executor
            || BiometricManager.From(activity).CanAuthenticate(Authenticators) != BiometricManager.BiometricSuccess)
            return Task.FromResult(DeviceUnlockResult.Unavailable);

        TaskCompletionSource<DeviceUnlockResult> result = new();
        BiometricPrompt prompt = new(activity, executor, new Callback(result));
        BiometricPrompt.PromptInfo info = new BiometricPrompt.PromptInfo.Builder()
            .SetTitle("Unlock OpenWallet")
            .SetAllowedAuthenticators(Authenticators)
            .Build();
        prompt.Authenticate(info);
        return result.Task;
    }

    sealed class Callback(TaskCompletionSource<DeviceUnlockResult> result) : BiometricPrompt.AuthenticationCallback
    {
        public override void OnAuthenticationSucceeded(BiometricPrompt.AuthenticationResult authResult) =>
            result.TrySetResult(DeviceUnlockResult.Unlocked);

        public override void OnAuthenticationError(int errorCode, Java.Lang.ICharSequence errString) =>
            result.TrySetResult(DeviceUnlockResult.Cancelled);
    }
#else
    static Task<DeviceUnlockResult> Prompt() => Task.FromResult(DeviceUnlockResult.Unavailable);
#endif
}
