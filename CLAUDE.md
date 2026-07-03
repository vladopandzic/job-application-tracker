# CLAUDE.md

Guidance for working in this repository. This is a **job application tracker** built as a
learning/portfolio project to practice Clean Architecture, DDD, and CQRS in .NET. It is
explicitly **work in progress** — expect rough edges, dead code, and hardcoded values in
places (see "Known rough edges" at the bottom). When something looks half-finished, it
probably is; prefer asking over assuming it's intentional.

## Domain in one paragraph

Two kinds of users: **Candidates** (job seekers) and **Employees** (internal staff/admins).
A Candidate applies for a job at a **Company**, through a **JobApplicationSource** (LinkedIn,
referral, etc.), producing a **JobApplication**. A JobApplication has a `JobType`,
`WorkLocationType`, a `JobApplicationStatus`, and a list of `InterviewStep`s. Both user types
authenticate with JWTs, but via **separate auth schemes** (`BearerEmployee` / `BearerCandidate`).

## Tech stack

- **.NET 8** (all projects target `net8.0`)
- **Blazor Server** frontend + **MudBlazor** UI + MVVM (ViewModels)
- **ASP.NET Core Web API** backend, endpoints via **Ardalis.ApiEndpoints**
- **EF Core** with **PostgreSQL** (Npgsql). SQL Server was used earlier; the codebase is
  built for **multiple providers**, with migrations kept in a dedicated assembly.
- **MediatR** (CQRS), **FluentValidation**, **Mapster** (mapping), **LanguageExt** (`Result<T>`)
- **Ardalis.Specification** (query specifications)
- **.NET Aspire** (`AppHost` + `ServiceDefaults`) for local orchestration
- **Testcontainers** for integration tests, **NetArchTest** for architecture tests
- Deployed to **AWS Elastic Beanstalk** via **GitHub Actions**

## Solution layout

Open `Procoding.ApplicationTracker.Domain.sln` (name is historical — it holds the whole solution,
not just Domain). Projects live under `src/` and `test/`.

### Production projects (`src/`)
| Project | Role |
|---|---|
| `...Domain` | Entities, value objects, domain events, repository interfaces, exceptions. **No dependencies on other projects.** |
| `...Application` | CQRS commands/queries + handlers, validators, specifications, auth/JWT logic. Depends only on Domain + DTOs. |
| `...DTOs` | Request / Response / Model DTOs shared between API and Web. |
| `...Infrastructure` | EF Core `ApplicationDbContext`, entity configurations, repository implementations, Identity user stores, SQL Server migrations, seed data. |
| `...Persistance.Migrations.PostgreSQL` | PostgreSQL migrations only (separate assembly so multiple providers can coexist). |
| `...Api` | Web API host. Ardalis endpoints, JWT auth, Swagger, global exception handling. **Composition root for the API.** |
| `...Web` | Blazor components, pages, ViewModels, HttpClient services, `AppAdapter`. Library, **not** a host. |
| `...Web.Root` | **Composition root for the Blazor app** — the actual runnable web host. Wires up DI and calls `AppAdapter`. |
| `...AppHost` | .NET Aspire orchestrator (runs Web.Root + Api together). |
| `...ServiceDefaults` | Aspire shared defaults (telemetry, health checks, service discovery). |

### Test projects (`test/`)
| Project | What it covers |
|---|---|
| `...Domain.Tests` | Unit tests for entities / value objects (NUnit + FluentAssertions). |
| `...Application.Tests` | Command handler unit tests. |
| `...Api.IntegrationTests` | Full HTTP round-trips against a **real Postgres in Testcontainers**. |
| `...Web.Tests` | Web helper tests. |
| `...Architecture.Tests` | **NetArchTest** rules enforcing layer boundaries, naming, EF config, repository conventions. |
| `...Api.Tests` | Currently just the template `UnitTest1` — effectively empty. |

## Architecture rules (enforced by tests — respect them)

The `Architecture.Tests` project fails the build if these are violated:

- **Domain** depends on nothing else in the solution.
- **Application** does not depend on Infrastructure / Web / Web.Root.
- **Infrastructure** does not depend on Web / Web.Root.
- **Web** does not depend on Infrastructure / Web.Root.
- The Web app is **not** the composition root — `Web.Root` is. Keep host wiring there.

Dependency direction: `Web(.Root) → Application → Domain`, `Infrastructure → Application/Domain`,
DTOs shared by API and Web. Never reference Infrastructure from Application or Domain.

## Core patterns (match these when adding code)

### CQRS via MediatR
- Commands: `Application/<Feature>/Commands/<Name>/` → `XCommand`, `XCommandHandler`, `XCommandValidator`.
- Queries: `Application/<Feature>/Queries/<Name>/` → `XQuery`, `XQueryHandler`.
- Handlers implement `ICommandHandler<TCommand, TResponse>` / `IQueryHandler<...>` (thin wrappers
  over MediatR's `IRequestHandler`, defined in `Application/Core/Abstractions/Messaging`).
- Handlers are `internal sealed`; commands/queries are `public sealed`.
- Handlers return **`LanguageExt.Common.Result<T>`**, not raw values. Endpoints unwrap with
  `result.Match<IActionResult>(Ok, err => BadRequest(err.MapToResponse()))`.

### DDD domain model
- Aggregate roots derive from `AggregateRoot`; entities from `EntityBase`. Roots: `Candidate`,
  `Company`, `JobApplication`, `JobApplicationSource`, `Employee`.
- Entities have a **private parameterless ctor for EF** (wrapped in `#pragma warning disable CS8618`)
  and are created through **static `Create(...)` factory methods**, not `new`.
- Setters are `private set` (or get-only); state changes go through **behavior methods**
  (e.g. `JobApplication.Update(...)`, `CreateNewInterview(...)`, `Candidate.ApplyForAJob(...)`).
- Behavior methods raise **domain events** via `AddDomainEvent(...)` (events in `Domain/Events`).
  ⚠️ Dispatch is currently **commented out** in `ApplicationDbContext.SaveChangesAsync` —
  events are recorded but not published yet.
- **Value objects** (`Domain/ValueObjects`): `Email`, `Link`, `CompanyName`, `Currency` wrap a
  primitive and validate in the ctor (throwing e.g. `InvalidEmailException`).
- **Enumeration pattern** (`Domain/Common/Enumeration`): `InterviewStepType` is a type-safe enum
  class with static members. `JobType` / `WorkLocationType` are `ValueObject`s wrapping a string.
  `JobApplicationStatus` is a **plain C# enum** (`Applied`, `InProcess`, `Offer`, `Accepted`,
  `Rejected`, `Withdrawed`), stored as a string column. Status changes go through
  `JobApplication.ChangeStatus(...)`, which enforces allowed transitions (see `AllowedTransitions`:
  `Applied → InProcess → Offer → Accepted`, plus `Rejected`/`Withdrawed` from any non-terminal state)
  and raises `JobApplicationStatusChangedDomainEvent`. The Blazor **Kanban board**
  (`/my-job-applications/board`) is the primary UI for this — drag a card to a column to change status.

### Persistence
- `IUnitOfWork` is implemented by `ApplicationDbContext`; commit with `_unitOfWork.SaveChangesAsync(ct)`.
- **Auditing** (`IAuditableEntity`: `CreatedOnUtc`/`ModifiedOnUtc`) and **soft delete**
  (`ISoftDeletableEntity`: `DeletedOnUtc`) are applied automatically in `SaveChangesAsync`.
- A **global query filter** on `JobApplication` scopes rows to the current user: employees see
  all, candidates see only their own (via `IIdentityContext`).
- Repositories (`Infrastructure/Repositories`, `internal sealed`) implement interfaces from
  `Domain/Repositories` and use **Ardalis.Specification** (`Application/Specifications`) for
  list/filter queries.
- EF mappings live in `Infrastructure/Configurations` as `IEntityTypeConfiguration<T>`, auto-applied
  via `ApplyConfigurationsFromAssembly`. Register new repositories in
  `Infrastructure/ServiceCollectionExtensions.AddPersistance`.

### API layer
- One class per endpoint using `Ardalis.ApiEndpoints`
  (`EndpointBaseAsync.WithRequest<TReq>.WithResult<IActionResult>`), grouped by feature under
  `Api/Endpoints/<Feature>/`.
- Endpoints inject `ISender` (MediatR) and dispatch a command/query; they do not contain business logic.
- **Two JWT schemes**: `BearerEmployee` and `BearerCandidate`, with policies `Policies.EmployeeOnly`
  / `Policies.CandidateOnly` (see `Api/Policy.cs` and `Program.cs`). Guard endpoints with
  `[Authorize(AuthenticationSchemes = "BearerEmployee,BearerCandidate", Policy = Policies.XOnly)]`.
- Validation via FluentValidation (`Api/Validation` + `Application/.../*Validator`). Errors surface
  as RFC7807 **ProblemDetails** through `GlobalExceptionHandler`.

### Web layer (Blazor + MVVM)
- Pages are split: `Foo.razor` (markup) + `Foo.razor.cs` (`partial class`, code-behind).
- Each page injects a **ViewModel** (`Web/ViewModels/...`, derive from `ViewModelBase`); the VM
  holds state + calls a **service**. Pages call `ViewModel.InitializeViewModel()` in
  `OnInitializedAsync`. Keep logic in the ViewModel, not the page.
- **Services** (`Web/Services/...`) are typed `HttpClient` wrappers over the API, named `"ServerApi"`,
  base URL from config key `BaseApiUrl`, with a Polly retry policy. Interfaces in `Services/Interfaces`.
- DI wiring for the web app is in `Web.Root/Program.cs` + `AppAdapter` + `ViewModels/IServiceCollectionExtensions`.

## Build / run / test

Run from the repo root (PowerShell). Requires the **.NET 8 SDK**; integration tests require **Docker**.

```powershell
dotnet restore Procoding.ApplicationTracker.Domain.sln
dotnet build   Procoding.ApplicationTracker.Domain.sln -c Release

# Run everything via Aspire (Web.Root + Api together)
dotnet run --project src/Procoding.ApplicationTracker.AppHost

# Or run the API alone (Swagger at /swagger, https://localhost:7140)
dotnet run --project src/Procoding.ApplicationTracker.Api

# Run the Blazor app alone
dotnet run --project src/Procoding.ApplicationTracker.Web.Root

# Tests
dotnet test Procoding.ApplicationTracker.Domain.sln                      # all
dotnet test test/Procoding.Architecture.Tests                            # architecture only (fast)
dotnet test test/Procoding.ApplicationTracker.Api.IntegrationTests       # needs Docker running
```

### Database
- **Provider is switchable** via the `DatabaseProvider` config key (`Postgres` | `SqlServer`),
  read in `Api/Program.cs`. Both connection strings use the same key `ConnectionStrings:JobApplicationDatabase`.
  - **Postgres** migrations live in `...Persistance.Migrations.PostgreSQL`; connection e.g.
    `Host=localhost;Database=ApplicationDbTracker;Username=postgres;Password=admin`.
  - **SqlServer** migrations live in the `...Infrastructure` assembly; connection e.g.
    `Server=localhost;Database=JobApplicationTracker;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;`.
  - The dev default (`Api/appsettings.Development.json`) is currently **SqlServer** (temporary).
    Flip back by setting `"DatabaseProvider": "Postgres"` and the Postgres connection string.
- Adding a migration — target the assembly for the provider you're generating for, e.g. Postgres:
  ```powershell
  dotnet ef migrations add <Name> `
    --project src/Procoding.ApplicationTracker.Persistance.Migrations.PostgreSQL `
    --startup-project src/Procoding.ApplicationTracker.Api
  ```
  Apply migrations with `dotnet ef database update --project <migrations-assembly> --startup-project src/Procoding.ApplicationTracker.Api`
  (set `ASPNETCORE_ENVIRONMENT=Development` so the right provider/connection string is picked up).
- Integration tests spin up their **own** Postgres container (Testcontainers) — no local DB needed
  for them, only a running Docker daemon.

## CI/CD

`.github/workflows/dotnet.yml`: on push to `master` / any PR → restore, build (Release), `dotnet test`.
On `master`, a `deploy` job pushes Web.Root and Api to **AWS Elastic Beanstalk** (`eu-west-1`) with
`dotnet eb deploy-environment`. AWS creds come from repo secrets.
Note: the workflow still installs the **.NET 7** SDK even though projects target net8.0 — a build on
a clean runner may need this bumped to `8.0.x`.

## Conventions

- Namespaces are file-scoped and mirror folders. Naming: `PascalCase` types, `_camelCase` private fields.
- DTOs are named by direction/role: `*RequestDTO`, `*ResponseDTO`, `*DTO` (models).
- Tests: **NUnit** + **FluentAssertions**; test data via `TestData/*TestData.cs` classes.
- Times are UTC; get "now" from the injected `TimeProvider`, never `DateTime.UtcNow` directly.

## Known rough edges (WIP — don't treat as reference behavior)

These are known-imperfect; if you touch them, consider fixing rather than copying:
- Domain-event dispatch is **commented out** in `ApplicationDbContext.SaveChangesAsync` — status
  changes and other domain events are recorded on the aggregate but never published/handled yet.
- Placeholder/dead files exist: `Application/ISomething.cs`, `Application/Something.cs`,
  `Api/WeatherForecast.cs`, `Web/Pages/Counter.razor`, `Weather.razor`; `Api.Tests` is the empty template.
- The JWT `SecretKey` is committed in `Api/appsettings.json` — fine for local dev, **must not** be
  used in production.
- `TestDatabaseHelper` still contains SQL Server / hardcoded-connection remnants alongside the
  Testcontainers Postgres path used by `TestBase`.
- The solution file is named `...Domain.sln` for historical reasons; it contains the full solution.
