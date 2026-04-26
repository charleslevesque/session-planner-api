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

builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
})
.AddHttpMessageHandler<AuthenticatedHttpHandler>();

builder.Services.AddScoped<IApiClient, ApiClient>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new ApiClient(factory.CreateClient("API"));
});

builder.Services.AddFluentUIComponents();

await builder.Build().RunAsync();
