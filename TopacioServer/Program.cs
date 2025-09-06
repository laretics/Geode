using TopacioServer.Components;
using TopacioServer.Layout;
using MontefaroMatias;

Microsoft.AspNetCore.Builder.WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

//Serialización JSON.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    options.SerializerOptions.TypeInfoResolverChain.Insert(1, SharedSerializeContext.Default);
});

//Acceso desde Blazor WASM
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
    builder =>
    {
        builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
    });
});

// Inyectamos el Kernel como singleton
builder.Services.AddSingleton<Kernel>();

// Añadimos los controladores REST (nueva sintaxis)
builder.Services.AddControllers();

Microsoft.AspNetCore.Builder.WebApplication app = builder.Build();
app.UseCors("AllowBlazorClient");

//Mapeamos los controladores REST
app.MapControllers();

//Iniciamos el servidor.
app.Run();
