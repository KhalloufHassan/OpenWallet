namespace OpenWallet.UI.Services;

/// <summary>
/// Sign-in related actions that work differently per host: the web app uses its login page and a
/// session cookie; the mobile app its own sign-in page and tokens.
/// </summary>
public interface IAccountService
{
    /// <summary>Whether there may be a session worth asking the server about (the app knows it has none before choosing a server).</summary>
    Task<bool> HasSessionAsync();

    /// <summary>Send the user to sign in, coming back to the current page afterwards.</summary>
    void RedirectToLogin();

    Task SignOutAsync();

    /// <summary>Ends the session after inactivity; the login page then says why.</summary>
    Task LockAsync();

    /// <summary>Opens the security settings where passkeys can be added: in the browser, for the mobile app.</summary>
    void OpenSecuritySettings();
}
