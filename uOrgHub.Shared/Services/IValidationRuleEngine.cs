using FluentValidation;
using FluentValidation.Results;

namespace uOrgHub.Shared.Services;

public interface IValidationRuleEngine
{
    Task ApplyRulesAsync<T>(string entityType, T dto, AbstractValidator<T> validator, CancellationToken ct = default);
    Task<List<ValidationFailure>> ValidateAsync<T>(string entityType, T instance, CancellationToken ct = default);
}
