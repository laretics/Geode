var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.TopacioAspire_ApiService>("apiservice");

builder.AddProject<Projects.TopacioAspire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
