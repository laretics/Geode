using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using TopacioCTC;
using TopacioCTC.Authentication;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<AuthService>(
sp =>
{
    var jsRuntime = sp.GetRequiredService<IJSRuntime>();
    return new AuthService(jsRuntime);
});



var host = builder.Build();

// Inicializa el servicio aquí si es necesario
var authService = host.Services.GetRequiredService<AuthService>();
// Realiza cualquier inicialización adicional aquí

await host.RunAsync();

