using System.Linq.Expressions;
using System.Reflection;

namespace uOrgHub.Shared.Extensions;

public static class QueryableSortingExtensions
{
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string? sortBy, bool descending, Dictionary<string, string>? propertyMappings = null)
        => BuildPrimaryOrder(query, sortBy, descending, propertyMappings) ?? query;

    /// <summary>
    /// Sorts by <paramref name="sortBy"/> and settles ties with <paramref name="tieBreak"/>.
    ///
    /// Paging needs a total order. When the primary key of the sort is not unique — a date, a
    /// status — the database is free to return tied rows in any order it likes, and it need not
    /// pick the same one for page 2 as it did for page 1, so rows can repeat or vanish as the
    /// user pages. A unique tie-break removes that freedom.
    ///
    /// This exists as its own method because the obvious composition does not work: chaining
    /// <c>.OrderBy(tie).ApplySorting(...)</c> makes the later OrderBy *replace* the earlier one
    /// rather than demote it, and the tie-break is silently dropped from the generated SQL. The
    /// tie-break has to be attached as a ThenBy on the ordered query, which is what happens here.
    /// </summary>
    public static IQueryable<T> ApplySorting<T, TTieBreak>(
        this IQueryable<T> query,
        string? sortBy,
        bool descending,
        Expression<Func<T, TTieBreak>> tieBreak,
        bool tieBreakDescending = false,
        Dictionary<string, string>? propertyMappings = null)
    {
        var ordered = BuildPrimaryOrder(query, sortBy, descending, propertyMappings);

        // No usable primary sort — the tie-break becomes the whole ordering, which still leaves
        // the query totally ordered and so still safe to page.
        if (ordered is null)
            return tieBreakDescending ? query.OrderByDescending(tieBreak) : query.OrderBy(tieBreak);

        return tieBreakDescending ? ordered.ThenByDescending(tieBreak) : ordered.ThenBy(tieBreak);
    }

    /// <summary>
    /// The OrderBy half, shared by both overloads. Returns null when there is nothing to sort by —
    /// no sort requested, or a field that is not a property — so callers can decide the fallback
    /// rather than getting an unordered query that only looks ordered.
    /// </summary>
    private static IOrderedQueryable<T>? BuildPrimaryOrder<T>(
        IQueryable<T> query, string? sortBy, bool descending, Dictionary<string, string>? propertyMappings)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return null;

        if (propertyMappings?.TryGetValue(sortBy, out var mapped) == true)
            sortBy = mapped;

        var parameter = Expression.Parameter(typeof(T), "e");
        if (!TryBuildPropertyAccess(parameter, sortBy, out var property))
            return null; // Unknown sort field — ignore rather than throwing a 500.

        var lambda = Expression.Lambda(property!, parameter);

        var methodName = descending ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(
            typeof(Queryable), methodName,
            [typeof(T), property!.Type],
            query.Expression, Expression.Quote(lambda));

        // Safe cast: the expression just built is an OrderBy/OrderByDescending call, so the
        // provider's queryable for it is genuinely ordered and ThenBy may be attached.
        return (IOrderedQueryable<T>)query.Provider.CreateQuery<T>(resultExpression);
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
            // An enum column filtered by name ("Posted") or by its numeric value. Unwrapped first
            // so a nullable enum takes the same path as a plain one.
            var enumType = Nullable.GetUnderlyingType(targetType) is { IsEnum: true } inner
                ? inner
                : targetType.IsEnum ? targetType : null;

            object convertedValue;
            try
            {
                if (enumType is not null)
                    convertedValue = Enum.Parse(enumType, value, ignoreCase: true);
                else if (targetType == typeof(Guid) || targetType == typeof(Guid?))
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
            // Enum.Parse and Guid.Parse signal a bad value with ArgumentException rather than
            // FormatException, so both are caught — an unparseable filter must never reach the
            // Expression.Constant below, where a type mismatch would surface as a 500.
            catch (Exception ex) when (ex is FormatException or ArgumentException or OverflowException)
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
