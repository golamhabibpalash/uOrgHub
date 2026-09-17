using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using uOrgHub.Procurement.Repositories;
using uOrgHub.Shared.Behaviors;

namespace uOrgHub.Procurement;

public static class ProcurementServiceExtension
{
    public static IServiceCollection AddProcurementModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ProcurementServiceExtension).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddValidatorsFromAssembly(typeof(ProcurementServiceExtension).Assembly);

        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<IPurchaseRequisitionRepository, PurchaseRequisitionRepository>();
        services.AddScoped<IRequestForQuotationRepository, RequestForQuotationRepository>();
        services.AddScoped<IVendorQuotationRepository, VendorQuotationRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IGoodsReceivedNoteRepository, GoodsReceivedNoteRepository>();

        return services;
    }
}
