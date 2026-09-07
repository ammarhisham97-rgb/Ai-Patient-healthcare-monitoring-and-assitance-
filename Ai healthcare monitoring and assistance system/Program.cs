using Ai_healthcare_monitoring_and_assistance_system.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

app.UseApplicationMiddleware();
app.MapApplicationEndpoints(builder.Configuration);

app.Run();
