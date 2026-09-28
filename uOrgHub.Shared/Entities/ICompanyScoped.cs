namespace uOrgHub.Shared.Entities;

/// <summary>
/// Marks an entity as belonging to one sister concern (see SISTER_CONCERN_PLAN.md). AppDbContext
/// discovers every entity implementing this interface by reflection and applies a global query
/// filter scoping it to the caller's active company — no per-entity wiring needed beyond adding
/// this interface, a CompanyId column, and the FK/index in the entity's own configuration.
/// Not folded into BaseEntity: several company-scoped entities are children reached only through
/// a scoped parent (e.g. JournalEntryLine via JournalEntry) and must not carry their own column,
/// and at least one (NumberingSequence) does not inherit BaseEntity at all.
/// </summary>
public interface ICompanyScoped
{
    Guid CompanyId { get; set; }
}
