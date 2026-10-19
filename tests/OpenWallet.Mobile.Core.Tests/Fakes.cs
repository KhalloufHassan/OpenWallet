using System.Net;
using System.Net.Http.Json;
using OpenWallet.Mobile.Core.Auth;

namespace OpenWallet.Mobile.Core.Tests;

/// <summary>A server that answers with whatever the test says, and records what it was sent.</summary>
internal sealed class FakeServer : HttpMessageHandler
{
    public const string Address = "https://wallet.test/";

    public List<(HttpMethod Method, string Path, string? Body, string? Authorization)> Requests { get; } = [];

    public Func<HttpRequestMessage, string?, HttpResponseMessage> Respond { get; set; } = (_, _) => new HttpResponseMessage(HttpStatusCode.NoContent);

    public bool Unreachable { get; set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (Unreachable)
            throw new HttpRequestException("No route to host.");

        string? body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        lock (Requests)
            Requests.Add((request.Method, request.RequestUri!.PathAndQuery.TrimStart('/'), body, request.Headers.Authorization?.ToString()));

        HttpResponseMessage response = Respond(request, body);
        response.RequestMessage = request;
        return response;
    }

    public HttpClient Client(HttpMessageHandler? first = null) => new(first ?? this, disposeHandler: false) { BaseAddress = new Uri(Address) };

    public static HttpResponseMessage Json<T>(T value, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = JsonContent.Create(value) };
}

internal sealed class MemorySessionStorage : ISessionStorage
{
    public string? Address { get; set; }

    public AppSession? Session { get; set; }

    public Task<string?> GetServerAddressAsync() => Task.FromResult(Address);

    public Task SetServerAddressAsync(string address)
    {
        Address = address;
        return Task.CompletedTask;
    }

    public Task<AppSession?> LoadSessionAsync() => Task.FromResult(Session);

    public Task SaveSessionAsync(AppSession session)
    {
        Session = session;
        return Task.CompletedTask;
    }

    public Task ClearSessionAsync()
    {
        Session = null;
        return Task.CompletedTask;
    }
}

internal sealed class FakeTime(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now;
}
