var builder = DistributedApplication.CreateBuilder(args);

var kafka = builder.AddConnectionString("kafka");
builder.AddProject<Projects.ChatService_Web>("chatservice-web")
    .WithReference(kafka);

builder.Build().Run();
