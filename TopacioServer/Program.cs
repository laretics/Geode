using System.Text.Json.Serialization;
using TopacioServer.Layout;

const string ORIGINS_URI = "http://localhost:5154";

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    options.SerializerOptions.TypeInfoResolverChain.Insert(1, TopologySerializerContext.Default);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
    builder =>
    {
        builder.WithOrigins(ORIGINS_URI) // Cambia esto al puerto de tu aplicación Blazor
     .AllowAnyHeader()
     .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowBlazorClient");
TodoSample auxTodo = new TodoSample(app);
LayoutController auxLayout = new LayoutController(app);

app.Run();
