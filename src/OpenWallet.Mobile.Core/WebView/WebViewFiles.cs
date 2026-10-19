using System.Text.RegularExpressions;

namespace OpenWallet.Mobile.Core.WebView;

/// <summary>
/// Answers the web view's own API requests: attachment images (<c>&lt;img src="api/…"&gt;</c>). Those go
/// to the app's origin, with neither the server's address nor the app's token, so the app fetches them
/// from the server with the token.
/// </summary>
/// <param name="server">An HttpClient straight to the server, with the token.</param>
public sealed partial class WebViewFiles(Func<HttpClient> server)
{
    /// <summary>Whether the web view's request is one for the app to answer.</summary>
    public static bool Handles(string method, Uri uri) =>
        method.Equals("GET", StringComparison.OrdinalIgnoreCase) && AttachmentPath().IsMatch(uri.AbsolutePath);

    /// <summary>
    /// Fetches the file from the server. The web view is answered before this finishes, so failures
    /// (offline, not found) come back as an empty body, which the page shows as a broken image.
    /// </summary>
    public async Task<Stream> FetchAsync(Uri uri, CancellationToken cancellationToken)
    {
        HttpClient http = server();
        try
        {
            HttpResponseMessage response = await http.GetAsync(uri.PathAndQuery.TrimStart('/'), HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                response.Dispose();
                http.Dispose();
                return Stream.Null;
            }

            Stream body = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            return new OwningStream(body, response, http);
        }
        catch (HttpRequestException)
        {
            http.Dispose();
            return Stream.Null;
        }
    }

    [GeneratedRegex(@"^/api/records/\d+/attachments/\d+$")]
    private static partial Regex AttachmentPath();

    /// <summary>The response body, which also disposes the response and client once read.</summary>
    sealed class OwningStream(Stream inner, IDisposable response, IDisposable client) : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            inner.ReadAsync(buffer, offset, count, cancellationToken);

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            inner.ReadAsync(buffer, cancellationToken);

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                inner.Dispose();
                response.Dispose();
                client.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
