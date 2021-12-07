var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();

// To run use:
// dapr run --app-id dapr-listener --app-port 5076 --dapr-http-port 3602 --dapr-grpc-port 60002 --components-path ../components -- dotnet run --urls="http://0.0.0.0:5076"
