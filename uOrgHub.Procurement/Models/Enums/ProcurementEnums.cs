namespace uOrgHub.Procurement.Models.Enums;

public enum VendorType
{
    Supplier,
    Contractor,
    Consultant,
    ServiceProvider
}

public enum VendorStatus
{
    Active,
    Inactive,
    Blacklisted
}

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
