# Capitec Business Invoicing

A small business invoicing workspace built with Angular 20 and ASP.NET Core 10. It stores customers, invoices, line items, and simulated overdue reminders in SQLite. The reminder feature records what would have been sent; it never sends email, SMS, or other external messages.

> **Local demo only.** This initial slice has no authentication, authorization, or tenant isolation. It is not safe for real customer or financial data and must not be exposed to a network or deployed as a production service. The local development servers and Docker web port bind to loopback only. Add identity, tenant scoping, authorization policies, and a security review before considering any shared or hosted deployment.

## Features

- Create customer billing contacts and invoices with multiple line items.
- Track due dates, invoice status, totals, overdue days, and customer balances.
- Search and filter invoices, mark sent invoices paid, and export the current invoice list as CSV.
- Void sent invoices with a required, persisted reason; a voided invoice cannot be edited or reactivated.
- Trigger a reminder simulation manually or enable an optional background schedule. Both modes use the same unique invoice and policy-window constraint, so repeated runs do not duplicate reminder records.
- Keep SQLite data across container restarts using a named Docker volume.

## Further Considerations

Below items are not implemented, only thought out.
- `Authentication and Authorization:` These could be implemented using Identity, OAuth 2.0, or a simple password. Controllers can be locked using the `Authorize` attributes. Where necessary, role-based authorization can be used. Other authentication schemes to be used are JWT, cookies and 2-FA.
- `Database design:` Currently SQLite is used with minimum migrations. For production ready system, various SQL database providers such as MSSQL Server, PostgreSQL, Oracle can be used. Preffered choice being MSSQL Server. Entity Framework Core, with code first migrations would be preferable. Where database exists, scaffolding using database-first migration is key. Current system follows simple relationship and cardinalities. Invoice items link to invoice, invoices links to a customer.
- `Invoice Completion:` In cases where an invoice is to be marked as 'Paid', a user would be required to upload proof payment. Upon review, the uploaded files would be available to view.
- `Invoice Cancellation:` When an invoice is voided, an intemediate status is set until the invoice is reviewed by a supervisor, or a supervisor code is supplied.
- `Email Service Integration:` This is crucial and will be used to send out emails to customers when an invoice is generated, due, overdue or paid.
- `Payment Gateway Integration:` Generated invoices could contain a link to payment page. Upon successful payment, the system automatically mark the invoice as 'Paid'
- `Partial Payment:` A useful feature that will enable to customers to pay their invoice in parts and/or intervals.

## API Architecture

The .NET solution uses a dependency-inverted n-tier structure:

- `Capitec.Domain` contains persistence-agnostic business entities and invoice status.
- `Capitec.Application` contains use cases, application DTOs, repository contracts, and the Unit of Work abstractions. It references Domain only.
- `Capitec.Infrastructure` implements repositories and the Unit of Work with EF Core/SQLite, and owns database initialization and persistence exception translation.
- `Capitec.Api` contains HTTP controllers, request validation, hosted scheduling, and composition-root registration. Controllers call Application services and do not access EF Core directly.

The Unit of Work coordinates repositories, `SaveChanges`, and transaction commit/rollback. Application services depend on `IUnitOfWorkFactory` and repository interfaces; only Infrastructure knows about `DbContext`.

Customer, invoice, line-item, and simulated-notification IDs are integer identities. The demo database schema was reset when switching from GUID IDs; if reusing an older local or Docker database, back up any data you need and recreate its schema before starting the updated version. The GUID-valued `Idempotency-Key` request header is an operation token, not an entity ID.

## Angular UI Structure

The Angular root component is the workspace shell: it owns navigation, routed content, global API feedback overlays, and the shared dialog host. `/overview`, `/invoices`, `/customers`, and `/reminders` are separate standalone page components. `WorkspaceStore` centralizes shared API-backed state and actions; reusable page-heading, invoice-action, and dialog-host components keep presentation consistent across pages.

## Requirements

- .NET 10 SDK
- Node.js 22.12 or later in the Node 22 line, with npm
- Docker Desktop and Docker Compose for container use
- Microsoft Edge or Google Chrome to run the Angular Karma tests

## Run Locally

Open two terminals from the `Capitec` project directory.

Start the API:

```powershell
dotnet run --project api/Capitec.Api --urls http://127.0.0.1:5105
```

Start the Angular development server:

```powershell
cd web
npm ci
npm start
```

Open <http://127.0.0.1:4200>. The Angular development server binds to loopback and forwards `/api` requests to the API at `http://localhost:5105`. The API creates `capitec.db` in its working directory on first run.

## Build And Test

From the `Capitec` directory, build and test the API:

```powershell
dotnet build Capitec.slnx -c Release
dotnet test Capitec.slnx -c Release
```

The API tests cover invoice idempotency, conflicting reuse of an idempotency key, backdated invoices, and sequential/concurrent reminder deduplication through the Unit of Work.

Build the frontend:

```powershell
cd web
npm ci
npm run build
```

Run the Angular component tests with Microsoft Edge on Windows:

```powershell
$env:CHROME_BIN = 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'
npm test
Remove-Item Env:\CHROME_BIN
```

On systems with Google Chrome available on `PATH`, run `npm test` without setting `CHROME_BIN`.

## Docker

Build and run both the API and Angular frontend:

```powershell
docker compose up --build
```

Open <http://127.0.0.1:8080>. The frontend is served by Nginx, which proxies API and health requests to the ASP.NET Core service. The API container runs as a non-root user and is not published on a host port; SQLite persists in the `capitec-data` named volume. The published web port is loopback-only by default.

Useful commands:

```powershell
docker compose logs -f
docker compose down
```

Stopping the services preserves the database. `docker compose down --volumes` removes the stored data as well.

Set `CAPITEC_WEB_PORT` to change the loopback host port. Scheduled simulations are disabled by default. To enable them, set `CAPITEC_SCHEDULED_SIMULATIONS=true`; optional schedule values are `CAPITEC_SIMULATION_INTERVAL_MINUTES` (default `60`) and `CAPITEC_REMINDER_WINDOW_DAYS` (default `7`). Manual simulations remain available regardless of the scheduler setting. Do not change the Compose port binding to `0.0.0.0` or expose the API service directly.

## API

- `GET /api/dashboard`
- `GET|POST /api/customers`, `GET /api/customers/{id}`
- `GET|POST /api/invoices`, `GET /api/invoices/{id}`, `PUT /api/invoices/{id}/status`, `POST /api/invoices/{id}/void`
- `POST /api/invoices/{id}/void` with a required `reason` field
- `GET /api/notifications`, `POST /api/notifications/simulate`
- `GET /health`

Invoice creation requires an `Idempotency-Key` header. Replaying the same key and payload returns the original invoice; using that key for a different payload returns `409 Conflict`. Invoice numbers are generated by the API. Only ZAR is enabled at this stage.

Voiding a sent invoice requires a nonblank reason of at most 1000 characters. The reason is stored on the invoice and included in API responses. Voiding a paid invoice is rejected, and the generic status endpoint cannot set `Void` without a reason.

Invoices may be entered with a past due date, which supports businesses bringing existing receivables into the workspace. Each overdue reminder window is seven days by default: the first reminder is recorded for overdue days 1–7, the next for days 8–14, and so on. The unique database index makes manual and scheduled runs converge on the same reminder record.

## Configuration And Scope

The API reads its SQLite connection string from `ConnectionStrings__Capitec` and reminder settings from `Notifications__Enabled`, `Notifications__IntervalMinutes`, and `Notifications__WindowDays`. Scheduled processing is off by default, including in local development.

This is an unauthenticated, single-workspace local demo, not a production-ready financial platform. Before any shared or hosted use, add authentication, tenant isolation on every data query and write, authorization policies, database migrations and backup/restore operations, audit retention policies, rate limiting, HTTPS termination, and an operational secrets strategy. Review proxy/header trust and allowed hosts as part of that deployment work. The Compose setup binds the UI to loopback and keeps the API private to its internal Docker network; it does not substitute for authentication. The API does not consume forwarded client/protocol headers. This demo does not provide payment processing, tax calculations, PDF generation, or real notification delivery.



> **Disclaimer:** A great deal of this project is build using A.I agents. Changes were made to parts where I disagreed with the agents.