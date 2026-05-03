using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using SessionPlanner.Web;
using SessionPlanner.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredSessionStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AppAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AppAuthStateProvider>());
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddTransient<AuthenticatedHttpHandler>();

// Use ApiBaseAddress from config (e.g. appsettings.Development.json) so that
// local dev can target the API on a different port. Falls back to same-origin
// for production same-host deployments.
var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? builder.HostEnvironment.BaseAddress;

// Plain client used by AuthService — no auth handler to avoid circular DI.
builder.Services.AddHttpClient("AuthClient", client =>
{
    client.BaseAddress = new Uri(apiBaseAddress);
});

// Authenticated client used by ApiClient — adds Bearer token via handler.
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(apiBaseAddress);
})
.AddHttpMessageHandler<AuthenticatedHttpHandler>();

builder.Services.AddScoped<IApiClient, ApiClient>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new ApiClient(factory.CreateClient("API"));
});

builder.Services.AddFluentUIComponents();

await builder.Build().RunAsync();
