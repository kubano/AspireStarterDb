var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithRedisCommander();

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();

var todosDb = postgres.AddDatabase("todosdb");

var apiDbService = builder.AddProject<Projects.AspireStarterDb_ApiDbService>("apidbservice")
    .WithReference(todosDb)
    .WaitFor(postgres);

var apiService = builder.AddProject<Projects.AspireStarterDb_ApiService>("apiservice")
    .WithReference(todosDb)
    .WaitFor(apiDbService);

builder.AddProject<Projects.AspireStarterDb_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WithReference(apiService)
    .WaitFor(apiService)
    .WaitFor(cache);

builder.Build().Run();
