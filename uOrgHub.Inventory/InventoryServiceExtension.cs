using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using uOrgHub.Inventory.Repositories;

namespace uOrgHub.Inventory;

public static class InventoryServiceExtension
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(InventoryServiceExtension).Assembly));
        services.AddValidatorsFromAssembly(typeof(InventoryServiceExtension).Assembly);

        services.AddScoped<IInventoryTypeRepository, InventoryTypeRepository>();
        services.AddScoped<IInventoryCategoryRepository, InventoryCategoryRepository>();
        services.AddScoped<IUnitOfMeasureRepository, UnitOfMeasureRepository>();
        services.AddScoped<IAttributeDefinitionRepository, AttributeDefinitionRepository>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IItemVariantRepository, ItemVariantRepository>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IStockBalanceRepository, StockBalanceRepository>();
        services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();

        return services;
    }
}
