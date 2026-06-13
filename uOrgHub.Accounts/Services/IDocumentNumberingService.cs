namespace uOrgHub.Accounts.Services;

public interface IDocumentNumberingService
{
    Task<string> GenerateNextAsync(string documentType, string prefix, int? year = null, int? month = null);
}
