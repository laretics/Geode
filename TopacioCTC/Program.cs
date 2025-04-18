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

builder.Services.AddSingleton<StorageService>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(mvarServerUri) });
builder.Services.AddScoped<TopacioClient>(); //Cliente para obtener topología y estado del enclavamiento
builder.Services.AddSingleton<StorageService>();
builder.Services.AddScoped<TopacioAuthService>(
sp =>
{
    var jsRuntime = sp.GetRequiredService<IJSRuntime>();
    return new TopacioAuthService(jsRuntime);
});

var host = builder.Build();

// Inicializa el servicio aquí si es necesario
var authService = host.Services.GetRequiredService<TopacioAuthService>();
// Realiza cualquier inicialización adicional aquí

await host.RunAsync();

