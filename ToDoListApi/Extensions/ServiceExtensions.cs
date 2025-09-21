using Microsoft.AspNetCore.Authentication;
using Serilog;
using ToDoListApi.Mappings;
using ToDoListApi.Models;
using ToDoListApi.Services;
using Microsoft.EntityFrameworkCore;
using ToDoListApi.Repositories;
using ToDoListApi.Security;

namespace ToDoListApi.Extensions;

public static class ServiceExtensions
{
  public const string CorsPolicy = "TodoCorsPolicy";

  public static void AddServices(this IServiceCollection services)
  {
    services.AddScoped<ITodoService, TodoService>()
      .AddLogging(a => a.AddSerilog())
      .AddSwaggerGen()
      .AddAutoMapper();

    services.AddLogging(a => a.AddSerilog());
    // Register EF Core InMemory and repository
    services.AddDbContext<AppDbContext>(options =>
      options.UseInMemoryDatabase("TodoDb"));

    // Register API Key authentication using configuration value "ApiKey"
    services
      .AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
      .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationHandler.SchemeName, _ => { });

    // Add CORS
    services.AddCors(options =>
    {
      options.AddPolicy(name: CorsPolicy,
        bldr =>
        {
          bldr.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
    });

    services.AddScoped<ITodoRepository, TodoRepository>();
  }

  private static IServiceCollection AddAutoMapper(this IServiceCollection serviceCollection)
  {
    return serviceCollection.AddAutoMapper(cfg =>
    {
      cfg.AddProfiles([
        new TodoMapper()
      ]);
    });
  }
}
