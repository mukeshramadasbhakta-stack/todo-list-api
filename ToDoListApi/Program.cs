using Serilog;
using ToDoListApi.Extensions;
using ToDoListApi.Middleware;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (string.IsNullOrWhiteSpace(environment))
{
  environment = "Development"; // default
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var config = builder.Configuration
  .AddJsonFile("./appsettings.json")
  .AddJsonFile($"appsettings.{environment}.json", true, true)
  .AddEnvironmentVariables()
  .Build();

var logFileName = config["LogFileName"] ?? "./logs/Todo-Api.log";
Log.Logger = new LoggerConfiguration()
  .Enrich.FromLogContext()
  .WriteTo.File(logFileName,
    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
    rollingInterval: RollingInterval.Day)
  .WriteTo.Console()
  .CreateLogger();

builder.Services.AddServices();

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors(ServiceExtensions.CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
