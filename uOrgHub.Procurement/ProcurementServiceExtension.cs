using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using uOrgHub.Procurement.Repositories;

namespace uOrgHub.Procurement;

public static class ProcurementServiceExtension
{
    public static IServiceCollection AddProcurementModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProcurementServiceExtension).Assembly));
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
