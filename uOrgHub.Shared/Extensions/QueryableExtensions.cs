using System.Linq.Expressions;

namespace uOrgHub.Shared.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> WhereSearch<T>(this IQueryable<T> query, string? searchTerm, params Expression<Func<T, string>>[] propertySelectors)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || propertySelectors.Length == 0)
            return query;

        var parameter = Expression.Parameter(typeof(T), "e");
        Expression? orExpression = null;
        var containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)]);
        var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes);

        // Both sides are lowered so the match is case-insensitive. PostgreSQL's LIKE is
        // case-sensitive, so without this a search for "jv-2026" would miss "JV-2026-0001" —
        // which is how entry numbers, codes and names are actually stored.
        var loweredTerm = Expression.Constant(searchTerm.ToLower());

        foreach (var selector in propertySelectors)
        {
            var property = selector.Body.ReplaceParameter(selector.Parameters[0], parameter);

            // A null column would make ToLower() throw in memory and yield NULL in SQL; coalescing
            // to empty keeps a nullable field (ReferenceNumber, say) a non-match rather than a fault.
            var safeProperty = Expression.Coalesce(property, Expression.Constant(string.Empty));
            var loweredProperty = Expression.Call(safeProperty, toLowerMethod!);
            var containsCall = Expression.Call(loweredProperty, containsMethod!, loweredTerm);

            orExpression = orExpression == null ? containsCall : Expression.OrElse(orExpression, containsCall);
        }

        var lambda = Expression.Lambda<Func<T, bool>>(orExpression!, parameter);
        return query.Where(lambda);
    }
}

internal static class ExpressionExtensions
{
    public static Expression ReplaceParameter(this Expression expression, ParameterExpression oldParameter, ParameterExpression newParameter)
        => new ParameterReplacer(oldParameter, newParameter).Visit(expression);

    private sealed class ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParameter ? newParameter : base.VisitParameter(node);
    }
}