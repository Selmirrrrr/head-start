var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
                      .WithPgWeb();

var postgresdb = postgres.AddDatabase("postgresdb");

var keycloak = builder.AddKeycloak("keycloak", 8080)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithRealmImport("./Realms");

var webapi = builder.AddProject<Projects.HeadStart_WebAPI>("webapi")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(postgresdb)
    .WaitFor(postgresdb);

builder.AddProject<Projects.HeadStart_BFF>("bff")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(webapi)
    .WaitFor(webapi);

await builder.Build().RunAsync();
