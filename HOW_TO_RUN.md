# How to Run CarePortal

This guide explains how to run the CarePortal application with the default SQLite database (for development) or optionally with PostgreSQL.

## Prerequisites

- .NET 8 SDK

## Quick Start (SQLite - Default)

The application is configured to use SQLite by default, which requires no additional database setup.

1. **Restore and Build**

```powershell
dotnet restore
dotnet build CarePortal.sln
```

2. **Run the Application**

```powershell
dotnet run --project CarePortal.Api
```

The API will automatically apply pending migrations when running in Development mode.

### Useful URLs
-- PORT: 5203
- Health check: `https://localhost:<port>/health`
- Swagger UI (Development only): `https://localhost:<port>/swagger`

## Run Tests

Run all tests:

```powershell
dotnet test CarePortal.sln
```

The tests use an in-memory SQLite database for fast, reliable execution.

## Optional: Using PostgreSQL

If you want to use PostgreSQL instead of SQLite, follow these steps:

### Prerequisites for PostgreSQL

- PostgreSQL installed locally
- A PostgreSQL user with permission to create/use databases

### 1. Create Local Databases

Create an application database:

```sql
CREATE DATABASE careportal;
```

Optional test database for PostgreSQL integration tests:

```sql
CREATE DATABASE careportal_tests;
```

### 2. Update Database Provider and Connection Strings

Update `CarePortal.Infrastructure/DependencyInjection.cs` to use PostgreSQL:

```csharp
services.AddDbContext<CarePortalDbContext>(options =>
    options.UseNpgsql(connectionString));
```

Update `CarePortal.Api/appsettings.json` with your PostgreSQL connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=careportal;Username=postgres;Password=postgres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

Update the username/password to match your local PostgreSQL setup.

### 3. Add Npgsql Package (if not already present)

Ensure `Npgsql.EntityFrameworkCore.PostgreSQL` is installed in `CarePortal.Infrastructure.csproj`.

### 4. Run Migrations

Install EF Core CLI if needed:

```powershell
dotnet tool install --global dotnet-ef
```

Apply migrations to the application database:

```powershell
dotnet ef database update --project CarePortal.Infrastructure --startup-project CarePortal.Api
```

### 5. Run the Application

```powershell
dotnet run --project CarePortal.Api
```
