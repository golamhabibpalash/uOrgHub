# uOrgHub — Agent Instructions

## Quick Commands

```bash
# Backend
dotnet build uOrgHub.sln
dotnet test
dotnet run --project uOrgHub.API
dotnet ef migrations add AddXxx \
  --project uOrgHub.Shared --startup-project uOrgHub.API --output-dir Data/Migrations

# Frontend
cd uOrgHub.Web
npm run build    # tsc -b && vite build
npm run lint     # eslint .
npm run dev
```

## Architecture

- **Modular monolith**: .NET 8 + PostgreSQL 16 + React/TS (Vite + TanStack Query + Tailwind).
- Modules: `uOrgHub.API` (entrypoint, controllers, middleware), `uOrgHub.Shared` (AppDbContext, BaseEntity, ApiResponse, shared helpers), `uOrgHub.Auth`, `uOrgHub.HR`, `uOrgHub.Accounts`, `uOrgHub.Inventory`, `uOrgHub.Procurement`, `uOrgHub.Projects`, `uOrgHub.Settings`, `uOrgHub.Tests`.
- All migrations live in `uOrgHub.Shared/Data/Migrations/`; Program.cs auto-migrates on startup then runs seeds (`IAuthSeeder`, `SettingsSeeder`).
- EF Configurations: `uOrgHub.{Module}/Models/Configurations/{Entity}Configuration.cs` (IEntityTypeConfiguration).

## Current API pattern: MediatR — the `Services/` layout is STALE

`CODING_STANDARDS.md` holds the root conventions, but its **Services layer structure is outdated**. Current modules use MediatR feature slices:

```
uOrgHub.{Module}/Features/{Feature}/Commands/{Feature}Commands.cs   # records + handlers
uOrgHub.{Module}/Features/{Feature}/Queries/{Feature}Queries.cs
```

- Pattern: `record XxxCommand(...) : ICommand<ResultDto>` (and `IQuery<T>`) with a sibling `XxxCommandHandler : IRequestHandler<XxxCommand, ResultDto>`. Controllers call `_mediator.Send(...)`.
- Marker interfaces + `ValidationBehavior` live in `Features/_Common/ICommand.cs` (validates FluentValidation + dynamic `IValidationRuleEngine`).
- Each module's `{Module}ServiceExtension.cs` registers `AddMediatR(RegisterServicesFromAssembly + AddOpenBehavior(ValidationBehavior<,>))` plus `AddValidatorsFromAssembly`.
- Accounts also has **legacy `Services/` for some flows** — when editing, mirror whatever that file/feature already does.

## Controllers

- In `uOrgHub.API/Controllers/{Module}/`, inherit `BaseController`, `[Authorize]`, route `api/v1/[controller]`, `{id:guid}` constraints.
- Endpoints are permission-gated with `[RequireClaim(Claims.{Module}.{Entity}.{Action})]` — constants in `uOrgHub.Auth/Authorization/Claims.cs`. Add both the attribute AND a `Claims` constant when creating an endpoint.
- Return `ApiResponse<T>.Ok(...)`; throw typed exceptions from `uOrgHub.Shared/Exceptions` (global ExceptionMiddleware handles them). Never return raw entities.

## Shared helpers (uOrgHub.Shared/Extensions + Models)

- `WhereSearch(query, term, props...)` — case-insensitive ILike partial search; `ApplySorting(sortBy, sortDesc)` — both used by every list query. `PaginationRequest` / `PagedResult<T>` drive all list endpoints.
- Export: `IExportService` (Shared) + `Reporting/ExportColumns/{Entity}ExportColumns.cs` per module; list pages expose `[HttpGet("export?format=csv|xlsx")]`.

## Conventions (CODING_STANDARDS.md is source of truth)

`Guid` PK · `DateTime.UtcNow` only · soft delete only (always filter `!x.IsDeleted`) · Riok.Mapperly, never AutoMapper · FluentValidation on Create/Update DTOs in `DTOs/Validators/` · async + `CancellationToken` everywhere · `[Table("inv_...")]` prefix per module (`hr_`, `acc_`, `inv_`, `proc_`, `proj_`).

## Database

- Local dev: root `docker-compose.yml` → postgres:16 on `localhost:5433`, DB `orgHub`, user `postgres`, pwd `Admin1234!`.
- `deploy/docker-compose.yml` is instance-agnostic — never run directly; use `sudo ./deploy/deploy.sh <instance>` (`deploy/README.md`, `deploy/RUNBOOK-*.md`).

## Frontend

- Base URL: `VITE_API_URL` else `http://localhost:5177/api/v1` (`src/api/client.ts`). 401 auto-refreshes token; 403 dispatches `auth:forbidden` window event.
- List pages use `DataGrid` + `useDataGrid` + `ExportMenu` (CODING_STANDARDS.md §18). Query params are camelCase and map to backend `PaginationRequest`.

## Git

`{type}: {desc}` — feat | fix | refactor | init | migration | test