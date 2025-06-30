using ChatService.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddWebService();
builder.AddApplicationService();
builder.AddInfrastructureService();
builder.AddPersistenceServices();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.ConfigureSwagger();
// }
app.ConfigureMiddleware();

app.Run();