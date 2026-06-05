# uOrgHub — Agent Instructions

## Quick Commands

```bash
# Backend
dotnet build uOrgHub.sln
dotnet test
dotnet run --project uOrgHub.API
dotnet ef migrations add Add{Module}Module --project uOrgHub.Shared --startup-project uOrgHub.API --output-dir Data/Migrations

# Frontend
cd uOrgHub.Web
npm run build    # tsc -b && vite build
npm run lint     # eslint .
npm run dev      # vite
```

## Architecture

- **Modular monolith** (.NET 8 + PostgreSQL 16 + React/TypeScript frontend)
- Backend modules: `uOrgHub.HR`, `uOrgHub.Accounts`, `uOrgHub.Inventory`, `uOrgHub.Procurement`, `uOrgHub.Projects`, `uOrgHub.Auth`, `uOrgHub.Shared`
- API entrypoint: `uOrgHub.API` (references all modules)
- Frontend: `uOrgHub.Web` (Vite + React + TanStack Query + Tailwind)

## Critical Conventions (from CODING_STANDARDS.md)

| Rule | Detail |
|------|--------|
| **PK** | Always `Guid`, never `int` |
| **Time** | `DateTime.UtcNow` only, never `DateTime.Now` |
| **Delete** | Soft delete only (`IsDeleted = true`) |
| **Mapping** | Riok.Mapperly only — no AutoMapper |
| **Response** | All controllers return `ApiResponse<T>` |
| **Validation** | FluentValidation on Create/Update DTOs |
| **Async** | All service/repo methods async + `CancellationToken` |
| **Table names** | Module prefix: `hr_`, `acc_`, `inv_`, `proc_`, `proj_` |

## Module Structure (every module)

```
uOrgHub.{Module}/
├── Models/Entities/{Entity}.cs
├── DTOs/{Create,Update,}{Entity}ResponseDto.cs + Validators/
├── Repositories/I{Entity}Repository.cs + {Entity}Repository.cs
├── Services/I{Entity}Service.cs + {Entity}Service.cs
├── Mappings/{Entity}Mapper.cs (Mapperly)
└── {Module}ServiceExtension.cs  (DI registration)
```

## Controller Rules

- Inherit `BaseController`
- Route: `api/v1/[controller]`
- `[Authorize]` by default
- `{id:guid}` route constraints
- No business logic in controllers

## Search Behavior

All search queries use `WhereSearch()` extension (PostgreSQL `ILike`) — case-insensitive partial matching across all modules.

## Database

```bash
docker-compose up -d  # PostgreSQL 16 on port 5433
```
Connection: `localhost:5433`, DB `orgHub`, user `postgres`, pwd `Admin1234!`

## Git Commits

Format: `{type}: {description}`
Types: `feat`, `fix`, `refactor`, `init`, `migration`, `test`

## Frontend Notes

- API calls in `src/api/*.ts` (e.g., `hr.ts`, `inventory.ts`)
- Pages in `src/pages/{module}/`
- Shared components in `src/components/shared/`
- Layout: `AppLayout` + `Sidebar` (active state fixed via `WhereSearch`/`end` prop logic)