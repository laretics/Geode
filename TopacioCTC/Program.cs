using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using TopacioCTC;
using TopacioCTC.Authentication;
using TopacioCTC.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
string mvarServerUri = builder.Configuration["ServerUri"] ??"http://localhost:5000";
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<TopacioLayoutService>(sp =>
    new TopacioLayoutService(sp, sp.GetRequiredService<IJSRuntime>()));
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(mvarServerUri) });
builder.Services.AddScoped<TopacioClient>(); //Cliente para obtener topología y estado del enclavamiento
builder.Services.AddScoped<TopacioAuthService>(sp =>
{    
    return new TopacioAuthService(sp);
});

var host = builder.Build();
var authService = host.Services.GetRequiredService<TopacioAuthService>();
authService.setJSRuntime(host.Services.GetRequiredService<IJSRuntime>());
await host.RunAsync();

