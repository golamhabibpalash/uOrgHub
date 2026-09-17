# uOrgHub — Agent Instructions

Modular-monolith ERP: .NET 8 API + PostgreSQL 16 + React 19/Vite/TS frontend (`uOrgHub.Web`).
`CODING_STANDARDS.md` is the style authority, but its §3/§6 **Services+Repository layout is stale** — the code uses MediatR slices. Follow the rules there, match existing modules for layout. Reference module: `uOrgHub.HR`.

## Commands (backend from root, frontend from `uOrgHub.Web`)

```bash
docker compose up -d                          # postgres:16 on localhost:5433 (DB orgHub / postgres / Admin1234!)
dotnet build                                  # solution gate — must compile (CI blocks on this)
dotnet test --filter "FullyQualifiedName~DepartmentHandler"  # single test class
dotnet run --project uOrgHub.API              # API → http://localhost:5177, Swagger at /swagger
dotnet ef migrations add AddXxx --project uOrgHub.Shared/uOrgHub.Shared.csproj \
  --startup-project uOrgHub.API/uOrgHub.API.csproj --output-dir Data/Migrations

cd uOrgHub.Web && npm run build               # tsc -b && vite build (CI gate) | npm run lint | npm run dev
```

- SDK pinned by `global.json` (8.0.400, rollForward latestPatch).
- `dotnet test` needs **no database** — `uOrgHub.Tests/TestDb.cs` uses EF InMemory. The API itself needs postgres (auto-migrates + seeds on startup); if DB is down it logs a warning and still starts.
- CI (`/.github/workflows/ci.yml`): build gates; `dotnet test` and `npm run lint` are `continue-on-error` (known pre-existing failures) — still fix what you touch, but don't chase the backlog.
- No frontend test runner (`playwright` dep is browser-automation only).
- README's "port 5432" is wrong — local dev is **5433** per `docker-compose.yml` + `appsettings*.json`.

## Backend pattern (Controller → MediatR → Handler → Repository → AppDbContext)

```
uOrgHub.{Module}/Features/{Area}/Commands/{Entity}Commands.cs  # record XxxCommand : ICommand<T> + handler
uOrgHub.{Module}/Features/{Area}/Queries/{Entity}Queries.cs
uOrgHub.{Module}/{DTOs,Models/Entities+Configurations+Enums,Mappings,Repositories,Reporting}/
uOrgHub.{Module}/{Module}ServiceExtension.cs  # AddMediatR + ValidationBehavior + validators + repos; registered in Program.cs
```

- Validation: shared `uOrgHub.Shared/Behaviors/ValidationBehavior<,>`, wired via `AddOpenBehavior` in all 5 modules. It validates the request plus every nested property with a registered validator — commands wrap DTOs (`CreatePRCommand(CreatePRDto Dto)`), so write validators against the **DTO**, never the command. Dynamic `IValidationRuleEngine` rules match the DTO-derived entity name (`CreateEmployeeDto` → `Employee`).
- Package versions are split: MediatR unified at 12.4.1, but FluentValidation is 11.9.x everywhere except Accounts/Procurement/Projects (12.1.1) — match the local csproj when adding refs or NU1605 fails the build. Test validator doubles must be public top-level types (assembly scan skips nested/private).
- Some modules also keep `Services/` for domain flows (`Accounts/`, `Projects/`, `Auth/`, `Settings/`) — mirror whatever the file you're editing already does.
- New module checklist: class lib + `{Module}ServiceExtension` + register in `Program.cs` + controllers in `uOrgHub.API/Controllers/{Module}/` + claims in `uOrgHub.Auth/Authorization/Claims.cs` + `src/api/{module}.ts` + `src/pages/{module}/`.

## Controllers / auth / responses

- Inherit `BaseController`, `[Authorize]`, route `api/v1/[controller]`, `{id:guid}` constraints. Gate with `[RequireClaim(Claims.{Module}.{Entity}.{Action})]` — add the `Claims` constant too.
- Return `ApiResponse<T>.Ok(...)` (paged: `ApiResponse<PagedResult<T>>`); throw typed `uOrgHub.Shared/Exceptions` (`NotFoundException`, `AppException`). Never return raw entities or null.
- Middleware order in `Program.cs` is deliberate — don't reorder (Maintenance → Exception → AccessLog → Auth → Permission).

## Data conventions

`Guid` PK · `BaseEntity` · soft delete only (always `!x.IsDeleted`) · `DateTime.UtcNow` · `[Table]` prefix per module (`hr_ acc_ inv_ proc_ proj_`) · Mapperly only (never AutoMapper/hand-map) · FluentValidation in `DTOs/Validators/` · async + `CancellationToken`.
- List queries: `WhereSearch(term, props...)` (Postgres `ILIKE`, don't hand-roll `Contains`/`ToLower`) + `ApplySorting(sortBy, sortDesc)` · `PaginationRequest`/`PagedResult<T>` · export via `IExportService` + per-module `ExportColumns/`.

## Migrations / deploy

- All migrations live in `uOrgHub.Shared/Data/Migrations/`; never edit by hand. `Program.cs` runs `Migrate()` then `IAuthSeeder` + `SettingsSeeder`.
- Deploy: never run `deploy/docker-compose.yml` directly — `sudo ./deploy/deploy.sh <instance>` (`deploy/README.md`). `VITE_API_URL` is **baked at build time**, so each instance has its own web image tag. Secrets via `.env` (copy `.env.example`); never commit.

## Frontend

- All calls via `src/api/client.ts` (JWT inject + 401 refresh queue + toasts); 403 fires `auth:forbidden`. Shapes in `src/types/api.ts` mirror `ApiResponse`/`PagedResult`.
- List pages: `DataGrid` + `useDataGrid` + `ExportMenu` only (never `DataTable`/`Pagination`); query params camelCase → `PaginationRequest`. Pattern: `CODING_STANDARDS.md` §18.
- State: Zustand `authStore`; routing react-router v7; only `/uploads` is proxied to the API in `vite.config.ts`.

## Git

`{type}: {desc}` — feat | fix | refactor | init | migration | test
