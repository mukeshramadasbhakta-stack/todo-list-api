using System.Net;
using System.Text.Json;

namespace ToDoListApi.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
  public async Task InvokeAsync(HttpContext context)
  {
    var correlationId = Guid.NewGuid();

    try
    {
      await next(context);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);
      await WriteProblemDetailsAsync(context, correlationId);
    }
  }

  private static async Task WriteProblemDetailsAsync(HttpContext context, Guid correlationId)
  {
    if (!context.Response.HasStarted)
    {
      context.Response.Clear();
      context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
      context.Response.ContentType = "application/problem+json";

      var problem = new
      {
        type = "https://httpstatuses.com/500",
        title = "An unexpected error occurred.",
        status = 500,
        traceId = context.TraceIdentifier,
        correlationId
      };

      var json = JsonSerializer.Serialize(problem);
      await context.Response.WriteAsync(json);
    }
  }
}
