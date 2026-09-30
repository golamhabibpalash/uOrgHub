using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Services.Attachments;
using uOrgHub.Shared.Services.FileStorage;

namespace uOrgHub.API.Services;

/// <summary>
/// Wires up the cross-module attachment system. To let another module accept attachments, add its
/// target to <see cref="SeedTargets"/> with the parent record's View/Edit claims — nothing else
/// changes; endpoints, storage, validation and UI component are all shared.
/// </summary>
public static class AttachmentsRegistration
{
    public static IServiceCollection AddAttachments(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SecureFileStorageOptions>(configuration.GetSection(SecureFileStorageOptions.SectionName));
        services.AddScoped<ISecureFileStorage, LocalSecureFileStorage>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddSingleton<IAttachmentTargetRegistry>(_ => SeedTargets());
        return services;
    }

    private static AttachmentTargetRegistry SeedTargets()
    {
        var registry = new AttachmentTargetRegistry();

        registry.Register(new AttachmentTargetDefinition
        {
            EntityType = "Voucher",
            ViewClaim = Claims.Accounts.Vouchers.View,
            EditClaim = Claims.Accounts.Vouchers.Edit,
        });

        registry.Register(new AttachmentTargetDefinition
        {
            EntityType = "Bill",
            ViewClaim = Claims.Accounts.Bills.View,
            EditClaim = Claims.Accounts.Bills.Edit,
            AdditionalEditClaims = [Claims.Accounts.Bills.Create],
        });

        // Payments have no edit screen — whoever records the payment attaches its receipt.
        registry.Register(new AttachmentTargetDefinition
        {
            EntityType = "Payment",
            ViewClaim = Claims.Accounts.Payments.View,
            EditClaim = Claims.Accounts.Payments.Edit,
            AdditionalEditClaims = [Claims.Accounts.Payments.Create],
        });

        // Register further targets here, e.g. invoices, employees, purchase orders.

        return registry;
    }
}
