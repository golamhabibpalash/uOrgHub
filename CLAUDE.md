# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

uOrgHub — a modular ERP for a civil construction company. .NET 8 Web API backend
(PostgreSQL 16 via EF Core 8) + a React 19 / Vite / TypeScript frontend (`uOrgHub.Web`).

## Read first

`CODING_STANDARDS.md` (solution root) is the authoritative style guide: naming, entity
rules, soft-delete, table prefixes, commit format. **Follow it.** Other root docs worth
knowing: `BUSINESS_FLOW.md` (procure-to-pay / order-to-cash / project money flows),
`UORGHUB_SYSTEM_GUIDE.md` (feature-level system guide), `AGENTS.md` (condensed rules),
`CFL_DEPLOYMENT_INFO.md` + `deploy/README.md` (multi-instance deployment).

A few rules to keep front of mind:

- Every entity inherits `BaseEntity` (`uOrgHub.Shared/Entities/BaseEntity.cs`), uses `Guid` PK, and is soft-deleted (`IsDeleted = true`, never hard delete).
- Always `DateTime.UtcNow`, never `DateTime.Now`.
- Mapping is **Riok.Mapperly only** — never AutoMapper, never hand-mapping in handlers/controllers.
- Controllers always return `ApiResponse<T>` (`uOrgHub.Shared/Models/ApiResponse.cs`); paged endpoints return `ApiResponse<PagedResult<T>>`.
- Table names carry a module prefix via `[Table("...")]`: `hr_`, `acc_`, `inv_`, `proc_`, `proj_`.

> Note: CODING_STANDARDS.md §3/§6 describe an older Service+Repository layout. The code has
> since moved to **CQRS with MediatR** (see Architecture below). The *rules* in the standards
> still apply; the *folder layout* there is outdated — match existing modules, not the doc.

- **`uOrgHub.Shared` must never reference a business module** — every module depends on
  `Shared`, not the other way round. This governs where a cross-module entity can live: put it
  in `uOrgHub.Shared/Entities/` with *no* navigation properties/collections back into module
  entities (e.g. `Shared.Entities.Vendor` holds a bare `Guid? PayableAccountId`, not a
  `ChartOfAccount` nav). Configure that one relationship from the dependent module's side with
  `modelBuilder.Entity<Vendor>().HasOne<ChartOfAccount>().WithMany().HasForeignKey(...)` (the
  generic overload needs no navigation property on the Shared side).
- FluentValidation package versions differ per module: MediatR is unified at 12.4.1, but
  FluentValidation is 11.9.x everywhere except Accounts/Procurement/Projects (12.1.1) — match
  the local `.csproj` when adding refs or you'll get `NU1605`.
- `ValidationBehavior` validates the MediatR request **and every nested property that has a
  registered validator**. Commands wrap DTOs (`CreatePRCommand(CreatePRDto Dto)`), so write
  validators against the DTO, never the command. `IValidationRuleEngine`'s dynamic rules match
  by the DTO-derived entity name (`CreateEmployeeDto` → `Employee`). Validator test doubles must
  be public top-level classes — the assembly scan that wires validators skips nested/private
  types.

## Architecture (actual current pattern)

Each business module (`uOrgHub.HR`, `uOrgHub.Accounts`, `uOrgHub.Inventory`,
`uOrgHub.Procurement`, `uOrgHub.Projects`) is its own class library, registered in
`uOrgHub.API/Program.cs` via a `{Module}ServiceExtension.cs` (e.g. `AddHRModule()`).
Supporting libraries: `uOrgHub.Shared` (BaseEntity, `AppDbContext`, `ApiResponse`,
exceptions, `WhereSearch` — see below), `uOrgHub.Auth` (JWT + claims, below),
`uOrgHub.Settings` (system settings + `ValidationRuleEngine`). `uOrgHub.HR` is still the
reference module. `uOrgHub.Projects/Services/` holds domain services (financial
rollups, cost-limit checks) alongside its normal MediatR handlers — not the old
Service+Repository pattern.

Cross-module search convention: all list/search endpoints filter via the `WhereSearch()`
extension (`uOrgHub.Shared/Extensions/`), which emits PostgreSQL `ILIKE` for
case-insensitive partial matching — don't hand-roll `Contains`/`ToLower` filters. Paginated
query handlers sort via the `ApplySorting()` extension (same folder) instead of a hardcoded
`OrderBy`, so the frontend's column-header sort clicks work generically.

Request flow: **Controller → MediatR `_mediator.Send(command/query)` → Handler → Repository → `AppDbContext`.**

Module folder layout (see `uOrgHub.HR` as the reference module):
```
uOrgHub.{Module}/
├── Features/{Area}/Commands/{Entity}Commands.cs   # records + IRequestHandler, write side
├── Features/{Area}/Queries/{Entity}Queries.cs     # records + IRequestHandler, read side
├── Features/_Common/                              # ICommand<T>, ValidationBehavior, etc.
├── DTOs/  (+ DTOs/Validators/  FluentValidation)
├── Models/Entities/ + Models/Configurations/ (EF) + Models/Enums/
├── Mappings/{Entity}Mapper.cs                     # [Mapper] partial class
├── Repositories/I{Entity}Repository.cs + {Entity}Repository.cs
├── Reporting/                                     # export column definitions
└── {Module}ServiceExtension.cs                    # AddMediatR, validators, repos
```
- Commands/queries are `record`s implementing `ICommand<T>` / `IRequest<T>`; handlers implement `IRequestHandler<,>`.
- Validation runs as a MediatR `ValidationBehavior` pipeline (registered per module) — FluentValidation validators are picked up by assembly scan, not called manually.
- Throw typed exceptions from `uOrgHub.Shared/Exceptions/` (`NotFoundException`, `AppException`); `ExceptionMiddleware` translates them. Never return null/raw exceptions.

Controllers live in `uOrgHub.API/Controllers/{Module}/`, inherit `BaseController`, are
`[Authorize]` by default, and gate actions with `[RequireClaim(Claims....)]`. Auth lives in
`uOrgHub.Auth` (JWT + claim-based permissions: `Authorization/Claims.cs`, `Roles.cs`,
`AuthorizationCatalog.cs`); `PermissionMiddleware` enforces claims. Middleware order in
`Program.cs` is deliberate — don't reorder.

### Server-generated PDFs

Two separate QuestPDF template systems, not one shared "reports" layer:
- `uOrgHub.Shared/Export/Pdf/` (`ReportPdfPage`, `PdfTableStyles`, `PdfFormat`, `PdfLineTree`) —
  tabular financial reports (Accounts).
- `uOrgHub.Procurement/Reporting/Pdf/` + `ProcurementDocumentPage.cs` — letterhead-style
  documents (PR/RFQ today; reuse this one, not the Shared tabular one, for future PO/GRN
  documents).

### Money-flow map

`BUSINESS_FLOW.md` traces procure-to-pay / order-to-cash / project-costing across modules and
explicitly lists the points where the chain is wired vs. only present in schema — read it before
assuming two modules' data is connected just because both reference the same concept (e.g. two
modules independently modeling "vendor" or "customer" with no FK between them is the kind of gap
it tracks).

## Frontend (`uOrgHub.Web`)

React 19 + Vite + TypeScript + Tailwind + shadcn/ui. State: Zustand (`src/store/authStore.ts`)
+ TanStack Query. Routing: react-router-dom v7.
- `src/api/client.ts` — axios instance: injects JWT, auto-refreshes on 401 (with request queue), toasts API messages/errors. Use it for all calls.
- `src/api/{module}.ts` — typed API functions + TS interfaces per module.
- `src/pages/{module}/` — page components, one folder per module.
- Backend `ApiResponse<T>` / `PagedResult<T>` shapes mirrored in `src/types/api.ts`.
- Every list page uses the shared `DataGrid` component (`src/components/shared/DataGrid.tsx`)
  + `useDataGrid` hook (`src/hooks/useDataGrid.ts`) for paging/sort/search/filter state — don't
  use the old `DataTable`/`Pagination` components. See CODING_STANDARDS.md §18 for the exact
  page-component pattern and the `DataGridColumn`/`useDataGrid` APIs.
- A 403 response fires a global `auth:forbidden` event (handled in `client.ts`) rather than
  being left for each call site to catch. Only `/uploads` is proxied to the API in
  `vite.config.ts` — everything else goes through `VITE_API_URL`.
- Printing (`ReportLayout.tsx`'s `handlePrint`, extracted into `src/utils/printDocument.ts`)
  renders into a detached off-screen `<iframe>` that **clones the live page's own compiled
  `<link>`/`<style>` tags**, not a fresh stylesheet load — a fresh load races Tailwind's CDN
  timing and prints washed-out/unstyled content. Reuse this utility for new print surfaces
  instead of opening a new tab or re-deriving styles.

## Commands

Backend (run from solution root):
```bash
docker-compose up -d                       # start PostgreSQL
dotnet build                               # build solution
dotnet run --project uOrgHub.API           # API → http://localhost:5177 (Swagger at /swagger)
dotnet test                                # xUnit + Moq + FluentAssertions
dotnet test --filter "FullyQualifiedName~DepartmentHandler"   # single test class
```
Frontend (from `uOrgHub.Web`):
```bash
npm run dev      # Vite dev server (proxies to API; default VITE_API_URL http://localhost:5177/api/v1)
npm run build    # tsc -b && vite build
npm run lint     # eslint .
```
There is no frontend unit-test runner (`playwright` is a dependency but only for browser
automation). All automated tests are backend xUnit in `uOrgHub.Tests`.

EF Core migrations (live in `uOrgHub.Shared/Data/Migrations/`, applied automatically on API
startup via `db.Database.Migrate()` in `Program.cs`):
```bash
dotnet ef migrations add Add{Module}Module \
  --project uOrgHub.Shared/uOrgHub.Shared.csproj \
  --startup-project uOrgHub.API/uOrgHub.API.csproj \
  --output-dir Data/Migrations
```
Never edit migration files by hand.

- SDK is pinned by `global.json` (8.0.400, rollForward latestPatch).
- `dotnet test` needs **no database** — `uOrgHub.Tests/TestDb.cs` uses EF InMemory. The API
  itself needs Postgres to run for real (auto-migrates + seeds on startup); if it's down at
  boot, the API logs a warning and still starts.
- CI (`.github/workflows/ci.yml`): `dotnet build` and the frontend build are hard gates;
  `dotnet test` and `npm run lint` are `continue-on-error` (known pre-existing failures) — still
  fix what you touch, but don't feel obligated to chase the whole backlog in an unrelated change.

## Config notes

- DB connection: `uOrgHub.API/appsettings*.json` → `ConnectionStrings:DefaultConnection`
  (local dev points at **Port 5433**, not 5432 — `README.md`'s Database Configuration table
  still says 5432; that's stale, trust `docker-compose.yml`/`appsettings*.json`). Local
  container: DB `orgHub`, user `postgres`, password `Admin1234!`.
- Secrets for deployment come from `.env` (copy `.env.example`); never commit real secrets.
- Adding a new module: create the class library, add a `{Module}ServiceExtension`, register it
  in `Program.cs`, add controllers under `Controllers/{Module}/`, define claims in `uOrgHub.Auth`,
  and add the matching `src/api/{module}.ts` + `src/pages/{module}/` on the frontend.
- Deploying: never run `deploy/docker-compose.yml` directly — use
  `sudo ./deploy/deploy.sh <instance>` (see `deploy/README.md`). The frontend bakes
  `VITE_API_URL` in at build time, so each deployed instance needs its own web image tag.
