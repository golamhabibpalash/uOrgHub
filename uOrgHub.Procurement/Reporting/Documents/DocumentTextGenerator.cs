using System.Text;

namespace uOrgHub.Procurement.Reporting.Documents;

/// <summary>
/// Reusable token names replaced inside document templates (and by the frontend editor hint), so
/// future documents (Purchase Order, GRN, ...) can share the same placeholder vocabulary.
/// </summary>
public static class DocumentTokens
{
    public const string CompanyName = "{{CompanyName}}";
    public const string CompanyAddress = "{{CompanyAddress}}";
    public const string CompanyPhone = "{{CompanyPhone}}";
    public const string CompanyEmail = "{{CompanyEmail}}";
    public const string PRNumber = "{{PRNumber}}";
    public const string PRDate = "{{RequisitionDate}}";
    public const string Department = "{{Department}}";
    public const string RequesterName = "{{RequesterName}}";
    public const string RequiredDate = "{{RequiredDate}}";
    public const string Purpose = "{{Purpose}}";
    public const string Items = "{{Items}}";
    public const string TotalAmount = "{{TotalAmount}}";
    public const string RFQNumber = "{{RFQNumber}}";
    public const string RFQDate = "{{RFQDate}}";
    public const string ClosingDate = "{{ClosingDate}}";
    public const string Title = "{{Title}}";
    public const string ListedItems = "{{ListedItems}}";

    public static readonly IReadOnlyList<string> All = new[]
    {
        CompanyName, CompanyAddress, CompanyPhone, CompanyEmail,
        PRNumber, PRDate, Department, RequesterName, RequiredDate, Purpose, Items, TotalAmount,
        RFQNumber, RFQDate, ClosingDate, Title, ListedItems
    };
}

/// <summary>
/// Builds the contextual application body for a procurement document from the underlying data, so
/// the wording reflects the actual request (department, purpose, item count, quantities, dates)
/// instead of a single generic paragraph. Data stays structured; only the generated text is
/// editable by the user.
/// </summary>
public static class DocumentTextGenerator
{
    public static string ResolvePlaceholders(string? text, IReadOnlyDictionary<string, string> tokens)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var result = new StringBuilder(text);
        foreach (var kvp in tokens)
            result.Replace(kvp.Key, kvp.Value);
        return result.ToString();
    }

    public static string GeneratePRBody(PRDocumentSource source)
    {
        var sb = new StringBuilder();

        var requestDescribed = DescribeRequest(source);
        var purposeSentence = BuildPurposeSentence(source.Purpose);

        var opening = $"On behalf of {source.DepartmentName}, we respectfully request approval to procure {requestDescribed} "
                      + $"required for {purposeSentence.ToLowerInvariant().TrimEnd('.')}.";
        sb.AppendLine(opening);

        sb.AppendLine();
        var (has, verb) = DescribeRequestVerb(source);
        sb.Append($"{Capitalize(has)} identified as necessary for the smooth operation of {source.DepartmentName}.");
        if (!string.IsNullOrWhiteSpace(source.Purpose))
            sb.Append($" The requisition supports the following objective: {source.Purpose.Trim()}.");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(source.RequiredDateText))
        {
            sb.AppendLine();
            sb.AppendLine($"It is essential that {requestDescribed} {verb} available by {source.RequiredDateText} to avoid disruption to ongoing activities.");
        }

        sb.AppendLine();
        sb.AppendLine($"We kindly request that this purchase requisition (Ref. No. {source.PRNumber}) be reviewed and approved. A copy should be returned to {source.RequestedByName} at {source.DepartmentName} for record-keeping.");

        return sb.ToString().Trim();
    }

    private static string DescribeRequest(PRDocumentSource source)
    {
        if (source.Items.Count == 0) return $"the requested items";
        if (source.Items.Count == 1)
        {
            var it = source.Items[0];
            var uom = string.IsNullOrWhiteSpace(it.UOM) ? "" : $" {it.UOM.Trim()}";
            return $"{FormatQty(it.Quantity)}{uom} of \"{it.VariantName}\"";
        }
        return $"{source.Items.Count} line items (totalling {FormatQty(source.Items.Sum(i => i.Quantity))} units)";
    }

    private static (string has, string verb) DescribeRequestVerb(PRDocumentSource source)
    {
        if (source.Items.Count == 1)
        {
            var qty = source.Items[0].Quantity;
            return qty == 1 ? ("The requested item has", "is") : ($"The requested items have", "are");
        }
        return ("The requested items have", "are");
    }

    public static string GenerateRFQBody(RfqDocumentSource source)
    {
        var sb = new StringBuilder();

        var itemsList = FormatItemList(source.Items);

        sb.AppendLine("Dear Sir/Madam,");
        sb.AppendLine();
        sb.AppendLine($"We are pleased to request your quotation for the supply of the following items required by our organization:");
        sb.AppendLine();
        sb.AppendLine(itemsList);

        if (!string.IsNullOrWhiteSpace(source.ClosingDateText))
            sb.AppendLine($"Quotations must be submitted by {source.ClosingDateText}.");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(source.Description))
            sb.AppendLine($"Scope of requirement: {source.Description.Trim()}");

        sb.AppendLine();
        sb.AppendLine("We request that your quotation include the item price, delivery schedule, warranty/guarantee terms, and the validity period of your offer. Please quote in the company's standard currency and indicate any applicable tax, freight, and other charges separately.");
        sb.AppendLine();
        sb.AppendLine("We look forward to receiving your quotation. Please do not hesitate to contact us should you require any clarification regarding this request.");
        sb.AppendLine();
        sb.AppendLine("Thank you in advance for your prompt attention to this matter.");

        return sb.ToString().Trim();
    }

    private static string FormatItemList(IReadOnlyList<DocumentItemSource> items)
    {
        if (items.Count == 0) return "No items requested.";

        var sb = new StringBuilder();
        for (int i = 0; i < items.Count; i++)
        {
            var it = items[i];
            sb.Append($"{i + 1}. {it.VariantName}");
            if (!string.IsNullOrWhiteSpace(it.UOM)) sb.Append($" ({it.UOM.Trim()})");
            sb.Append($" — Quantity: {FormatQty(it.Quantity)}");
            if (!string.IsNullOrWhiteSpace(it.Notes)) sb.Append($". {it.Notes.Trim()}");
            if (i < items.Count - 1) sb.AppendLine();
        }
        return sb.ToString();
    }

    private static string BuildPurposeSentence(string? purpose)
    {
        var trimmed = purpose?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed)) return "our ongoing operations";
        return trimmed.EndsWith(".") ? trimmed : trimmed + ".";
    }

    private static string Capitalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        return char.ToUpperInvariant(value[0]) + value[1..];
    }

    private static string FormatQty(decimal qty)
    {
        return qty == (long)qty ? ((long)qty).ToString("N0") : qty.ToString("0.###");
    }
}

public record DocumentItemSource(
    string? VariantName,
    string? UOM,
    decimal Quantity,
    string? Notes);

public record PRDocumentSource(
    string PRNumber,
    string RequestedByName,
    string DepartmentName,
    string? Purpose,
    string? RequiredDateText,
    IReadOnlyList<DocumentItemSource> Items);

public record RfqDocumentSource(
    string? PRNumber,
    string Title,
    string? Description,
    string? ClosingDateText,
    IReadOnlyList<DocumentItemSource> Items);