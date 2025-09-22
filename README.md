# ToDoList API

A simple, ASP.NET Core Web API for managing Todo items. It demonstrates lose coupling, clean layering, mapping, testing, logging, and API documentation.

## Features

- .NET 9, ASP.NET Core, C# 13
- RESTful endpoints
- API key authentication
- Global exception handling
- Serilog logging (console + rolling file)
- OpenAPI/Swagger UI
- In-memory EF Core in tests
- Unit and integration tests

## Tech Stack

- ASP.NET Core (Web API, MVC)
- Entity Framework Core (for data access)
- AutoMapper (DTO - Model mapping)
- Serilog (logging)
- Swashbuckle/OpenAPI (docs)
- xUnit, Moq, FluentAssertions, AutoFixture (tests)

## Solution Structure

- ToDoListApi.sln
  - ToDoListApi (main API project)
  - ToDoListApi.Tests (unit tests)
  - ToDoListApi.IntegrationTests (end-to-end tests)

Typical folders in the API project:
- Controllers: HTTP endpoints
- Services: Business logic (interfaces + implementations)
- Repositories: Data access abstractions/implementations
- Models: Domain entities
- Dtos: API contracts
- Mappings: AutoMapper profiles
- Middleware: Cross-cutting concerns (e.g., exception handling)
- Extensions/Configuration: DI setup, CORS, etc.

## Getting Started

### Setup

- Restore packages:
  - dotnet restore
- Build:
  - dotnet build
- Run API:
  - dotnet run --project ToDoListApi

By default, the API runs with Environment=Development if not set.

### Configuration

- appsettings.json and appsettings.{Environment}.json control logging and other settings.
- Environment variable: ASPNETCORE_ENVIRONMENT (Development/Staging/Production).
- Log file path can be configured with LogFileName in configuration (defaults to ./logs/Todo-Api.log).

### Authentication

- API key authentication is required.
- Provide the API key via an HTTP header (e.g., x-api-key). Ensure your client includes this header for all protected endpoints.
- When using Swagger UI, click “Authorize” and enter the API key.
- When using Postman, add the API key to the Auth -
<img width="536" height="421" alt="image" src="https://github.com/user-attachments/assets/c0b6742a-4808-40c8-9a80-cafaa86f7410" />


**NOTE** - You can find the API key in the appsettings.Development.json.

## API

Base URL: https://localhost:{port}/

Controller: /Todo

Endpoints:
- GET /Todo/GetAll — list all todos
- GET /Todo?id={guid} — get a todo by id
- POST /Todo — create/update a todo (Upsert)
- DELETE /Todo?id={guid} — delete a todo

Notes:
- On server errors, responses are application/problem+json with a correlationId.
- Validation and argument errors return 400 with details.

## OpenAPI/Swagger

- In Development:
  - Swagger JSON: /swagger/v1/swagger.json
  - Swagger UI: /swagger
- The UI supports API key authorization for testing.

## Logging

- Serilog writes to console and rolling file:
  - Default file: ./logs/Todo-Api.log (daily rolling)
- Message template includes timestamps and exceptions.

## Testing

- Run all tests:
  - `dotnet test`
- Unit tests cover mappings and service logic.
- Integration tests exercise repository and service end-to-end using EF Core InMemory.

## Common Commands

- Restore: `dotnet restore`
- Build: `dotnet build`
- Run: `dotnet run --project ToDoListApi`
- Test: `dotnet test`
- Publish: `dotnet publish ToDoListApi -c Release -o ./publish`


## Developer Notes

- To keep DTOs stable, add mappings in AutoMapper profiles.
- Use the middleware for consistent/structured error handling.
- Logging can be further enhanced with tools like Seq, Azure Application Insights or AWS CloudWatch.
- Though Automapper has been used in this use-case, I prefer writing my own mapping code for performance.
- AutoFixtures can be enhanced with Fake/Bogus packages.
- I would consider enhancing the code-base with the MediatR pattern.
- For more complex and performant operations, I would consider using gRPC, TPL supported by CancellationTokens.
- SAST (like SonarQube) and DAST (GitLab) tools could be integrated in the CI/CD pipelines.


