using System.Linq.Expressions;
using System.Reflection;

namespace uOrgHub.Shared.Extensions;

public static class QueryableSortingExtensions
{
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string? sortBy, bool descending, Dictionary<string, string>? propertyMappings = null)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query;

        if (propertyMappings?.TryGetValue(sortBy, out var mapped) == true)
            sortBy = mapped;

        var parameter = Expression.Parameter(typeof(T), "e");
        if (!TryBuildPropertyAccess(parameter, sortBy, out var property))
            return query; // Unknown sort field — ignore rather than throwing a 500.

        var lambda = Expression.Lambda(property!, parameter);

        var methodName = descending ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(
            typeof(Queryable), methodName,
            [typeof(T), property!.Type],
            query.Expression, Expression.Quote(lambda));

        return query.Provider.CreateQuery<T>(resultExpression);
    }

    public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, Dictionary<string, string>? filters)
    {
        if (filters == null || filters.Count == 0)
            return query;

        foreach (var (key, value) in filters)
        {
            if (string.IsNullOrWhiteSpace(value))
                continue;

            var parameter = Expression.Parameter(typeof(T), "e");
            if (!TryBuildPropertyAccess(parameter, key, out var property))
                continue; // Unknown filter field — skip it instead of throwing.

            var targetType = property!.Type;

            object convertedValue;
            try
            {
                if (targetType == typeof(Guid) || targetType == typeof(Guid?))
                    convertedValue = Guid.Parse(value);
                else if (targetType == typeof(int) || targetType == typeof(int?))
                    convertedValue = int.Parse(value);
                else if (targetType == typeof(bool) || targetType == typeof(bool?))
                    convertedValue = bool.Parse(value);
                else if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                    convertedValue = DateTime.Parse(value);
                else if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                    convertedValue = decimal.Parse(value);
                else
                    convertedValue = value;
            }
            catch (FormatException)
            {
                continue; // Bad filter value for the target type — skip rather than 500.
            }

            var constant = Expression.Constant(convertedValue, targetType);
            var equals = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);

            query = query.Where(lambda);
        }

        return query;
    }

    /// <summary>
    /// Resolves a (possibly nested, dot-separated) property path against <paramref name="parameter"/>,
    /// matching property names case-insensitively. Returns false if any segment is not a public property.
    /// </summary>
    private static bool TryBuildPropertyAccess(ParameterExpression parameter, string path, out Expression? access)
    {
        access = parameter;
        foreach (var segment in path.Split('.'))
        {
            var prop = access.Type.GetProperty(
                segment,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null)
            {
                access = null;
                return false;
            }
            access = Expression.Property(access, prop);
        }
        return true;
    }
}
