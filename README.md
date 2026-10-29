# Capitec Business Invoicing

A small-business invoicing workspace built with Angular 20 and ASP.NET Core 10. The application manages customer billing contacts, invoices, payment-proof documents, Identity accounts, and simulated overdue reminders. Reminder simulations are audit records only; the application does not send email, SMS, or other external messages.

This is a demo application, not a production-ready financial platform. Authentication and role-based authorization are implemented, but the application is single-workspace and does not provide tenant isolation. Do not expose it to untrusted networks or use it for real customer or financial data without a security and operational review.

## Features

- Create and search customer billing contacts; create, search, filter, and export invoices.
- Track due dates, invoice statuses, totals, overdue days, and customer balances. ZAR is the only supported currency.
- Require a valid proof-of-payment upload to mark an invoice paid. PDF, PNG, and JPEG files up to 10 MB are stored with the invoice in the database.
- Void sent invoices with a required reason. Users can request cancellation; Admins and Managers can authorize it. Voided invoices cannot be reactivated.
- Record overdue reminder simulations manually or on an optional background schedule. A unique invoice/window constraint prevents duplicate records.
- Sign in with ASP.NET Core Identity cookies. Roles are `Admin`, `Manager`, and `User`; invoice and customer APIs require authentication, and user administration is Admin-only.
- Manage users at `/users`: add and edit accounts, change roles, unlock locked accounts, reset passwords, and soft-delete accounts. Deleted users are omitted from the directory and cannot sign in. The last active Admin cannot be deleted or demoted.

## Solution Structure

`Capitec.slnx` contains four projects:

- `api/Capitec.Api`: HTTP controllers, request contracts, authentication and authorization setup, dependency registration, health checks, and the optional reminder worker.
- `api/Capitec.Business`: application services, service abstractions, commands, DTOs, and Identity/user-management workflows.
- `api/Capitec.DAL`: EF Core entities and `DbContext`, SQL Server configuration, migrations, repositories, and Unit of Work implementation.
- `api/Capitec.Api.Tests`: backend tests. Tests use an isolated SQLite database; the application provider is SQL Server.

The Angular application is under `web/`. Standalone page components are routed from `app.routes.ts`; `WorkspaceStore` owns shared invoice, customer, dashboard, and reminder state. The user-management page uses its own API calls and an Admin route guard. The server independently enforces Admin authorization on every user-management endpoint.

## Requirements

- .NET 10 SDK
- Node.js 22.12 or later in the Node 22 line, with npm
- A reachable SQL Server instance for local API execution
- Docker Desktop and Docker Compose for the container setup
- Microsoft Edge or Google Chrome to run Angular Karma tests

## Run Locally

The API uses the SQL Server EF Core provider. Set a SQL Server connection string and the bootstrap Admin credentials in the API process environment before starting it. For PowerShell, from the repository root:

```powershell
$env:ConnectionStrings__Capitec = 'Server=localhost,1433;Database=Capitec;User Id=sa;Password=<your-strong-password>;TrustServerCertificate=True'
$env:AdminUser__Username = 'admin@example.test'
$env:AdminUser__Password = '<your-strong-admin-password>'
dotnet run --project api/Capitec.Api --urls http://127.0.0.1:5105
```

The Identity seeder creates the configured Admin account on startup if it does not already exist. Passwords must meet the configured Identity policy: at least 8 characters, including an uppercase letter and a digit. Keep real credentials out of source control.

In a second terminal, start the Angular development server:

```powershell
cd web
npm ci
npm start
```

Open <http://127.0.0.1:4200>. The development server binds to loopback and forwards `/api` requests to `http://localhost:5105`.

Set `ConnectionStrings__Capitec` for the actual SQL Server provider. The `ConnectionStrings:Capitec` value in `appsettings.json` is SQLite-formatted and is not a usable connection string for the configured SQL Server provider.

## Build And Test

From the repository root:

```powershell
dotnet build Capitec.slnx -c Release
dotnet test Capitec.slnx -c Release
```

The API tests cover invoice idempotency, status and void rules, proof-of-payment validation and persistence, backdated invoices, and reminder generation/deduplication.

Build the frontend:

```powershell
cd web
npm ci
npm run build
```

Run the Angular tests with Microsoft Edge on Windows:

```powershell
$env:CHROME_BIN = 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'
npm test
Remove-Item Env:\CHROME_BIN
```

If Google Chrome is available on `PATH`, run `npm test` without setting `CHROME_BIN`.

## Docker Compose

Compose starts the API, SQL Server, and Nginx-hosted Angular frontend. Create a root-level `.env` file with values for the required database and bootstrap administrator settings before starting the stack:

```dotenv
DB_SERVER=sqlserver
DB_NAME=Capitec
DB_USER=sa
DB_PASSWORD=<your-strong-sql-server-password>
CAPITEC_ADMIN_USERNAME=admin@example.test
CAPITEC_ADMIN_PASSWORD=<your-strong-admin-password>
CAPITEC_SCHEDULED_SIMULATIONS=false
CAPITEC_SIMULATION_INTERVAL_MINUTES=60
CAPITEC_REMINDER_WINDOW_DAYS=7
CAPITEC_WEB_PORT=8080
```

Do not commit `.env` or use example credentials outside a local demo. Start the stack from the repository root:

```powershell
docker compose up --build
```

Open <http://127.0.0.1:8080>. Nginx proxies API and health requests to the API container. The API is not published on a host port; the web port binds to loopback. `CAPITEC_WEB_PORT` changes the host port. Compose declares a named `capitec-data` volume for SQL Server; verify the SQL Server data-directory mount before relying on it for persistence. `docker compose down --volumes` removes named volumes.

Useful commands:

```powershell
docker compose logs -f
docker compose down
```

Scheduled reminder simulations are disabled when `CAPITEC_SCHEDULED_SIMULATIONS=false`; manual simulations remain available. Do not expose the API directly or change the web binding to `0.0.0.0` without a deployment security review.

## API

All API paths below use the `/api` prefix. Except login and health, endpoints require an authenticated Identity cookie. `/api/admin/users` requires the `Admin` role.

| Endpoint | Access | Purpose |
| --- | --- | --- |
| `POST /auth/login` | Public | Sign in with email, password, and optional remember-me choice |
| `GET /auth/session` | Authenticated | Return authenticated status and role names |
| `POST /auth/logout` | Authenticated | Sign out |
| `POST /auth/register` | Admin | Register an account with a role |
| `GET /dashboard` | Authenticated | Dashboard totals |
| `GET /customers`, `GET /customers/{id}`, `POST /customers` | Authenticated | List, retrieve, and create customers |
| `GET /invoices`, `GET /invoices/{id}`, `POST /invoices` | Authenticated | List, retrieve, and create invoices |
| `PUT /invoices/{id}/status` | Authenticated | Update an allowed invoice status |
| `POST /invoices/{id}/payment-proof` | Authenticated | Upload proof and mark a sent invoice paid |
| `POST /invoices/{id}/void` | Authenticated | Request or authorize a void; body requires `reason` |
| `GET /notifications`, `POST /notifications/simulate` | Authenticated | List or run reminder simulations |
| `GET /admin/users` | Admin | List non-deleted user accounts |
| `POST /admin/users` | Admin | Create a user |
| `PUT /admin/users/{id}` | Admin | Update user details and role |
| `POST /admin/users/{id}/unlock` | Admin | Clear account lockout and failed-access count |
| `POST /admin/users/{id}/reset-password` | Admin | Set a replacement password |
| `DELETE /admin/users/{id}` | Admin | Soft-delete a user account |
| `GET /health` | Public | Health check |

Invoice creation requires an `Idempotency-Key` header. Replaying the same key and request returns the original invoice; reusing the key for a different request returns `409 Conflict`. Invoice numbers are assigned by the API. Only ZAR is supported.

Proof-of-payment uploads must contain a valid PDF, PNG, or JPEG file and be no larger than 10 MB. The status and document are saved together. Voiding requires a nonblank reason of at most 1000 characters. A `User` role can request cancellation; an `Admin` or `Manager` can authorize it. Paid invoices cannot be voided, and the generic status endpoint cannot bypass the proof or void workflows.

Reminder windows are seven days by default: the first simulated reminder is recorded for overdue days 1-7, the next for days 8-14, and so on. Manual and scheduled runs share a unique database constraint to avoid duplicate records.

## Configuration And Security Scope

The API reads the SQL Server connection string from `ConnectionStrings__Capitec`, the bootstrap account from `AdminUser__Username` and `AdminUser__Password`, and scheduling options from `Notifications__Enabled`, `Notifications__IntervalMinutes`, and `Notifications__WindowDays`. Docker Compose maps these to `DB_*`, `CAPITEC_ADMIN_*`, and `CAPITEC_*SIMULATIONS` variables.

Authentication and role-based authorization are enabled, but all authenticated users share the same workspace data. Tenant isolation, audit history for administrative actions, production secret management, backup/restore procedures, rate limiting, and a full deployment security review are not implemented. Cookie authentication is not a substitute for these controls. The app does not process payments, calculate taxes, generate invoice PDFs, or deliver real notifications. Review SQL Server volume persistence, HTTPS termination, proxy/header trust, and host restrictions before any hosted deployment.

> **Disclaimer:** This project was developed with AI assistance; maintainers have reviewed and changed parts of the generated work.
