# AGENTS.md — SemsApi

SEMS (School Evaluation Management System) backend — ASP.NET Core Web API on .NET 9, EF Core + SQL Server, ASP.NET Identity, JWT auth (ACCESS_TOKEN cookie) + Google sign-in restricted to the dmc.edu.ph hosted domain. Single project in `SemsApi.sln`: `SemsApi/SemsApi.csproj`. Runtime depends on a reachable SQL Server (connection string in `appsettings.Development.json`, which is gitignored and holds secrets — never commit it or paste its values).

## Dev environment

- Prerequisites: .NET 9 SDK (csproj targets `net9.0`; other SDK majors will not build it) and SQL Server per `appsettings.Development.json` `DefaultConnection` (current setup: SQLEXPRESS on a dev machine).
- Startup seeds roles via `Seeders/RoleSeeder` (Program.cs) — the app will not boot without a working DB connection.
- Frontend dev origin for CORS: `https://localhost:5173` (`Frontend:BaseUrl` in dev settings).

## Build, run, test

- Build: `dotnet build SemsApi.sln`
- Run: `dotnet run --project SemsApi/SemsApi.csproj` → `http://localhost:5277` (http profile) / `https://localhost:7278` (https profile); Swagger UI at `/swagger` in Development (see `Properties/launchSettings.json`).
- Migrations: `dotnet ef migrations add <Name> --project SemsApi/SemsApi.csproj`, `dotnet ef database update --project SemsApi/SemsApi.csproj`.
- Tests: none — there is no test project in the solution. Verify with `dotnet build`; no linter/formatter is configured. `Nullable` and `ImplicitUsings` are enabled in the csproj.
- API smoke checks: Postman collections under `SemsApi/postman/`, ad-hoc requests in `SemsApi/SemsApi.http`.

## Conventions

- Feature-based layers under `SemsApi/SemsApi/`: `Controllers/` (thin — `[ApiController]`, `Route("api/<resource>")`, `CancellationToken` passed through to the service), `Services/` (`*Service` classes), `Interfaces/` (`I*Service`), `DTO/<Feature>/` (`*Dto`, `Create*Request`, `Update*Request`), `Models/`, `Data/ApplicationDbContext.cs`, `Migrations/`, `Seeders/`, `Utilities/` (helpers + `GlobalExceptionHandler`), `Exceptions/`, `Enums/`.
- Scrutor auto-registers any `*Service` class as scoped against its interface (Program.cs `builder.Services.Scan`) — new services need no manual DI registration.
- Read settings via `builder.Configuration[...]` directly (Program.cs) — do not add Options-pattern classes.
- Errors flow through `GlobalExceptionHandler` (`AddExceptionHandler` + `ProblemDetails`): services throw, controllers do not handle exceptions themselves. Some controllers still contain `debug_userId` dev-bypass params — do not copy that pattern.
- Commits are imperative sentences ("Add facility evaluation model classes"); feature branches are `devmork/EA-<ticket>-<slug>` (Jira ticket key in branch name).

## Pitfalls

- JWT is sent via the `ACCESS_TOKEN` **cookie**, not the `Authorization` header — the Swagger "Bearer" scheme in Program.cs is misleading for testing.
- `[Authorize]` is commented out on several endpoints (e.g. `FacilitiesController` POST/PUT/DELETE) — auth is not enforced everywhere.
- Google sign-in is locked to `hd = dmc.edu.ph` (Program.cs) — non-DMC accounts cannot sign in locally.
- `SemsApi/appsettings.Development.json` (secrets) and `SemsApi/repomix-output.xml` (~500 KB generated dump) are gitignored but present in the working tree — do not edit or commit them.
