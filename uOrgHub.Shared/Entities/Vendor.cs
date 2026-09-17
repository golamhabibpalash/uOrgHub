using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace uOrgHub.Shared.Entities;

public enum VendorType { Supplier, Contractor, Consultant, ServiceProvider }
public enum VendorStatus { Active, Inactive, Blacklisted }

/// <summary>
/// The single vendor record shared by Accounts (billing) and Procurement (sourcing) — these used to
/// be two disconnected tables (acc_vendors, proc_vendors) with no relationship between them, so the
/// vendor a PO was raised to and the vendor a Bill was raised against could silently be different
/// rows. Both modules' controllers/claims stay separate; only the underlying data is now one record.
/// </summary>
[Table("vendors")]
public class Vendor : BaseEntity
{
    [Required, MaxLength(30)] public string VendorCode { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(100)] public string? ContactPerson { get; set; }
    [MaxLength(200)] public string? Email { get; set; }
    [MaxLength(20)] public string? Phone { get; set; }
    [MaxLength(500)] public string? Address { get; set; }
    [MaxLength(50)] public string? TIN { get; set; }
    [MaxLength(50)] public string? BIN { get; set; }
    [MaxLength(100)] public string? TradeLicense { get; set; }
    public VendorType VendorType { get; set; } = VendorType.Supplier;
    public VendorStatus Status { get; set; } = VendorStatus.Active;
    [Column(TypeName = "decimal(18,2)")] public decimal CreditLimit { get; set; } = 0;
    public int PaymentTermDays { get; set; } = 30;
    [MaxLength(1000)] public string? Notes { get; set; }

    /// <summary>
    /// The GL account a Bill against this vendor posts Accounts Payable to. Null until someone sets
    /// it (e.g. a vendor first created from Procurement has no billing relationship yet) — Bill
    /// creation/approval guards against posting with this unset rather than the entity requiring it.
    /// Typed as a plain Guid (no navigation property): uOrgHub.Shared cannot reference
    /// uOrgHub.Accounts.Models.Entities.ChartOfAccount without creating a circular project
    /// dependency (Accounts already depends on Shared). The FK relationship itself is still
    /// configured — from the Accounts side, in VendorPayableAccountConfiguration.
    /// </summary>
    public Guid? PayableAccountId { get; set; }

    // No inverse navigation collections (Bills/Payments/PurchaseOrders/Quotations) for the same
    // reason: those entities live in uOrgHub.Accounts/uOrgHub.Procurement, which depend on Shared,
    // not vice versa. Each dependent entity's own Vendor FK is configured one-directionally
    // (HasOne(...).WithMany()) from its own module's configuration class instead.
}
