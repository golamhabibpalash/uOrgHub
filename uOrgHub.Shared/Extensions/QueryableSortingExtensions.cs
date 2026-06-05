using System.Linq.Expressions;

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
        Expression property = sortBy.Split('.').Aggregate((Expression)parameter, Expression.Property);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = descending ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(
            typeof(Queryable), methodName,
            [typeof(T), property.Type],
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
            Expression property = key.Split('.').Aggregate((Expression)parameter, Expression.Property);
            var targetType = property.Type;

            object convertedValue;
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

            var constant = Expression.Constant(convertedValue, targetType);
            var equals = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);

            query = query.Where(lambda);
        }

        return query;
    }
}
