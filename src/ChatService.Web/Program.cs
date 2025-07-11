using ChatService.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddWebService();
builder.AddApplicationService();
builder.AddInfrastructureService();
builder.AddPersistenceServices();

var app = builder.Build();

app.MapDefaultEndpoints();

app.ConfigureMiddleware();

app.Run();