namespace uOrgHub.Procurement.Models.Enums;

// VendorType/VendorStatus moved to uOrgHub.Shared.Entities.Vendor — Vendor itself now lives in
// Shared (unified with Accounts' former vendor table), so its enums live alongside it.

public enum PRStatus
{
    Draft,
    Submitted,
    Approved,
    Rejected,
    Converted
}

public enum RFQStatus
{
    Draft,
    Sent,
    Closed,
    Cancelled
}

public enum QuotationStatus
{
    Received,
    Evaluated,
    Accepted,
    Rejected
}

public enum POStatus
{
    Draft,
    Sent,
    Confirmed,
    PartiallyReceived,
    FullyReceived,
    Cancelled
}

public enum GRNStatus
{
    Draft,
    Confirmed,
    Cancelled
}
