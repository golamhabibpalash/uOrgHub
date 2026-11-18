using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Sms.Providers;

public interface ISmsProviderRegistry
{
    IReadOnlyList<SmsProviderDescriptor> Descriptors { get; }
    bool Exists(string key);
    ISmsProvider Get(string key);
}

/// <summary>Every registered <see cref="ISmsProvider"/>, looked up by key.</summary>
public sealed class SmsProviderRegistry : ISmsProviderRegistry
{
    private readonly Dictionary<string, ISmsProvider> _providers;

    public SmsProviderRegistry(IEnumerable<ISmsProvider> providers)
    {
        _providers = providers.ToDictionary(p => p.Descriptor.Key, StringComparer.OrdinalIgnoreCase);
        Descriptors = _providers.Values.Select(p => p.Descriptor).OrderBy(d => d.DisplayName).ToList();
    }

    public IReadOnlyList<SmsProviderDescriptor> Descriptors { get; }

    public bool Exists(string key) => _providers.ContainsKey(key);

    public ISmsProvider Get(string key)
        => _providers.TryGetValue(key, out var p) ? p
            : throw new AppException($"SMS provider '{key}' is not available in this installation.");
}
