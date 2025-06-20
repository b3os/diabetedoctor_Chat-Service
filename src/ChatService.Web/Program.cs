var builder = WebApplication.CreateBuilder(args);

builder.AddWebService();
builder.AddApplicationService();
builder.AddInfrastructureService();
builder.AddPersistenceServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.ConfigureSwagger();
// }
app.ConfigureMiddleware();

app.Run();