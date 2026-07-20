using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.FluentUI.AspNetCore.Components;
using SessionPlanner.Web;
using SessionPlanner.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAuthorizationCore();
builder.Services.AddFluentUIComponents();

builder.Services.AddScoped(sp =>
{
    return new HttpClient
    {
        BaseAddress = ResolveApiBaseUri(builder.Configuration["ApiBaseUrl"], builder.HostEnvironment)
    };
});

builder.Services.AddScoped<SessionStorageService>();
builder.Services.AddScoped<AuthSessionService>();
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());

await builder.Build().RunAsync();

static Uri ResolveApiBaseUri(string? configuredBaseUrl, IWebAssemblyHostEnvironment environment)
{
    var appBaseUri = new Uri(environment.BaseAddress);

    if (string.IsNullOrWhiteSpace(configuredBaseUrl))
    {
        return new Uri(appBaseUri, "api/v1/");
    }

    var normalized = configuredBaseUrl.Trim().TrimEnd('/') + "/";
    if (Uri.TryCreate(normalized, UriKind.Absolute, out var absoluteUri))
    {
        return absoluteUri;
    }

    return new Uri(appBaseUri, normalized);
}
