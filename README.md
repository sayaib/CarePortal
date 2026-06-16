# CarePortal Billing Assignment

CarePortal is a .NET 8 Clean Architecture solution that demonstrates invoice billing, append-only ledger accounting, PostgreSQL persistence, EF Core Code First migrations, and xUnit test coverage.

## Solution Structure

```text
CarePortal.Api             ASP.NET Core API and composition root
CarePortal.Application     Use-case contracts and application abstractions
CarePortal.Domain          Billing entities and business rules
CarePortal.Infrastructure  EF Core, PostgreSQL, repositories, billing allocator
CarePortal.Tests           Unit, persistence, and integration tests
```

## Architecture Decisions

- Clean Architecture keeps business rules independent from infrastructure concerns.
- `Domain` contains entities such as `Invoice`, `InvoiceLineItem`, and `LedgerEntry`.
- `Application` defines contracts, including `IBillingPaymentAllocator`.
- `Infrastructure` implements persistence and allocation using EF Core and PostgreSQL.
- EF Core Fluent API configuration is kept outside domain entities.
- Money is represented with `decimal` and configured as `numeric(18,2)`.
- Ledger entries are append-only. Existing ledger rows are never updated or deleted.
- PostgreSQL is the production database provider.

## Ledger Design

The ledger records financial activity as immutable entries:

- `PaymentReceived`: records that a payment was received for an invoice.
- `Allocation`: applies part of a payment to a specific invoice line item.
- `Credit`: records unapplied overpayment at invoice level.

`LedgerEntry.LineItemId` is nullable so credits and payment receipts can exist at invoice level. Allocations always reference a line item.

The outstanding balance is calculated from ledger history:

```text
Outstanding Balance =
Total Line Item Amounts
- Total Allocations
- Total Credits
```

A credit can therefore produce a negative balance.

## Allocation Algorithm

`AllocatePayment(Guid invoiceId, decimal paymentAmount, DateTime receivedAt)`:

1. Starts a PostgreSQL-safe serializable transaction.
2. Inserts a `PaymentReceived` ledger entry.
3. Loads invoice line items ordered by:
   - `DueDate` ascending
   - `CreatedAt` ascending
4. Calculates outstanding per line item:

```text
line item amount - existing allocation entries for that line item
```

5. Allocates payment oldest-first.
6. Fully satisfies each line item before moving to the next.
7. Inserts `Allocation` ledger entries.
8. If payment exceeds outstanding amount, inserts a `Credit` ledger entry with `LineItemId = null`.
9. Returns the recalculated outstanding balance.

No existing ledger entry is modified.

## Concurrency Handling

The allocator uses PostgreSQL `SERIALIZABLE` isolation and EF Core/Npgsql retry execution strategy.

This approach is preferred over manual `SELECT FOR UPDATE` locking for this assignment because allocation depends on aggregate reads over append-only ledger rows. PostgreSQL serializable transactions detect conflicting concurrent reads/writes, including phantom-style conflicts, and abort one transaction when needed. Npgsql retry strategy then retries the entire transaction safely.

The implementation also clears EF Core tracked state before retrying, preventing stale tracked entities from leaking across retry attempts.

## Configure PostgreSQL

Update `CarePortal.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=careportal;Username=postgres;Password=postgres"
  }
}
```

Or use an environment variable:

```powershell
$env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=careportal;Username=postgres;Password=postgres"
```

## Running Migrations

Install EF Core CLI if needed:

```powershell
dotnet tool install --global dotnet-ef
```

Create a migration:

```powershell
dotnet ef migrations add InitialCreate --project CarePortal.Infrastructure --startup-project CarePortal.Api --output-dir Persistence/Migrations
```

Apply migrations:

```powershell
dotnet ef database update --project CarePortal.Infrastructure --startup-project CarePortal.Api
```

The API also applies pending migrations automatically in development.

## Running the Application

```powershell
dotnet restore
dotnet build
dotnet run --project CarePortal.Api
```

Useful endpoints:

- `GET /health`
- `GET /api/patients`
- `GET /api/patients/{id}`
- `POST /api/patients`

## Running Tests

Run all tests:

```powershell
dotnet test CarePortal.sln
```

The test suite includes:

- Domain validation tests
- EF Core persistence tests
- Allocation scenario tests using SQLite in-memory relational database
- PostgreSQL Testcontainers integration tests

PostgreSQL Testcontainers tests require Docker. If Docker is not available, those tests are skipped automatically.

## Assignment Scenarios Covered

The allocation tests cover:

- Partial payment:
  - Invoice line items: `100`, `150.50`, `200.25`
  - Payment: `120`
  - Allocations: `100`, `20`
  - Balance: `330.75`

- Exact payment:
  - Payment: `450.75`
  - Balance: `0`

- Overpayment:
  - Payment: `500`
  - Credit: `49.25`
  - Balance: `-49.25`

## Assumptions

- Payment amounts must be greater than zero.
- Line item amounts must be greater than zero.
- Invoice references are unique.
- Credits are stored as positive ledger amounts and subtracted in the balance formula.
- `PaymentReceived` entries are audit records and are not included directly in balance calculation.
- `Allocation` entries are the source of truth for applied payments.
- `CreatedAt` timestamps are treated as UTC.
- PostgreSQL is the production provider; SQLite is used only for fast relational tests when PostgreSQL containers are unavailable.
- Docker must be installed and running to execute PostgreSQL Testcontainers tests.
