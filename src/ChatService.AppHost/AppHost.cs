var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ChatService_Web>("chatservice-web");

builder.Build().Run();
