# CarePortal

Production-ready starter solution for a .NET 8 Care Portal API built with ASP.NET Core, Clean Architecture, EF Core Code First, PostgreSQL, and xUnit.

## Solution structure

```text
CarePortal.Api             ASP.NET Core HTTP API and composition root
CarePortal.Application     Use cases, DTOs, service abstractions
CarePortal.Domain          Entities, domain rules, domain events
CarePortal.Infrastructure  EF Core, PostgreSQL, repositories, persistence
CarePortal.Tests           xUnit tests for domain/application/infrastructure
```

## Prerequisites

- .NET 8 SDK
- PostgreSQL 14+
- Optional: EF Core CLI tools

```powershell
dotnet tool install --global dotnet-ef
```

## Configure PostgreSQL

Update `CarePortal.Api/appsettings.json` or use user secrets/environment variables.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=careportal;Username=postgres;Password=postgres"
  }
}
```

Environment variable alternative:

```powershell
$env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=careportal;Username=postgres;Password=postgres"
```

## Restore, build, and test

```powershell
dotnet restore
dotnet build
dotnet test
```

## Create and apply migrations

Migrations belong to the Infrastructure project while the API acts as the startup project.

```powershell
dotnet ef migrations add InitialCreate --project CarePortal.Infrastructure --startup-project CarePortal.Api --output-dir Persistence/Migrations
dotnet ef database update --project CarePortal.Infrastructure --startup-project CarePortal.Api
```

In development, the API also runs `Database.MigrateAsync()` on startup so pending migrations are applied automatically.

## Run the API

```powershell
dotnet run --project CarePortal.Api
```

Useful endpoints:

- `GET /health`
- `GET /api/patients`
- `GET /api/patients/{id}`
- `POST /api/patients`

Example request:

```json
{
  "firstName": "Ada",
  "lastName": "Lovelace",
  "email": "ada@example.com",
  "dateOfBirth": "1815-12-10"
}
```

## Architecture notes

- `Domain` has no dependency on other projects.
- `Application` depends only on `Domain` and exposes persistence abstractions.
- `Infrastructure` implements persistence with EF Core and PostgreSQL.
- `Api` wires dependency injection and exposes controllers.
- Tests use EF Core InMemory for fast persistence checks and xUnit for unit tests.
