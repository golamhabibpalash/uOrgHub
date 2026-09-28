# SISTER_CONCERN_PLAN.md

Design + rollout plan for managing **sister concerns** (affiliate/group companies) in uOrgHub —
one login, several legally distinct books.

This is a companion to `BUSINESS_FLOW.md` (money spine) and `CLAUDE.md` (architecture). It is a
**living plan document**, not a description of shipped behaviour — the "Status" line at the top
of each section says what's actually built. Every claim cites a `file.cs:line` so it can be
checked against the code, same convention as `BUSINESS_FLOW.md`.

> **Status as of 2026-09-21: Phases 1 (Accounts), 2 (Procurement), 3 (Projects), and 4a
> (Inventory) shipped and complete.** Accounts' `JournalEntry`, `Voucher`, `Bill`, `Invoice`,
> `Payment`, `Budget`, `BankAccount`, `NumberingSequence`, `FiscalYear`, `CostCenter`;
> Procurement's `PurchaseRequisition`, `RequestForQuotation`, `VendorQuotation`, `PurchaseOrder`,
> `GoodsReceivedNote`; Projects' `Project`; and Inventory's `Warehouse`, `StockBalance`,
> `StockTransaction` are all company-scoped, with isolation enforced end-to-end (login → JWT
> claim → write-time stamping → read-time filtering), verified against real Postgres — see §8.
> `Customer`/`ChartOfAccount`/`AccountGroup`/`TaxRate`/`Vendor`/`Client`/`Item`/`ItemVariant`
> remain deliberately shared (§3) — that was always the intended design, not a gap. HR (§6, phase
> 4b — `PayrollCycle`, `ExpenseRequest`) is the only module left unbuilt. This document exists so
> that remaining work can proceed module-by-module without re-deriving the design.

---

## 1. Where this stands today

- `uOrgHub.Shared/Entities/Company.cs` — a company profile row (name, address, logo, currency,
  timezone). Multiple rows are already possible.
- `uOrgHub.Auth/Models/Entities/UserCompany.cs` — join table: `UserId` ↔ `CompanyId`, with
  `RoleInCompany` and `IsDefault`. A user can already be linked to more than one company.
- `uOrgHub.API/Controllers/CompanyController.cs` — first-run setup wizard (`POST
  company/setup`), list/get/update/logo-upload endpoints, plus (new) `GET
  company/my-companies` and `POST company/switch/{companyId}` for the company switcher.
- **Shipped**: Accounts' 10 anchor entities, Procurement's 5 anchor entities, Projects' single
  anchor, and Inventory's 3 entities (§3) carry `CompanyId`, are stamped automatically on create,
  and are filtered automatically on every read — see §4 for the mechanism and §8 for how it was
  verified. Everything else (HR, and the deliberately-shared master data in §3) is still globally
  shared.

---

## 2. Chosen model: shared master data, separate transactions

Decided (2026-09-21): **not** full row-level multi-tenancy. Master/reference data stays global
across all sister concerns; financial and transactional *documents* get isolated per company.
Concretely:

- **Shared globally** (no `CompanyId`, visible/reusable from any sister concern): party masters
  (Vendor, Customer, Client), the inventory catalogue (Item/ItemVariant/UoM/Category), the chart
  of accounts structure, and HR people/org data (Employee, Department, Designation, and the
  salary/leave/shift setup tables). A vendor or an employee is the same real-world entity no
  matter which sister concern is transacting with them.
- **Scoped per company** (`CompanyId` on the entity, enforced by a global EF query filter):
  every document that represents money moving, stock moving, or a project's books — a Bill, a
  Voucher, a Journal Entry, a Purchase Order, a Project, a Payroll Cycle. Each sister concern's
  P&L, balance sheet, and stock position stay separate even though the address book they draw
  from is shared.

This mirrors real accounting practice for a group of companies: one set of contacts and people,
several separate ledgers.

---

## 3. Entity classification

Root **anchor** entities get their own `CompanyId` column. Everything hanging off an anchor via a
parent FK (line items, sub-documents) inherits scoping through the parent — it does **not** get
its own `CompanyId`; the query filter reaches it by joining/including the parent, same as the
existing soft-delete convention doesn't require every child row to re-check `IsDeleted` if the
parent is already filtered.

### Shared master data — no `CompanyId`

| Module | Entities |
|---|---|
| Shared | `Vendor` (`uOrgHub.Shared/Entities/Vendor.cs`) |
| Accounts | `Customer`, `ChartOfAccount`, `AccountGroup`, `TaxRate` |
| HR | `Employee`, `Department`, `Designation`, `LeaveType`, `SalaryComponent`, `SalaryGrade`, `Shift`, `WorkSchedule`, `OvertimeRule` |
| Inventory | `Item`, `ItemVariant`, `InventoryCategory`, `InventoryType`, `UnitOfMeasure`, `AttributeDefinition`, `VariantAttribute` |
| Projects | `Client`, `ProjectCategory` |

### Company-scoped anchors — gets `CompanyId` + global query filter

| Module | Anchor entity | Children that inherit scoping via the anchor's FK (no `CompanyId` of their own) |
|---|---|---|
| Accounts | `JournalEntry` | `JournalEntryLine` |
| Accounts | `Voucher` | — |
| Accounts | `Bill` | `BillLine` |
| Accounts | `Invoice` | `InvoiceLine` |
| Accounts | `Payment` | `PaymentAllocation` |
| Accounts | `Budget` | `BudgetLine` |
| Accounts | `BankAccount` | `BankTransaction` |
| Accounts | `NumberingSequence` | — (must be company-scoped: see §7, doc numbers can't share a counter across sister concerns) |
| Accounts | `FiscalYear` | — |
| Accounts | `CostCenter` | — (children below reach it via `CostCenterId`, not the other way round) |
| Procurement | `PurchaseRequisition` | `PurchaseRequisitionItem` |
| Procurement | `RequestForQuotation` | `RFQItem` |
| Procurement | `VendorQuotation` | `VendorQuotationItem` |
| Procurement | `PurchaseOrder` | `PurchaseOrderItem` |
| Procurement | `GoodsReceivedNote` | `GRNItem` |
| Projects | `Project` | `WorkBreakdownStructure`, `BillOfQuantity` (+`BOQItem`), `RABill` (+`RABillItem`), `ProjectBudget`, `ProjectExpense`, `ProjectMilestone`, `ProjectTeam`, `DailyProgressReport`, `SiteResourceAllocation`, `SafetyIncident`, `QAChecklist`(+item), `ProjectRFI`, `ProjectSubmittal`, `ProjectDrawing`, `ProjectStatusLog`, `ProjectMaterialRequest`(+item) |
| Inventory | `Warehouse` | — (`StockBalance`, `StockTransaction` key off `WarehouseId` but carry their own `CompanyId` too — see §6 phase 4a for why they couldn't just inherit it) |
| Inventory | `StockBalance` | — |
| Inventory | `StockTransaction` | — |
| HR | `PayrollCycle` | `PayrollEntry` |
| HR | `ExpenseRequest` | — |

### Resolved during implementation

`CostCenter` and `FiscalYear` were originally flagged here as open/undecided and shipped in
phase 1b (`20260921110159_UnifySisterConcernAccountsPhase1b.cs`) after the initial Phase 1 review
surfaced them as a real gap — a `Bill`/`Invoice`/`Payment`/`Budget` requires a `FiscalYearId`, and
leaving `FiscalYear` global would have meant two sister concerns sharing the same period-close
state, which isn't "separate books":
- **`CostCenter`** — company-scoped (`(CompanyId, Code)` composite unique, replacing the old bare
  `Code` unique index). It's also linked from `Project` via a bare `ProjectId` (`Accounts` can't
  reference `Projects` — see `BUSINESS_FLOW.md` §4), which stays as-is; that FK direction doesn't
  interact with company scoping since `ProjectId` is never joined through, only stored.
- **`FiscalYear`** — company-scoped. `FiscalYearService.GetCurrentAsync`/`SetCurrentAsync`
  needed no code changes — "the current fiscal year" now resolves per company automatically
  through the same global filter every other scoped entity uses.

---

## 4. Mechanism (as built)

### 4.1 Resolving "current company"

`JwtService.GenerateAccessToken`/`GenerateRefreshToken` (`uOrgHub.Auth/Services/JwtService.cs`)
take an optional `Guid? companyId` and, when present, add it as a `"company_id"` claim.
`AuthService` resolves that value three different ways depending on the flow:

- **Login/2FA verify** (`IssueFullLoginAsync`): the user's default `UserCompany` row
  (`IUserRepository.GetDefaultCompanyIdAsync` — `IsDefault` first, else earliest-assigned).
- **Refresh** (`RefreshTokenAsync`): the `CompanyId` stored on the `RefreshToken` row being
  redeemed, *not* the user's default — otherwise a mid-session company switch would silently
  revert the next time the access token naturally expires. `RefreshToken` gained a `CompanyId`
  column for exactly this (`uOrgHub.Auth/Models/Entities/RefreshToken.cs`).
- **Switch** (`AuthService.SwitchCompanyAsync`, called from `POST
  company/switch/{companyId}`): validates the caller has a `UserCompany` row for the target
  company, then mints a fresh access+refresh pair carrying it — same shape as login/refresh, no
  new frontend flow needed beyond replacing the stored tokens.

`GET auth/me` and `GET company/my-companies` round out the picture: the former returns the
profile including `activeCompanyId`/`activeCompanyName`/`companies[]` (derived from the caller's
JWT `company_id` claim, read via a new `AuthController.GetCompanyId()`), the latter lists every
company a user can switch into (used by the frontend switcher, §4.4).

### 4.2 Stamping `CompanyId` on write

`AuditInterceptor` (`uOrgHub.Shared/Data/AuditInterceptor.cs`) already reached into
`IHttpContextAccessor` inside `SaveChanges`/`SaveChangesAsync` to stamp `CreatedBy`/`CreatedAt`
on every `BaseEntity` — the isolation stamping reuses that same read of the request's claims
rather than adding a second lookup. A new marker interface, `ICompanyScoped` (bare `Guid
CompanyId`, in `uOrgHub.Shared/Entities/` — deliberately *not* folded into `BaseEntity`, since
`NumberingSequence` doesn't inherit it at all and several company-scoped children don't need
their own column), is picked up in a second `ChangeTracker.Entries<ICompanyScoped>()` loop: on
`EntityState.Added`, if `CompanyId == Guid.Empty`, set it from the same `"company_id"` claim.
Handlers never set `CompanyId` themselves.

### 4.3 Enforcing isolation on read

This is where the original plan (a `HasQueryFilter` declared inside each entity's own
`IEntityTypeConfiguration<T>`) turned out to be unbuildable and had to change: a filter that
closes over instance state (`_companyId`, a field on `AppDbContext`) can only be declared where
that field is in scope — inside `AppDbContext.OnModelCreating` itself — because
`ApplyConfigurationsFromAssembly` instantiates every module's `IEntityTypeConfiguration<T>` with
`Activator.CreateInstance(type)` and no constructor arguments, so a per-entity config class can
never see it. But `AppDbContext` lives in `uOrgHub.Shared`, which must never reference a business
module (`CLAUDE.md`), so it can't list `Accounts.Models.Entities.Bill` etc. by name either.

The fix actually shipped: `AppDbContext` reflects over `modelBuilder.Model.GetEntityTypes()`
*after* every module's configurations have been applied, finds every CLR type implementing
`ICompanyScoped` (regardless of which module it came from), and calls a private generic
`SetCompanyScopeFilter<TEntity>()` via `MakeGenericMethod` for each one — `e => _companyId ==
null || e.CompanyId == _companyId`. This means adding isolation to a *new* entity in any future
module is exactly: implement `ICompanyScoped`, add the column + FK/index in that entity's own
configuration (unchanged from the original plan) — nothing in `uOrgHub.Shared` ever needs
touching again. Each entity's own configuration still owns its FK/index as always; only the
*filter* moved.

The **soft-delete "only one HasQueryFilter" collision the original plan worried about never
actually arose**: none of the 8 Accounts anchors had a `HasQueryFilter(!IsDeleted)` to begin with
— like most module entities (`CLAUDE.md`), they filter it manually per-handler (`.Where(x =>
!x.IsDeleted)`), so the new company-scope filter is each entity's only global filter, no merge
needed. It *would* need merging (`!x.IsDeleted && x.CompanyId == ...`) if it's ever added to an
entity that already carries `HasQueryFilter(!IsDeleted)` (`Vendor`, `Company`, most of
`uOrgHub.Auth`) — keep that in mind if any of those ever becomes company-scoped.

`_companyId == null` (no claim — a seeder, a migration, a test, or a user with no `UserCompany`
row yet) means **don't filter**, not **show nothing**: `TestDb.NewContext()` passes no
`ICurrentCompanyAccessor` at all, so all 647 existing tests kept passing unchanged (confirmed,
§8) without needing to know isolation now exists.

### 4.4 Frontend

- `src/store/authStore.ts`'s `UserProfileDto` gained `activeCompanyId`, `activeCompanyName`, and
  `companies?: UserCompanyDto[]` (optional — a user object rehydrated from localStorage written
  before this shipped won't have it).
- `src/components/layout/CompanySwitcher.tsx` — a dropdown in `Topbar.tsx`, rendered only when
  `companies.length >= 2` (the common case is one company, where showing a switcher would be
  clutter). Calls `POST company/switch/{id}` (`src/api/auth.ts`'s `switchCompany`) and replaces
  the stored tokens/user via the existing `authStore.setAuth`.
- No page-level changes were needed beyond the switcher, confirming the original plan's
  prediction: every existing list/create/edit page already goes through `src/api/client.ts`, so
  once the backend filters/stamps transparently by JWT claim, `src/api/{module}.ts` and
  `src/pages/{module}/` needed zero changes for the 8 newly-scoped entities.

### 4.5 New users and company membership

Not in the original plan, found necessary during implementation: a user with **no**
`UserCompany` row carries no `"company_id"` claim at all, which (per §4.3) means they'd see
*every* company's data rather than none — the opposite of isolation. Two places now guarantee
every user has at least one membership row:
- `UserManagementService.CreateUserAsync` — a newly-created user is added to every company that
  exists at creation time (first one as `IsDefault`). There's no per-user company picker in the
  admin UI yet, so this is a deliberately permissive default matched to today's reality (every
  install has exactly one company in practice); an admin can't yet prune a multi-company user's
  access from the UI.
- The migration (§5) backfills a `UserCompany` row for every pre-existing user with none.

---

## 5. Migration — `20260921101647_UnifySisterConcernAccountsPhase1`

Shipped, following the same data-preserving pattern proven for the Vendor unification migration
(`20260917191746_UnifyVendorMaster.cs`): never drop or constrain data before it's backfilled.
Hand-edited after `dotnet ef migrations add` (as always — EF's auto-generated version added
`CompanyId` as `NOT NULL DEFAULT Guid.Empty`, which would have made the FK-to-`companies` step
fail outright on any table with existing rows, since nothing has `Id = Guid.Empty`):

1. Add `CompanyId` as **nullable** on all 8 tables (+ `auth_refresh_tokens`, which stays nullable
   permanently — see §4.1).
2. Backfill: `UPDATE {table} SET "CompanyId" = (SELECT "Id" FROM companies ORDER BY "CreatedAt"
   LIMIT 1) WHERE "CompanyId" IS NULL` — every pre-existing row belongs to whichever company was
   created first (in practice, before this phase, the only one). A companies table with zero rows
   makes this a safe no-op, since no business document could exist yet either (nothing can be
   created before the setup wizard creates the first company).
3. Backfill `auth_user_companies` for every existing user with no membership row (§4.5) —
   discovered necessary during implementation, not in the original plan.
4. Alter every backfilled column to `NOT NULL`.
5. Drop the old globally-unique number indexes (`BillNumber`, `VoucherNumber`, etc. — including
   `acc_journalentries`' raw-SQL `EntryNumber` index that was never in the EF model, per
   `JournalEntryConfiguration.cs`'s comment) and replace each with a `(CompanyId, Number)`
   composite (§7).
6. Add the FK to `companies` on all 8 tables, now that every row has a valid value.

Verified directly against a real Postgres 16 container (not just the InMemory test suite, which
can't exercise this at all): seeded pre-migration rows across all 8 tables under the old schema,
ran the migration, confirmed every row backfilled to the right company, confirmed two different
companies can reuse the same `BillNumber` while the same company rejects a duplicate (composite
unique index), and confirmed `Down()` reverses cleanly. See §8 for the read-isolation check.

**Follow-up — `20260921110159_UnifySisterConcernAccountsPhase1b`**: same recipe, applied to
`FiscalYear` and `CostCenter` once the gap in §3's "Resolved during implementation" was found.
Verified the same way: seeded a `FiscalYear` named `FY2026` and a `CostCenter` coded `HO` under
the old (pre-1b) schema, ran the migration, confirmed both backfilled correctly, then confirmed a
second company can independently create its own `FY2026`/`HO` without conflict.

---

## 6. Phasing

1. **Accounts — done.** `JournalEntry`, `Voucher`, `Bill`, `Invoice`, `Payment`, `Budget`,
   `BankAccount`, `NumberingSequence`, `FiscalYear`, `CostCenter`. This alone makes separate books
   possible (the actual point of "sister concern"), and the mechanism it forced into existence
   (§4) needs no further changes for the remaining phases — only new entities need to implement
   `ICompanyScoped`.
2. **Procurement — done.** `PurchaseRequisition` → `RequestForQuotation` → `VendorQuotation` →
   `PurchaseOrder` → `GoodsReceivedNote` chain, in `20260921110705_UnifySisterConcernProcurementPhase2.cs`.
   Same recipe as §5, and needed the exact same amount of handler-code change: zero — every
   number generator (`GeneratePRNumberAsync` etc., in each module's repository) counts rows
   through `_context.Set<T>()`, already scoped for free by the global filter, and none of the 5
   numbers are ever user-supplied (no duplicate-check code path existed to update either).
   `VendorQuotationConfiguration`'s `(RFQId, VendorId)` uniqueness needed no change — `RFQId`
   already pins a quotation to one company's RFQ, so it can't collide across companies on its
   own. Verified the same way as phase 1 (§8): seeded one row per anchor under the pre-migration
   schema, migrated, confirmed correct backfill, confirmed a second company can reuse the same
   `PR-2026-0001` while the same company rejects a duplicate.
3. **Projects — done.** `Project` scoped in `20260921111235_UnifySisterConcernProjectsPhase3.cs`
   — the module's single anchor; every other Projects entity (`WorkBreakdownStructure`,
   `BillOfQuantity`, `RABill`, `ProjectBudget`, `ProjectExpense`, and the rest of §3's child list)
   reaches it via `ProjectId` and needed no migration or code changes of its own. `ProjectCode`
   generation (`CreateProjectCommandHandler`, `_context.Set<Project>().CountAsync(ct)`) needed no
   change either, same zero-handler-change pattern as phases 1–2. Verified the same way: seeded a
   `PRJ-2026-0001` under the pre-migration schema, migrated, confirmed correct backfill, confirmed
   a second company can independently create its own `PRJ-2026-0001`.
4a. **Inventory — done.** `Warehouse`, `StockBalance`, `StockTransaction`, in
   `20260921111759_UnifySisterConcernInventoryPhase4a.cs`. **Deviates from the original plan**:
   `StockBalance`/`StockTransaction` were meant to inherit scoping through `WarehouseId` with no
   `CompanyId` of their own (like a Bill's line items) — found during implementation that this
   would have been wrong, because both have their own independent list/query handlers
   (`StockBalanceQueries.cs`, `StockTransactionQueries.cs`, both going through
   `BaseRepository<T>.BaseQuery() = _context.Set<T>().Where(!IsDeleted)`) that never join through
   `Warehouse` — inheriting scoping "through the parent" the way `BillLine` does would have left
   them completely unfiltered on their own list endpoints. Both entities carry their own
   `CompanyId` instead, same as `FiscalYear`/`CostCenter` in phase 1b. `Item`/`ItemVariant` (the
   catalogue) stay shared, as originally planned — only the stock a company actually holds is
   company-specific, not the catalogue it's drawn from. Verified the same way as prior phases:
   seeded one row per table (Warehouse `WH-01`, a stock balance, a stock transaction) under the
   pre-migration schema, migrated, confirmed correct backfill, confirmed a second company can
   reuse `WH-01` while the same company is rejected on a duplicate.
4b. **HR — not started.** `PayrollCycle` (+`PayrollEntry`), `ExpenseRequest`. Lower
   financial-statement impact than the rest of the spine.

Each remaining phase should land as its own PR: migration (nullable → backfill → NOT NULL, same
as §5) + entities implementing `ICompanyScoped` + a manual check that creating a document under
Company A is invisible from Company B's context (or reuse the throwaway-harness technique in §8).

---

## 7. Why `NumberingSequence` needed to move in the same phase as Bill/Invoice/Voucher

If `NumberingSequence` (`uOrgHub.Accounts/Models/Entities/NumberingSequence.cs`) had stayed
global, two sister concerns would race for the same invoice/bill/voucher number, producing gaps
or collisions in each company's own numbering — bad for an accounting document. It's
company-scoped in the same migration that scopes `Bill`/`Invoice`/`Voucher`
(`DocumentNumberingService` needed **no code change** to get this: its lookup/insert both go
through `_context.Set<NumberingSequence>()`, which the global filter and `AuditInterceptor`
already scope/stamp transparently).

---

## 8. How Phase 1 was verified

Beyond `dotnet build` (0 errors) and `dotnet test` (647/647 passing, unchanged — confirming
isolation is fully transparent when there's no company context, e.g. every existing test):

1. **Migration, against real Postgres 16**, twice — once against an empty database (fresh-install
   path) and once after manually seeding one row per affected table under the *pre*-migration
   schema (simulating a live install upgrading) — both applied cleanly, backfilled correctly, and
   `Down()` reversed cleanly.
2. **Composite uniqueness**, against real Postgres: two different companies inserting a `Bill`
   with the same `BillNumber` both succeed; the same company inserting a duplicate is rejected by
   `IX_acc_bills_CompanyId_BillNumber`.
3. **Read-time isolation, against real Postgres** (the InMemory test suite can't exercise this —
   `_companyId` is always null there, so the filter is a no-op by design, §4.3): a throwaway
   console harness constructed `AppDbContext` three times with a stub `ICurrentCompanyAccessor`
   returning Company A's id, Company B's id, and `null`, against a database seeded with one Bill
   per company. Company A's context returned only Company A's bill, Company B's only Company B's,
   and the `null` (system) context returned both — confirming the reflection-based filter
   mechanism (§4.3) actually filters at runtime, not just that it compiles.

All test/scratch data and the Docker container were torn down (`docker-compose down -v`)
afterward, leaving no residual state.
