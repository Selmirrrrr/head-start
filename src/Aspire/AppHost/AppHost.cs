var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgWeb();

var mailpit = builder.AddMailPit("mailpit", 8025, 1025)
    .WithLifetime(ContainerLifetime.Persistent);

var postgresdb = postgres.AddDatabase("postgresdb");

var keycloak = builder.AddKeycloak("keycloak", 8080)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithRealmImport("./Realms")
    .WithReference(mailpit);

var webapi = builder.AddProject<Projects.HeadStart_WebAPI>("webapi")
    .WithHttpHealthCheck("/health")
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(postgresdb)
    .WaitFor(postgresdb)
    .WithReference(mailpit);

builder.AddProject<Projects.HeadStart_BFF>("bff")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(webapi)
    .WaitFor(webapi);

builder.AddDockerComposeEnvironment("Claimly");

await builder.Build().RunAsync();
