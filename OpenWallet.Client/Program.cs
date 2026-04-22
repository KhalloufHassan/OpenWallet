using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OpenWallet.Client.Services;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<ApiClient>();
builder.Services.AddSingleton<PrivacyService>();
builder.Services.AddSingleton<LockService>();

await builder.Build().RunAsync();
