using System.Linq.Expressions;

namespace uOrgHub.Shared.Export;

public class ExportColumn<T>
{
    public string Key { get; }
    public string Label { get; }
    public Func<T, object?> ValueSelector { get; }

    public ExportColumn(string key, string label, Expression<Func<T, object?>> valueSelector)
    {
        Key = key;
        Label = label;
        ValueSelector = valueSelector.Compile();
    }
}
