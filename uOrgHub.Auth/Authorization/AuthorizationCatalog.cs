using System.Reflection;

namespace uOrgHub.Auth.Authorization;

public record ClaimDefinition(string Name, string Module, string Category, string Description);
public record RoleDefinition(string Name, string Description, IReadOnlyList<string> Claims);

public static class AuthorizationCatalog
{
    public static IReadOnlyList<ClaimDefinition> AllClaims { get; } = BuildClaims();
    public static IReadOnlyList<RoleDefinition> AllRoles { get; } = BuildRoles();

    private static IReadOnlyList<ClaimDefinition> BuildClaims()
    {
        var list = new List<ClaimDefinition>();

        foreach (var module in typeof(Claims).GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
        {
            var moduleName = module.Name;
            if (module.IsClass && module.IsAbstract && module.IsSealed && module.GetFields(BindingFlags.Public | BindingFlags.Static).Any())
            {
                foreach (var field in module.GetFields(BindingFlags.Public | BindingFlags.Static))
                    list.Add(new ClaimDefinition((string)field.GetValue(null)!, moduleName, moduleName, Describe(field.GetValue(null)!.ToString()!)));
            }

            foreach (var entity in module.GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
            {
                var category = entity.Name.TrimEnd('_');
                foreach (var field in entity.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    var value = (string)field.GetValue(null)!;
                    list.Add(new ClaimDefinition(value, moduleName, category, Describe(value)));
                }
            }
        }

        return list;
    }

    private static string Describe(string claim)
    {
        var parts = claim.Split('.');
        var action = parts[^1];
        var subject = parts.Length >= 3 ? parts[^2].TrimEnd('_') : parts[0];
        return action switch
        {
            "View" => $"View {subject}",
            "Create" => $"Create {subject}",
            "Edit" => $"Edit {subject}",
            "Delete" => $"Delete {subject}",
            "Approve" => $"Approve {subject}",
            "Post" => $"Post {subject}",
            "Process" => $"Process {subject}",
            "AssignRoles" => $"Assign roles to {subject}",
            "Print" => $"Print {subject}",
            _ => claim,
        };
    }

    private static IReadOnlyList<RoleDefinition> BuildRoles()
    {
        var all = AllClaims.Select(c => c.Name).ToList();
        var hr = all.Where(c => c.StartsWith("HR.")).ToList();
        var accounts = all.Where(c => c.StartsWith("Accounts.")).ToList();
        var inventory = all.Where(c => c.StartsWith("Inventory.")).ToList();
        var procurement = all.Where(c => c.StartsWith("Procurement.")).ToList();
        var projects = all.Where(c => c.StartsWith("Projects.")).ToList();
        var self = all.Where(c => c.StartsWith("Self.")).ToList();

        var salesClaims = new[]
        {
            Claims.Accounts.Customers.View, Claims.Accounts.Customers.Create, Claims.Accounts.Customers.Edit,
            Claims.Accounts.Invoices.View, Claims.Accounts.Invoices.Create, Claims.Accounts.Invoices.Edit,
            Claims.Accounts.Payments.View,
            Claims.Inventory.Items.View,
            Claims.Inventory.StockBalances.View,
        };

        return new List<RoleDefinition>
        {
            new(Roles.Admin, "Full system access. Implicitly granted every claim.", Array.Empty<string>()),
            new(Roles.HRManager, "Manages everything in the HR module.", hr.Concat(self).ToList()),
            new(Roles.Accountant, "Manages everything in the Accounts module.", accounts.Concat(self).ToList()),
            new(Roles.InventoryManager, "Manages everything in the Inventory module plus read on Procurement.",
                inventory.Concat(procurement.Where(c => c.EndsWith(".View")))
                         .Concat(self).ToList()),
            new(Roles.ProcurementOfficer, "Manages everything in the Procurement module plus read on Inventory.",
                procurement.Concat(inventory.Where(c => c.EndsWith(".View")))
                           .Concat(self).ToList()),
            new(Roles.ProjectManager, "Manages everything in the Projects module plus read on Inventory and HR people.",
                projects.Concat(inventory.Where(c => c.EndsWith(".View")))
                        .Concat(new[] { Claims.HR.Employees.View, Claims.HR.Departments.View, Claims.HR.Designations.View })
                        .Concat(self).ToList()),
            new(Roles.Sales, "Sells to customers; manages invoices and customer records.",
                salesClaims.Concat(self).ToList()),
            new(Roles.Employee, "Self-service only. Default role for every newly provisioned employee account.", self),
        };
    }
}
