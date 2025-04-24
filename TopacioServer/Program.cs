using TopacioServer.Components;
using TopacioServer.Layout;
using MontefaroMatias;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    options.SerializerOptions.TypeInfoResolverChain.Insert(1, SharedSerializeContext.Default);
});

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

var app = builder.Build();

app.UseCors("AllowBlazorClient");
Kernel mvarKernel = new Kernel();
LayoutController auxLayout = new LayoutController(app, mvarKernel);

app.Run();
