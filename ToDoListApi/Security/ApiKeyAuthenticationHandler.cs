using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace ToDoListApi.Security;

public sealed class ApiKeyAuthenticationHandler(
  IOptionsMonitor<AuthenticationSchemeOptions> options,
  ILoggerFactory logger,
  UrlEncoder encoder,
  IConfiguration configuration
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
  public const string SchemeName = "ApiKey";
  private const string HeaderName = "x-api-key";

  protected override Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    var expectedKey = configuration["Api-Key"];
    if (string.IsNullOrWhiteSpace(expectedKey))
    {
      return Task.FromResult(AuthenticateResult.Fail("API Key not configured"));
    }

    if (!Request.Headers.TryGetValue(HeaderName, out var provided)
        || string.IsNullOrWhiteSpace(provided))
    {
      return Task.FromResult(AuthenticateResult.Fail("API Key header missing"));
    }

    if (!string.Equals(provided.ToString(), expectedKey, StringComparison.Ordinal))
    {
      return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
    }

    var claims = new[]
    {
      new Claim(ClaimTypes.NameIdentifier, "api-key-user"),
      new Claim(ClaimTypes.Name, "ApiKeyUser")
    };

    var identity = new ClaimsIdentity(claims, SchemeName);
    var principal = new ClaimsPrincipal(identity);
    var ticket = new AuthenticationTicket(principal, SchemeName);

    return Task.FromResult(AuthenticateResult.Success(ticket));
  }

  protected override Task HandleChallengeAsync(AuthenticationProperties properties)
  {
    Response.StatusCode = StatusCodes.Status401Unauthorized;
    Response.Headers["WWW-Authenticate"] = $"{SchemeName} realm=\"api\"";
    return base.HandleChallengeAsync(properties);
  }
}
