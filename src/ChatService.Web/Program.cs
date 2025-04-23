var builder = WebApplication.CreateBuilder(args);

builder.AddWebService();
builder.AddInfrastructureService();
builder.AddApplicationService();
builder.AddPersistenceServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.ConfigureSwagger();
// }
app.ConfigureMiddleware();

app.Run();