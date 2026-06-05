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

        foreach (var selector in propertySelectors)
        {
            var property = selector.Body.ReplaceParameter(selector.Parameters[0], parameter);
            var containsCall = Expression.Call(property, containsMethod!, Expression.Constant(searchTerm));

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