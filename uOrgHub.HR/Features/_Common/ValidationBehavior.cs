using FluentValidation;
using MediatR;
using uOrgHub.Shared.Services;

namespace uOrgHub.HR.Features._Common;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly IValidationRuleEngine? _ruleEngine;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, IValidationRuleEngine? ruleEngine = null)
    {
        _validators = validators;
        _ruleEngine = ruleEngine;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(e => e != null)
            .ToList();

        if (_ruleEngine is not null)
        {
            var requestType = typeof(TRequest).Name;
            var suffixes = new[] { "Command", "Query", "Dto" };
            var entityType = requestType;
            foreach (var suffix in suffixes)
            {
                if (entityType.EndsWith(suffix))
                {
                    entityType = entityType[..^suffix.Length];
                    break;
                }
            }

            var dynamicFailures = await _ruleEngine.ValidateAsync(entityType, request, ct);
            failures.AddRange(dynamicFailures);
        }

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
