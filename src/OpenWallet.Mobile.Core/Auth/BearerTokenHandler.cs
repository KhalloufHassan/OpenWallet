using System.Net;
using System.Net.Http.Headers;

namespace OpenWallet.Mobile.Core.Auth;

/// <summary>
/// Adds the app's access token to API requests. When the server still answers 401 (e.g. the password
/// was changed), it refreshes once and sends the request again.
/// </summary>
public sealed class BearerTokenHandler(SessionManager session) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? token = await session.GetAccessTokenAsync(cancellationToken: cancellationToken);
        if (token == null)
            return await base.SendAsync(request, cancellationToken);

        byte[]? body = request.Content == null ? null : await request.Content.ReadAsByteArrayAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        string? refreshed = await session.GetAccessTokenAsync(force: true, cancellationToken);
        if (refreshed == null || refreshed == token)
            return response;

        response.Dispose();
        HttpRequestMessage retry = Copy(request, body);
        retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshed);
        return await base.SendAsync(retry, cancellationToken);
    }

    static HttpRequestMessage Copy(HttpRequestMessage request, byte[]? body)
    {
        HttpRequestMessage copy = new(request.Method, request.RequestUri) { Version = request.Version };
        foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            copy.Headers.TryAddWithoutValidation(header.Key, header.Value);

        if (body != null && request.Content != null)
        {
            copy.Content = new ByteArrayContent(body);
            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Content.Headers)
                copy.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return copy;
    }
}
