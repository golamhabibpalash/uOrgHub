using FluentValidation;
using FluentValidation.Results;
using MediatR;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Services;

namespace uOrgHub.Shared.Behaviors;

/// <summary>
/// MediatR pipeline that runs FluentValidation plus dynamic <see cref="IValidationRuleEngine"/> rules.
/// Validators in this repo target DTOs (e.g. <c>AbstractValidator&lt;CreatePRDto&gt;</c>), never the
/// command records that wrap them (e.g. <c>CreatePRCommand(CreatePRDto Dto)</c>), so resolving
/// <c>IValidator&lt;TRequest&gt;</c> alone finds nothing. This behavior additionally validates every
/// non-null request property whose runtime type has a registered validator — which covers the
/// universal <c>Dto</c> convention. Rule-engine checks run against the DTO with an entity name
/// derived from the DTO type (<c>CreateEmployeeDto</c> → <c>"Employee"</c>, matching seeded rules).
/// Wire it per module: <c>cfg.AddOpenBehavior(typeof(ValidationBehavior&lt;,&gt;))</c>.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly System.Reflection.MethodInfo ValidateRulesMethod =
        typeof(IValidationRuleEngine).GetMethod(nameof(IValidationRuleEngine.ValidateAsync))!;

    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly IServiceProvider _serviceProvider;
    private readonly IValidationRuleEngine? _ruleEngine;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        IServiceProvider serviceProvider,
        IValidationRuleEngine? ruleEngine = null)
    {
        _validators = validators;
        _serviceProvider = serviceProvider;
        _ruleEngine = ruleEngine;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var failures = new List<ValidationFailure>();

        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(new ValidationContext<TRequest>(request), ct);
            failures.AddRange(result.Errors);
        }

        // Commands wrap DTOs — validate each nested value that has a registered validator.
        foreach (var property in typeof(TRequest).GetProperties())
        {
            if (!property.CanRead || property.GetIndexParameters().Length != 0)
                continue;

            var value = property.GetValue(request);
            if (value is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(value.GetType());
            if (_serviceProvider.GetService(validatorType) is not IValidator validator)
                continue;

            var contextType = typeof(ValidationContext<>).MakeGenericType(value.GetType());
            var context = (IValidationContext)Activator.CreateInstance(contextType, value)!;
            var result = await validator.ValidateAsync(context, ct);
            failures.AddRange(result.Errors);

            if (_ruleEngine is not null)
                failures.AddRange(await ValidateRulesAsync(DeriveEntityType(value.GetType()), value, ct));
        }

        if (_ruleEngine is not null)
            failures.AddRange(await ValidateRulesAsync(DeriveEntityType(typeof(TRequest)), request, ct));

        var distinct = failures.Where(e => e != null).ToList();
        if (distinct.Count != 0)
            throw new Exceptions.ValidationException(distinct.Select(FormatFailure).ToList());

        return await next();
    }

    // IValidationRuleEngine.ValidateAsync is generic in the instance type (it reflects over
    // typeof(T) for property lookups), so it must be invoked with the runtime type, not object.
    private async Task<List<ValidationFailure>> ValidateRulesAsync(string entityType, object instance, CancellationToken ct)
    {
        var task = (Task<List<ValidationFailure>>)ValidateRulesMethod
            .MakeGenericMethod(instance.GetType())
            .Invoke(_ruleEngine, [entityType, instance, ct])!;
        return await task;
    }

    // Same "Property: message" shape the API uses for model-binding errors (Program.cs),
    // so clients see one consistent validation format.
    private static string FormatFailure(ValidationFailure failure) =>
        string.IsNullOrWhiteSpace(failure.PropertyName)
            ? failure.ErrorMessage
            : $"{failure.PropertyName}: {failure.ErrorMessage}";

    private static string DeriveEntityType(Type type)
    {
        var name = type.Name;
        foreach (var prefix in new[] { "Create", "Update", "Approve", "Reject", "Submit", "Delete" })
        {
            if (name.StartsWith(prefix) && name.Length > prefix.Length)
            {
                name = name[prefix.Length..];
                break;
            }
        }
        foreach (var suffix in new[] { "Dto", "Command", "Query" })
        {
            if (name.EndsWith(suffix) && name.Length > suffix.Length)
            {
                name = name[..^suffix.Length];
                break;
            }
        }
        return name;
    }
}
