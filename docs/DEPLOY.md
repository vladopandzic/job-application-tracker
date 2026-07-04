# JobTrek — Deployment

The frontend is a thin client over the API, so deployment = **two containers + one database**:

| Piece | What | Dockerfile |
|---|---|---|
| **API** | ASP.NET Core Web API (`...Api`) | `Dockerfile.api` |
| **Web** | Blazor Server host (`...Web.Root`) | `Dockerfile.web` |
| **DB** | PostgreSQL (app supports Postgres or SqlServer) | external (e.g. Neon) |

Both Dockerfiles are **platform-agnostic** — they run on Render, Azure Container Apps, Fly.io, or plain Docker. `.dockerignore` keeps the build context small.

## Build & run locally (test once Docker Desktop is running)

```bash
# from the repo root
docker build -f Dockerfile.api -t jobtrek-api .
docker build -f Dockerfile.web -t jobtrek-web .

# API (Postgres connection string via env)
docker run -p 5041:8080 \
  -e DatabaseProvider=Postgres \
  -e ConnectionStrings__JobApplicationDatabase="Host=...;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true" \
  -e EmployeeJwtTokenSettings__SecretKey="<random 64+ char secret>" \
  -e CandidateJwtTokenSettings__SecretKey="<same or another random secret>" \
  jobtrek-api

# Web (points at the API)
docker run -p 5185:8080 -e BaseApiUrl="http://localhost:5041" jobtrek-web
```

The container binds to `$PORT` if the platform sets it, else `8080`.

## Environment variables

### API
| Variable | Value |
|---|---|
| `DatabaseProvider` | `Postgres` |
| `ConnectionStrings__JobApplicationDatabase` | the Postgres connection string (Neon gives you one) |
| `EmployeeJwtTokenSettings__SecretKey` | **a strong random secret** (see security note) |
| `CandidateJwtTokenSettings__SecretKey` | a strong random secret |
| `EmployeeJwtTokenSettings__Issuer` / `__Audience` | keep or set to your domain |
| `CandidateJwtTokenSettings__Issuer` / `__Audience` | keep or set to your domain |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

### Web
| Variable | Value |
|---|---|
| `BaseApiUrl` | the **public** URL of the deployed API (e.g. `https://jobtrek-api.onrender.com`) |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

## ⚠️ Security — rotate the JWT secret (do this before any public deploy)

`Api/appsettings.json` ships a **committed** `SecretKey` (`this-is-my-secret-key-...`). That is fine for local dev but **must NOT be used in production** — anyone with the repo can forge tokens. Override it with a strong random value via the `EmployeeJwtTokenSettings__SecretKey` / `CandidateJwtTokenSettings__SecretKey` env vars. Generate one:

```bash
openssl rand -base64 64
```

Never commit the production secret — it lives only in the platform's env-var settings.

## Database & migrations

- App supports **Postgres** (`DatabaseProvider=Postgres`, migrations in `...Persistance.Migrations.PostgreSQL`) or SqlServer.
- A card-free managed Postgres like **Neon** (or Supabase) works — create a DB, copy the connection string into `ConnectionStrings__JobApplicationDatabase`.
- **The app does NOT auto-apply migrations on startup.** On a fresh DB you must either:
  1. Add a one-time `dbContext.Database.Migrate()` on API startup (simplest for a single instance), **or**
  2. Run migrations against the target DB before first run:
     ```bash
     dotnet ef database update \
       --project src/Procoding.ApplicationTracker.Persistance.Migrations.PostgreSQL \
       --startup-project src/Procoding.ApplicationTracker.Api
     ```
- Seeders (`TranslationSeeder`, and in Development `DevDataSeeder`) run on startup and populate base data.

## Blazor Server note

The Web host is **Blazor Server** (stateful, SignalR/WebSockets). On any host:
- WebSockets must be allowed (Render/Container Apps do).
- Prefer a single instance or enable **sticky sessions (session affinity)** so the SignalR circuit stays on the same instance.
- On free tiers that sleep when idle, the circuit drops on cold start — fine for a portfolio.

## Hosting options (card-free)

- **Render + Neon** — free web services (sleep when idle) + free Postgres, no card.
- **Azure Container Apps** — same Dockerfiles; Azure needs a card (or a card-free path: Azure for Students / an Azure Pass code from a Microsoft event).
- **Tunnel (cloudflared)** — for local demos only, no host needed.
