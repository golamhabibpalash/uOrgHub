# uOrgHub ERP — Coding Standards & Conventions

## 1. Technology Stack
- **Framework:** .NET 8 Web API (C#)
- **ORM:** Entity Framework Core 8
- **Database:** PostgreSQL 16 (via Npgsql)
- **Mapping:** Riok.Mapperly (never use AutoMapper)
- **Validation:** FluentValidation
- **Testing:** xUnit + Moq + FluentAssertions
- **Auth:** JWT Bearer tokens

---

## 2. Solution Structure
```
uOrgHub.API          → Entry point, controllers, middleware, program.cs
uOrgHub.Shared       → Base classes, models, interfaces, exceptions, DbContext
uOrgHub.HR           → HR & Payroll module
uOrgHub.Accounts     → Accounts & Finance module
uOrgHub.Inventory    → Inventory Management module
uOrgHub.Procurement  → Procurement module
uOrgHub.Projects     → Project Management module
uOrgHub.Tests        → All unit and integration tests
```

---

## 3. Module Folder Structure
Every module MUST follow this exact structure — no exceptions:
```
uOrgHub.{Module}/
├── Models/
│   └── Entities/
│       └── {Entity}.cs
├── DTOs/
│   ├── Create{Entity}Dto.cs
│   ├── Update{Entity}Dto.cs
│   └── {Entity}ResponseDto.cs
├── Repositories/
│   ├── I{Entity}Repository.cs
│   └── {Entity}Repository.cs
├── Services/
│   ├── I{Entity}Service.cs
│   └── {Entity}Service.cs
├── Mappings/
│   └── {Entity}Mapper.cs
└── {Module}ServiceExtension.cs
```

Controllers live in `uOrgHub.API/Controllers/{Module}/`

---

## 4. Naming Conventions

| Item | Convention | Example |
|---|---|---|
| Classes | PascalCase | EmployeeService |
| Interfaces | I + PascalCase | IEmployeeService |
| Methods | PascalCase | GetByIdAsync |
| Variables | camelCase | employeeId |
| Private fields | _camelCase | _repository |
| Constants | UPPER_SNAKE | MAX_PAGE_SIZE |
| DTOs | PascalCase + suffix | CreateEmployeeDto |
| Controllers | Entity + Controller | EmployeeController |
| Migrations | descriptive name | AddHRModule |

---

## 5. Entity Rules
- Every entity MUST inherit from `BaseEntity` (uOrgHub.Shared/Entities/BaseEntity.cs)
- Always use `Guid` as primary key — never `int`
- Never use `DateTime.Now` — always use `DateTime.UtcNow`
- Soft delete only — set `IsDeleted = true`, never DELETE from database
- All string properties that are required must have `[Required]` and `MaxLength`

```csharp
// CORRECT
public class Employee : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
}

// WRONG — never do this
public class Employee
{
    public int Id { get; set; }  // wrong type
    public string FirstName { get; set; }  // missing constraints
}
```

---

## 6. Repository Pattern Rules
- Controller calls Service only — never calls Repository directly
- Service calls Repository only — never calls DbContext directly
- Repository calls DbContext only
- Every repository MUST implement `IBaseRepository<T>`
- Always filter soft-deleted records: `.Where(x => !x.IsDeleted)`

```csharp
// CORRECT repository query
public async Task<Employee?> GetByIdAsync(Guid id)
    => await _context.Employees
        .Where(x => !x.IsDeleted && x.Id == id)
        .FirstOrDefaultAsync();

// WRONG — missing soft delete filter
public async Task<Employee?> GetByIdAsync(Guid id)
    => await _context.Employees.FindAsync(id);
```

---

## 7. API Response Rules
- ALL controller endpoints MUST return `ApiResponse<T>`
- Never return raw objects from controllers
- Use correct HTTP status codes

```csharp
// CORRECT
[HttpGet("{id:guid}")]
public async Task<IActionResult> GetById(Guid id)
{
    var result = await _service.GetByIdAsync(id);
    return Ok(ApiResponse<EmployeeResponseDto>.Ok(result));
}

// WRONG
[HttpGet("{id:guid}")]
public async Task<Employee> GetById(Guid id)  // never return raw entity
    => await _service.GetByIdAsync(id);
```

---

## 8. Controller Rules
- All controllers MUST inherit from `BaseController`
- Route format: `api/v1/[controller]`
- Always use `[Authorize]` unless explicitly public
- Always use `Guid` route constraints: `{id:guid}`
- Controller only handles HTTP — no business logic inside controllers

```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class EmployeeController : BaseController
{
    // ...
}
```

---

## 9. Async Rules
- ALL service and repository methods MUST be async
- Always use `Async` suffix on async methods
- Always use `CancellationToken` on public async methods
- Never use `.Result` or `.Wait()` — always await

```csharp
// CORRECT
public async Task<EmployeeResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)

// WRONG
public EmployeeResponseDto GetById(Guid id)
```

---

## 10. Mapping Rules
- Always use Mapperly (Riok.Mapperly) for entity ↔ DTO mapping
- Never map manually in service or controller
- One mapper class per entity in the Mappings/ folder

```csharp
// CORRECT
[Mapper]
public partial class EmployeeMapper
{
    public partial EmployeeResponseDto ToDto(Employee entity);
    public partial Employee ToEntity(CreateEmployeeDto dto);
    public partial void UpdateEntity(UpdateEmployeeDto dto, Employee entity);
}
```

---

## 11. Exception Rules
- Never return raw exceptions to the client
- Always throw typed exceptions from `uOrgHub.Shared/Exceptions/`
- ExceptionMiddleware handles all exceptions globally

```csharp
// CORRECT
var employee = await _repository.GetByIdAsync(id)
    ?? throw new NotFoundException(nameof(Employee), id);

// WRONG
if (employee == null) return null;
```

---

## 12. Validation Rules
- Use FluentValidation for all Create and Update DTOs
- Register validators in the module's ServiceExtension
- Validators live in `DTOs/Validators/` folder

---

## 13. DI Registration Rules
- Every module MUST have a `{Module}ServiceExtension.cs`
- Register all services, repositories, validators inside it
- Call the extension from `Program.cs` only

```csharp
// HRServiceExtension.cs
public static class HRServiceExtension
{
    public static IServiceCollection AddHRModule(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        // ... other registrations
        return services;
    }
}

// Program.cs
builder.Services.AddHRModule();
```

---

## 14. Migration Rules
- One migration per module: named `Add{Module}Module`
- Never edit migration files manually
- Always run `dotnet ef database update` after migration
- Migration command format:
```bash
dotnet ef migrations add Add{Module}Module \
  --project uOrgHub.Shared/uOrgHub.Shared.csproj \
  --startup-project uOrgHub.API/uOrgHub.API.csproj \
  --output-dir Data/Migrations
```

---

## 15. Git Commit Rules
Format: `{type}: {short description}`

| Type | When |
|---|---|
| feat | new feature or module |
| fix | bug fix |
| refactor | code change, no feature |
| init | first setup |
| migration | database migration added |
| test | adding tests |

Examples:
```
feat: add Employee CRUD endpoints
fix: soft delete filter missing in EmployeeRepository
migration: AddHRModule migration
```

---

## 16. Claude Code Prompt Header
Always start every Claude Code session with this header:

```
You are working on uOrgHub, a .NET 8 ERP for a civil construction company.
Before writing any code, read and strictly follow: CODING_STANDARDS.md at the solution root.
Reference module for patterns: uOrgHub.HR (once built).
Never use AutoMapper — use Riok.Mapperly only.
Never use int as primary key — always Guid.
Never hard delete — always soft delete.
Always return ApiResponse<T> from controllers.
Always inherit BaseEntity for all entities.
```
## 17. Table Naming Convention
Every entity MUST have a table prefix matching its module.
Use [Table("prefix_tablename")] attribute on every entity class.

| Module | Prefix |
|---|---|
| HR | hr_ |
| Accounts | acc_ |
| Inventory | inv_ |
| Procurement | proc_ |
| Projects | proj_ |

Example:
[Table("hr_employees")]
public class Employee : BaseEntity { ... }

---

## 18. DataGrid / List Page Convention

All list pages MUST use the `DataGrid` component + `useDataGrid` hook. Do NOT use the old `DataTable` or `Pagination` components.

### Backend — Query Handler Sorting

Every paginated query handler or repository `GetAllAsync` MUST use `ApplySorting()` instead of hardcoded `OrderBy`:

```csharp
// CORRECT — dynamic sorting
query = query.ApplySorting(request.Request.SortBy ?? "Name", request.Request.SortDescending);

// WRONG — hardcoded column
query = request.Request.SortDescending
    ? query.OrderByDescending(x => x.Name)
    : query.OrderBy(x => x.Name);
```

### Frontend — Page Structure

Every list page MUST follow this exact pattern:

```tsx
import DataGrid from "../../components/shared/DataGrid";
import ExportMenu from "../../components/shared/ExportMenu";
import { useDataGrid } from "../../hooks/useDataGrid";

export default function SomeList() {
  const dg = useDataGrid({ defaultSortBy: "name" });

  const { data, isLoading } = useQuery({
    queryKey: ["entity", dg.page, dg.search, dg.sortBy, dg.sortDescending /*, extra filters */],
    queryFn: () => getEntities(dg.queryParams /*, extra filters */),
  });

  const items = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;

  const columns: DataGridColumn<EntityType>[] = [
    { key: "name", label: "Name" },                              // sortable by default
    { key: "code", label: "Code" },
    { key: "status", label: "Status", sortable: false,           // opt out of sorting
      render: (row) => <StatusBadge active={row.isActive} /> },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-base font-medium text-gray-900">Title</h2>
        <button onClick={openAdd} className="...">+ Add</button>
      </div>
      <DataGrid
        columns={columns}
        data={items}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        onEdit={openEdit}
        onDelete={handleDelete}
        emptyMessage="No records found"
        toolbarPrefix={/* optional filter dropdowns */}
        actions={<ExportMenu baseUrl="..." filters={...} />}
      />
      <Modal ... />
      <ConfirmDialog ... />
    </div>
  );
}
```

### DataGridColumn Properties

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `key` | `string` | required | Maps to `SortBy` value on backend |
| `label` | `string` | required | Column header text |
| `sortable` | `boolean` | `true` | Show sort arrows in header |
| `render` | `(row: T) => ReactNode` | — | Custom cell renderer |
| `className` | `string` | — | Cell-level CSS class |
| `headerClassName` | `string` | — | Header cell CSS class |
| `width` | `string` | — | Column width (e.g. `"120px"`) |

### useDataGrid Hook API

```typescript
const dg = useDataGrid({ defaultSortBy?: string, defaultSortDescending?: boolean, defaultPageSize?: number });

// State
dg.page / dg.setPage(n)
dg.pageSize / dg.setPageSize(n)
dg.search / dg.setSearch(val)        // auto-resets page to 1
dg.sortBy / dg.sortDescending
dg.handleSort(columnKey)             // cycles none → asc → desc
dg.filters / dg.setFilter(key, val)  // column-specific filters
dg.resetFilters()
dg.queryParams                       // ready-to-pass PaginationRequest object
```
