using OpenWallet.UI.Services;

namespace OpenWallet.Mobile.Core.Auth;

public static class ServerAddress
{
    /// <summary>
    /// Turns what the user typed into a server address like "https://wallet.example.org/" (a trailing
    /// slash, so API paths resolve under it). Only HTTPS: the app never sends passwords or tokens in the clear.
    /// </summary>
    public static string Normalize(string? input)
    {
        string text = (input ?? string.Empty).Trim();
        if (text.Length == 0)
            throw new AppSignInException("Enter your server's address.");

        if (!text.Contains("://", StringComparison.Ordinal))
            text = "https://" + text;

        if (!Uri.TryCreate(text, UriKind.Absolute, out Uri? uri) || string.IsNullOrEmpty(uri.Host))
            throw new AppSignInException("That doesn't look like a web address.");

        if (uri.Scheme != Uri.UriSchemeHttps)
            throw new AppSignInException("The server must use HTTPS (an address starting with https://).");

        UriBuilder builder = new(uri) { Query = string.Empty, Fragment = string.Empty };
        if (!builder.Path.EndsWith('/'))
            builder.Path += "/";

        return builder.Uri.ToString();
    }
}
