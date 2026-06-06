using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using uOrgHub.Shared.Services;
using uOrgHub.Settings.Models.Entities;
using uOrgHub.Settings.Repositories;

namespace uOrgHub.Settings.Services;

public class ValidationRuleEngine : IValidationRuleEngine
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<string, List<ValidationRule>> _ruleCache = new();
    private static DateTime _cacheExpiry = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private static readonly object _cacheLock = new();

    public ValidationRuleEngine(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ApplyRulesAsync<T>(string entityType, T dto, AbstractValidator<T> validator, CancellationToken ct = default)
    {
        var rules = await GetRulesForEntityAsync(entityType);
        var entityRules = rules.Where(r => r.IsEnabled && !r.IsDeleted).ToList();

        foreach (var rule in entityRules)
        {
            var property = typeof(T).GetProperty(rule.FieldName);
            if (property is null) continue;

            switch (rule.RuleType.ToLower())
            {
                case "required":
                    validator.RuleFor(x => property.GetValue(x) as string)
                        .NotEmpty()
                        .WithMessage(rule.ErrorMessage ?? $"{rule.FieldName} is required.")
                        .When(_ => property.PropertyType == typeof(string));
                    break;

                case "email":
                    validator.RuleFor(x => property.GetValue(x) as string)
                        .EmailAddress()
                        .WithMessage(rule.ErrorMessage ?? $"{rule.FieldName} must be a valid email.")
                        .When(x => !string.IsNullOrWhiteSpace(property.GetValue(x) as string));
                    break;

                case "minlength":
                    if (int.TryParse(rule.RuleValue, out var minLen))
                        validator.RuleFor(x => property.GetValue(x) as string)
                            .MinimumLength(minLen)
                            .WithMessage(rule.ErrorMessage ?? $"{rule.FieldName} must be at least {minLen} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(property.GetValue(x) as string));
                    break;

                case "maxlength":
                    if (int.TryParse(rule.RuleValue, out var maxLen))
                        validator.RuleFor(x => property.GetValue(x) as string)
                            .MaximumLength(maxLen)
                            .WithMessage(rule.ErrorMessage ?? $"{rule.FieldName} cannot exceed {maxLen} characters.")
                            .When(x => property.GetValue(x) as string != null);
                    break;

                case "regex":
                    if (!string.IsNullOrWhiteSpace(rule.RuleValue))
                        validator.RuleFor(x => property.GetValue(x) as string)
                            .Matches(rule.RuleValue)
                            .WithMessage(rule.ErrorMessage ?? $"{rule.FieldName} has an invalid format.")
                            .When(x => !string.IsNullOrWhiteSpace(property.GetValue(x) as string));
                    break;

                case "phone":
                    validator.RuleFor(x => property.GetValue(x) as string)
                        .MaximumLength(20)
                        .Matches(@"^\+?[\d\s\-\(\)]{7,20}$")
                        .WithMessage(rule.ErrorMessage ?? $"{rule.FieldName} has an invalid phone format.")
                        .When(x => !string.IsNullOrWhiteSpace(property.GetValue(x) as string));
                    break;
            }
        }
    }

    public async Task<List<ValidationFailure>> ValidateAsync<T>(string entityType, T instance, CancellationToken ct = default)
    {
        var failures = new List<ValidationFailure>();
        var rules = await GetRulesForEntityAsync(entityType);
        var enabledRules = rules.Where(r => r.IsEnabled && !r.IsDeleted).ToList();

        foreach (var rule in enabledRules)
        {
            var property = typeof(T).GetProperty(rule.FieldName);
            if (property is null) continue;

            var value = property.GetValue(instance) as string;

            switch (rule.RuleType.ToLower())
            {
                case "required":
                    if (string.IsNullOrWhiteSpace(value))
                        failures.Add(new ValidationFailure(rule.FieldName, rule.ErrorMessage ?? $"{rule.FieldName} is required."));
                    break;

                case "email":
                    if (!string.IsNullOrWhiteSpace(value) && !value.Contains('@'))
                        failures.Add(new ValidationFailure(rule.FieldName, rule.ErrorMessage ?? $"{rule.FieldName} must be a valid email."));
                    break;

                case "minlength":
                    if (!string.IsNullOrWhiteSpace(value) && int.TryParse(rule.RuleValue, out var minLen) && value.Length < minLen)
                        failures.Add(new ValidationFailure(rule.FieldName, rule.ErrorMessage ?? $"{rule.FieldName} must be at least {minLen} characters."));
                    break;

                case "maxlength":
                    if (value != null && int.TryParse(rule.RuleValue, out var maxLen) && value.Length > maxLen)
                        failures.Add(new ValidationFailure(rule.FieldName, rule.ErrorMessage ?? $"{rule.FieldName} cannot exceed {maxLen} characters."));
                    break;

                case "regex":
                    if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(rule.RuleValue) && !Regex.IsMatch(value, rule.RuleValue))
                        failures.Add(new ValidationFailure(rule.FieldName, rule.ErrorMessage ?? $"{rule.FieldName} has an invalid format."));
                    break;

                case "phone":
                    if (!string.IsNullOrWhiteSpace(value) && !Regex.IsMatch(value, @"^\+?[\d\s\-\(\)]{7,20}$"))
                        failures.Add(new ValidationFailure(rule.FieldName, rule.ErrorMessage ?? $"{rule.FieldName} has an invalid phone format."));
                    break;
            }
        }

        return failures;
    }

    private async Task<List<ValidationRule>> GetRulesForEntityAsync(string entityType)
    {
        if (DateTime.UtcNow < _cacheExpiry && _ruleCache.TryGetValue(entityType, out var cached))
            return cached;

        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IValidationRuleRepository>();
        var rules = await repo.GetByEntityTypeAsync(entityType);

        lock (_cacheLock)
        {
            _ruleCache[entityType] = rules;
            _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);
        }

        return rules;
    }

    public static void InvalidateCache()
    {
        lock (_cacheLock)
        {
            _ruleCache.Clear();
            _cacheExpiry = DateTime.MinValue;
        }
    }
}
