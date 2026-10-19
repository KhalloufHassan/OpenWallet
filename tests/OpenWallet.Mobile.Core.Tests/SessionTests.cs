using System.Net;
using OpenWallet.Mobile.Core.Auth;
using OpenWallet.Shared.Constants;
using OpenWallet.Shared.DTOs;
using OpenWallet.UI.Services;

namespace OpenWallet.Mobile.Core.Tests;

public sealed class SessionTests
{
    [Theory]
    [InlineData("wallet.example.org", "https://wallet.example.org/")]
    [InlineData(" https://wallet.example.org ", "https://wallet.example.org/")]
    [InlineData("https://example.org/openwallet", "https://example.org/openwallet/")]
    [InlineData("https://wallet.example.org:8443/?x=1#y", "https://wallet.example.org:8443/")]
    public void ServerAddresses_AreNormalized(string input, string expected) =>
        Assert.Equal(expected, ServerAddress.Normalize(input));

    [Theory]
    [InlineData("http://wallet.example.org")]
    [InlineData("ftp://wallet.example.org")]
    [InlineData("")]
    public void ServerAddresses_MustBeHttps(string input) =>
        Assert.Throws<AppSignInException>(() => ServerAddress.Normalize(input));

    [Theory]
    [InlineData("1.0.0", "1.2.0", true)]
    [InlineData("1.2.0", "1.2.0", false)]
    [InlineData("1.3.0", "1.2.0", false)]
    [InlineData("1.0.0", null, false)]
    public void AppVersions_AreCompared(string current, string? minimum, bool older) =>
        Assert.Equal(older, SessionManager.IsOlder(current, minimum));

    [Fact]
    public async Task SettingTheServer_RejectsServersThatNeedANewerApp()
    {
        FakeServer server = new()
        {
            Respond = (_, _) => FakeServer.Json(new ServerInfoDto { Name = MobileApp.ServerName, Version = "2.0.0", ApiVersion = MobileApp.ApiVersion, MinimumAppVersion = "2.0.0" })
        };
        SessionManager session = NewSession(server, new MemorySessionStorage(), new FakeTime(DateTimeOffset.UtcNow));

        AppSignInException error = await Assert.ThrowsAsync<AppSignInException>(() => session.SetServerAsync("wallet.test"));

        Assert.Contains("newer version of the app", error.Message);
    }

    [Fact]
    public async Task SettingTheServer_RejectsOtherServers()
    {
        FakeServer server = new() { Respond = (_, _) => new HttpResponseMessage(HttpStatusCode.NotFound) };
        SessionManager session = NewSession(server, new MemorySessionStorage(), new FakeTime(DateTimeOffset.UtcNow));

        await Assert.ThrowsAsync<AppSignInException>(() => session.SetServerAsync("wallet.test"));
        Assert.Null(session.ServerAddress);
    }

    [Fact]
    public async Task SigningIn_KeepsTheTokensAndTheUser()
    {
        FakeServer server = new()
        {
            Respond = (request, _) => request.RequestUri!.AbsolutePath switch
            {
                "/api/auth/app/login" => FakeServer.Json(new AppTokensDto { AccessToken = "access-1", ExpiresIn = 1800, RefreshToken = "refresh-1" }),
                "/api/auth/me" => FakeServer.Json(new AuthMeDto { Username = "owner" }),
                _ => new HttpResponseMessage(HttpStatusCode.NotFound)
            }
        };
        MemorySessionStorage storage = new() { Address = FakeServer.Address };
        SessionManager session = NewSession(server, storage, new FakeTime(DateTimeOffset.UtcNow));
        await session.InitializeAsync();

        LoginResultDto result = await session.SignInAsync(new AppLoginDto { Username = "owner", Password = "secret" });

        Assert.True(result.Succeeded);
        Assert.Equal("access-1", storage.Session?.AccessToken);
        Assert.Equal("owner", session.Session?.Username);
        Assert.Equal("Bearer access-1", server.Requests.Single(r => r.Path == "api/auth/me").Authorization);
    }

    [Fact]
    public async Task SigningIn_ReportsThatACodeIsNeeded()
    {
        FakeServer server = new()
        {
            Respond = (_, _) => FakeServer.Json(new LoginResultDto { RequiresTwoFactor = true }, HttpStatusCode.Unauthorized)
        };
        MemorySessionStorage storage = new() { Address = FakeServer.Address };
        SessionManager session = NewSession(server, storage, new FakeTime(DateTimeOffset.UtcNow));
        await session.InitializeAsync();

        LoginResultDto result = await session.SignInAsync(new AppLoginDto { Username = "owner", Password = "secret" });

        Assert.False(result.Succeeded);
        Assert.True(result.RequiresTwoFactor);
        Assert.False(session.IsSignedIn);
    }

    [Fact]
    public async Task ExpiringTokens_AreRefreshedOnce_ForManyRequestsAtOnce()
    {
        FakeTime time = new(DateTimeOffset.UtcNow);
        int refreshes = 0;
        FakeServer server = new()
        {
            Respond = (request, _) =>
            {
                if (request.RequestUri!.AbsolutePath == "/api/auth/app/refresh")
                {
                    Interlocked.Increment(ref refreshes);
                    Thread.Sleep(50);
                    return FakeServer.Json(new AppTokensDto { AccessToken = "access-2", ExpiresIn = 1800, RefreshToken = "refresh-2" });
                }

                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
        };
        MemorySessionStorage storage = new() { Address = FakeServer.Address, Session = new AppSession("access-1", "refresh-1", time.Now.AddSeconds(30), "owner") };
        SessionManager session = NewSession(server, storage, time);
        await session.InitializeAsync();

        string?[] tokens = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(() => session.GetAccessTokenAsync())));

        Assert.All(tokens, token => Assert.Equal("access-2", token));
        Assert.Equal(1, refreshes);
        Assert.Equal("refresh-2", storage.Session?.RefreshToken);
    }

    [Fact]
    public async Task ARejectedRefresh_SignsOut()
    {
        FakeTime time = new(DateTimeOffset.UtcNow);
        FakeServer server = new() { Respond = (_, _) => new HttpResponseMessage(HttpStatusCode.Unauthorized) };
        MemorySessionStorage storage = new() { Address = FakeServer.Address, Session = new AppSession("a", "r", time.Now.AddSeconds(-1), "owner") };
        SessionManager session = NewSession(server, storage, time);
        await session.InitializeAsync();
        bool signedOut = false;
        session.Changed += () => signedOut = !session.IsSignedIn;

        Assert.Null(await session.GetAccessTokenAsync());
        Assert.True(signedOut);
        Assert.Null(storage.Session);
    }

    [Fact]
    public async Task Unreachable_AnExpiredTokenIsKept_SoTheAppStaysSignedIn()
    {
        FakeTime time = new(DateTimeOffset.UtcNow);
        FakeServer server = new() { Unreachable = true };
        MemorySessionStorage storage = new() { Address = FakeServer.Address, Session = new AppSession("a", "r", time.Now.AddSeconds(-1), "owner") };
        SessionManager session = NewSession(server, storage, time);
        await session.InitializeAsync();

        Assert.Equal("a", await session.GetAccessTokenAsync());
        Assert.True(session.IsSignedIn);
    }

    [Fact]
    public async Task A401_RefreshesAndRetriesTheRequestOnce()
    {
        FakeTime time = new(DateTimeOffset.UtcNow);
        FakeServer server = new()
        {
            Respond = (request, body) => request.RequestUri!.AbsolutePath == "/api/auth/app/refresh"
                ? FakeServer.Json(new AppTokensDto { AccessToken = "fresh", ExpiresIn = 1800, RefreshToken = "r2" })
                : request.Headers.Authorization?.Parameter == "fresh"
                    ? FakeServer.Json(new { echoed = body })
                    : new HttpResponseMessage(HttpStatusCode.Unauthorized)
        };
        MemorySessionStorage storage = new() { Address = FakeServer.Address, Session = new AppSession("revoked", "r", time.Now.AddHours(1), "owner") };
        SessionManager session = NewSession(server, storage, time);
        await session.InitializeAsync();
        using HttpClient http = server.Client(new BearerTokenHandler(session) { InnerHandler = server });

        using HttpResponseMessage response = await http.PostAsync("api/tags", new StringContent("{\"name\":\"food\"}", System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("food", await response.Content.ReadAsStringAsync());
        Assert.Equal(2, server.Requests.Count(r => r.Path == "api/tags"));
    }

    [Fact]
    public void Pkce_ChallengeIsTheHashOfTheVerifier()
    {
        (string verifier, string challenge) = Pkce.Create();

        Assert.Equal(43, verifier.Length);
        Assert.True(MobileApp.IsValidChallenge(challenge));
        Assert.Equal(challenge, Pkce.ChallengeFor(verifier));
    }

    static SessionManager NewSession(FakeServer server, MemorySessionStorage storage, FakeTime time) =>
        new(storage, address => new HttpClient(server, disposeHandler: false) { BaseAddress = new Uri(address) }, time, "1.0.0");
}

public sealed class ServerAddressHandlerTests
{
    [Theory]
    [InlineData("https://wallet.test/", "https://wallet.test/api/records?page=2")]
    [InlineData("https://example.test/openwallet/", "https://example.test/openwallet/api/records?page=2")]
    public async Task Requests_GoToTheChosenServer(string address, string expected)
    {
        FakeServer server = new();
        MemorySessionStorage storage = new() { Address = address };
        SessionManager session = new(storage, _ => new HttpClient(server), TimeProvider.System, "1.0.0");
        await session.InitializeAsync();
        string? sent = null;
        server.Respond = (request, _) =>
        {
            sent = request.RequestUri!.ToString();
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        };
        using HttpClient http = new(new ServerAddressHandler(session) { InnerHandler = server })
        {
            BaseAddress = ServerAddressHandler.Placeholder
        };

        await http.GetAsync("api/records?page=2");

        Assert.Equal(expected, sent);
    }
}
