using Microsoft.AspNetCore.Authentication;
using Serilog;
using ToDoListApi.Mappings;
using ToDoListApi.Models;
using ToDoListApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ToDoListApi.Repositories;
using ToDoListApi.Security;

namespace ToDoListApi.Extensions;

public static class ServiceExtensions
{
  public const string CorsPolicy = "TodoCorsPolicy";
  private const string ApiTitle = "TODO API";

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

    // Configure CORS
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

    // Configure swagger
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(c =>
    {      
      c.SwaggerDoc("v1", new OpenApiInfo
      {
        Title = ApiTitle,
        Version = "v1",
        Description = "API for managing todos"
      });

      c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
      c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
      {
        Description = "API Key to access the endpoints.",
        Name = "x-api-key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
      });
      c.AddSecurityRequirement(new OpenApiSecurityRequirement
      {
        {
          new OpenApiSecurityScheme
          {
            Reference = new OpenApiReference
            {
              Type = ReferenceType.SecurityScheme,
              Id = "ApiKey"
            }
          },
          Array.Empty<string>()
        }
      });
    });
    
    // Add the repos and services
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
