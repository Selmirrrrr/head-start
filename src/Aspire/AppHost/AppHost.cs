var builder = DistributedApplication.CreateBuilder(args);

var keycloak = builder.AddKeycloak("keycloak", 8080)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithRealmImport("./Realms");


var seq = builder.AddSeq("seq")
    .ExcludeFromManifest()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("ACCEPT_EULA", "Y");

var webapi = builder.AddProject<Projects.HeadStart_WebAPI>("webapi")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(seq)
    .WaitFor(seq);

builder.AddProject<Projects.HeadStart_BFF>("bff")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(keycloak)
    .WithReference(webapi)
    .WithReference(seq)
    .WaitFor(seq);

await builder.Build().RunAsync();
