using Microsoft.EntityFrameworkCore;
using uOrgHub.Shared.Data;
using uOrgHub.Settings.Models.Entities;

namespace uOrgHub.Settings.Seeders;

public static class SettingsSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await SeedValidationRulesAsync(context);
    }

    private static async Task SeedValidationRulesAsync(AppDbContext context)
    {
        if (await context.Set<ValidationRule>().AnyAsync()) return;

        var rules = new List<ValidationRule>
        {
            // Employee field validations
            new() { EntityType = "Employee", FieldName = "Phone", RuleType = "Phone", RuleValue = null, ErrorMessage = "Phone number has an invalid format.", Severity = "Error", IsEnabled = true, SortOrder = 1, CreatedAt = DateTime.UtcNow },
            new() { EntityType = "Employee", FieldName = "MobilePhone", RuleType = "Phone", RuleValue = null, ErrorMessage = "Mobile number has an invalid format.", Severity = "Error", IsEnabled = true, SortOrder = 2, CreatedAt = DateTime.UtcNow },
            new() { EntityType = "Employee", FieldName = "PersonalEmail", RuleType = "Email", RuleValue = null, ErrorMessage = "Personal email must be a valid email address.", Severity = "Error", IsEnabled = true, SortOrder = 3, CreatedAt = DateTime.UtcNow },
            new() { EntityType = "Employee", FieldName = "Email", RuleType = "Email", RuleValue = null, ErrorMessage = "Company email must be a valid email address.", Severity = "Error", IsEnabled = true, SortOrder = 4, CreatedAt = DateTime.UtcNow },
            new() { EntityType = "Employee", FieldName = "NationalId", RuleType = "Required", RuleValue = null, ErrorMessage = "National ID is required.", Severity = "Error", IsEnabled = true, SortOrder = 5, CreatedAt = DateTime.UtcNow },
            new() { EntityType = "Employee", FieldName = "FirstName", RuleType = "MinLength", RuleValue = "2", ErrorMessage = "First name must be at least 2 characters.", Severity = "Error", IsEnabled = true, SortOrder = 6, CreatedAt = DateTime.UtcNow },
            new() { EntityType = "Employee", FieldName = "FirstName", RuleType = "MaxLength", RuleValue = "100", ErrorMessage = "First name cannot exceed 100 characters.", Severity = "Error", IsEnabled = true, SortOrder = 7, CreatedAt = DateTime.UtcNow },
            new() { EntityType = "Employee", FieldName = "EmployeeCode", RuleType = "MaxLength", RuleValue = "20", ErrorMessage = "Employee code cannot exceed 20 characters.", Severity = "Error", IsEnabled = true, SortOrder = 8, CreatedAt = DateTime.UtcNow },

            // Department validations
            new() { EntityType = "Department", FieldName = "Name", RuleType = "Required", RuleValue = null, ErrorMessage = "Department name is required.", Severity = "Error", IsEnabled = true, SortOrder = 1, CreatedAt = DateTime.UtcNow },
            new() { EntityType = "Department", FieldName = "Name", RuleType = "MaxLength", RuleValue = "200", ErrorMessage = "Department name cannot exceed 200 characters.", Severity = "Error", IsEnabled = true, SortOrder = 2, CreatedAt = DateTime.UtcNow },
            new() { EntityType = "Department", FieldName = "Code", RuleType = "Required", RuleValue = null, ErrorMessage = "Department code is required.", Severity = "Error", IsEnabled = true, SortOrder = 3, CreatedAt = DateTime.UtcNow },
            new() { EntityType = "Department", FieldName = "Code", RuleType = "MaxLength", RuleValue = "20", ErrorMessage = "Department code cannot exceed 20 characters.", Severity = "Error", IsEnabled = true, SortOrder = 4, CreatedAt = DateTime.UtcNow },

            // User validations
            new() { EntityType = "User", FieldName = "Username", RuleType = "Required", RuleValue = null, ErrorMessage = "Username is required.", Severity = "Error", IsEnabled = true, SortOrder = 1, CreatedAt = DateTime.UtcNow },
            new() { EntityType = "User", FieldName = "Username", RuleType = "MinLength", RuleValue = "3", ErrorMessage = "Username must be at least 3 characters.", Severity = "Error", IsEnabled = true, SortOrder = 2, CreatedAt = DateTime.UtcNow },
            new() { EntityType = "User", FieldName = "Username", RuleType = "MaxLength", RuleValue = "50", ErrorMessage = "Username cannot exceed 50 characters.", Severity = "Error", IsEnabled = true, SortOrder = 3, CreatedAt = DateTime.UtcNow },
            new() { EntityType = "User", FieldName = "Email", RuleType = "Email", RuleValue = null, ErrorMessage = "Email must be a valid email address.", Severity = "Error", IsEnabled = true, SortOrder = 4, CreatedAt = DateTime.UtcNow },
            new() { EntityType = "User", FieldName = "Email", RuleType = "MaxLength", RuleValue = "200", ErrorMessage = "Email cannot exceed 200 characters.", Severity = "Error", IsEnabled = true, SortOrder = 5, CreatedAt = DateTime.UtcNow },
        };

        context.Set<ValidationRule>().AddRange(rules);
        await context.SaveChangesAsync();
    }
}
