var builder = DistributedApplication.CreateBuilder(args);

var keycloak = builder.AddKeycloak("keycloak", 8080)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithRealmImport("./Realms");

var webapi = builder.AddProject<Projects.HeadStart_WebAPI>("webapi")
    .WithExternalHttpEndpoints()
    .WithReference(keycloak)
    .WithHttpHealthCheck("/health")
    .WaitFor(keycloak);

builder.AddProject<Projects.HeadStart_BFF>("bff")
    .WithExternalHttpEndpoints()
    .WithReference(keycloak)
    .WithReference(webapi)
    .WithHttpHealthCheck("/health")
    .WaitFor(webapi);

builder.Build().Run();
