namespace uOrgHub.Inventory.Models.Enums;

public enum StockTransactionType
{
    GoodsReceived,
    GoodsIssued,
    Transfer,
    Adjustment,
    Return
}

public enum StockTransactionStatus
{
    Draft,
    Confirmed,
    Cancelled
}
