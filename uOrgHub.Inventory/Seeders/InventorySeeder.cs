using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Inventory.Models.Enums;
using uOrgHub.Shared.Data;

namespace uOrgHub.Inventory.Seeders;

public static class InventorySeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await SeedInventoryTypesAsync(context);
        await SeedUnitsOfMeasureAsync(context);
        await SeedInventoryCategoriesAsync(context);
        await SeedAttributeDefinitionsAsync(context);
        await SeedWarehousesAsync(context);
    }

    private static async Task SeedInventoryTypesAsync(AppDbContext context)
    {
        if (await context.Set<InventoryType>().AnyAsync()) return;

        var types = new[]
        {
            new InventoryType { Name = "Finished Goods", Code = "FG", Description = "Products ready for sale", IsActive = true, CreatedAt = DateTime.UtcNow },
            new InventoryType { Name = "Raw Materials", Code = "RM", Description = "Materials used in production", IsActive = true, CreatedAt = DateTime.UtcNow },
            new InventoryType { Name = "Work In Progress", Code = "WIP", Description = "Items being manufactured", IsActive = true, CreatedAt = DateTime.UtcNow },
            new InventoryType { Name = "Consumables", Code = "CON", Description = "Items consumed during operations", IsActive = true, CreatedAt = DateTime.UtcNow },
            new InventoryType { Name = "Spare Parts", Code = "SP", Description = "Replacement parts for machinery", IsActive = true, CreatedAt = DateTime.UtcNow },
        };
        context.Set<InventoryType>().AddRange(types);
        await context.SaveChangesAsync();
    }

    private static async Task SeedUnitsOfMeasureAsync(AppDbContext context)
    {
        if (await context.Set<UnitOfMeasure>().AnyAsync()) return;

        var units = new[]
        {
            new UnitOfMeasure { Name = "Piece", Abbreviation = "PC", IsActive = true, CreatedAt = DateTime.UtcNow },
            new UnitOfMeasure { Name = "Kilogram", Abbreviation = "KG", IsActive = true, CreatedAt = DateTime.UtcNow },
            new UnitOfMeasure { Name = "Gram", Abbreviation = "G", IsActive = true, CreatedAt = DateTime.UtcNow },
            new UnitOfMeasure { Name = "Litre", Abbreviation = "L", IsActive = true, CreatedAt = DateTime.UtcNow },
            new UnitOfMeasure { Name = "Millilitre", Abbreviation = "ML", IsActive = true, CreatedAt = DateTime.UtcNow },
            new UnitOfMeasure { Name = "Metre", Abbreviation = "M", IsActive = true, CreatedAt = DateTime.UtcNow },
            new UnitOfMeasure { Name = "Centimetre", Abbreviation = "CM", IsActive = true, CreatedAt = DateTime.UtcNow },
            new UnitOfMeasure { Name = "Box", Abbreviation = "BOX", IsActive = true, CreatedAt = DateTime.UtcNow },
            new UnitOfMeasure { Name = "Carton", Abbreviation = "CTN", IsActive = true, CreatedAt = DateTime.UtcNow },
            new UnitOfMeasure { Name = "Dozen", Abbreviation = "DOZ", IsActive = true, CreatedAt = DateTime.UtcNow },
        };
        context.Set<UnitOfMeasure>().AddRange(units);
        await context.SaveChangesAsync();
    }

    private static async Task SeedInventoryCategoriesAsync(AppDbContext context)
    {
        if (await context.Set<InventoryCategory>().AnyAsync()) return;

        var fgType = await context.Set<InventoryType>().FirstOrDefaultAsync(x => x.Code == "FG");
        var rmType = await context.Set<InventoryType>().FirstOrDefaultAsync(x => x.Code == "RM");
        if (fgType == null || rmType == null) return;

        var categories = new[]
        {
            new InventoryCategory { Name = "Electronics", Code = "ELEC", TypeId = fgType.Id, Description = "Electronic products", IsActive = true, CreatedAt = DateTime.UtcNow },
            new InventoryCategory { Name = "Clothing", Code = "CLTH", TypeId = fgType.Id, Description = "Apparel and accessories", IsActive = true, CreatedAt = DateTime.UtcNow },
            new InventoryCategory { Name = "Food & Beverage", Code = "FB", TypeId = fgType.Id, Description = "Food and drink products", IsActive = true, CreatedAt = DateTime.UtcNow },
            new InventoryCategory { Name = "Chemicals", Code = "CHEM", TypeId = rmType.Id, Description = "Chemical raw materials", IsActive = true, CreatedAt = DateTime.UtcNow },
            new InventoryCategory { Name = "Metals", Code = "MET", TypeId = rmType.Id, Description = "Metal raw materials", IsActive = true, CreatedAt = DateTime.UtcNow },
        };
        context.Set<InventoryCategory>().AddRange(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAttributeDefinitionsAsync(AppDbContext context)
    {
        if (await context.Set<AttributeDefinition>().AnyAsync()) return;

        var attributes = new[]
        {
            new AttributeDefinition { Name = "Color", DataType = AttributeDataType.List, IsRequired = false, PredefinedValues = "Red,Blue,Green,Black,White,Yellow,Orange,Purple,Pink,Brown", IsActive = true, CreatedAt = DateTime.UtcNow },
            new AttributeDefinition { Name = "Size", DataType = AttributeDataType.List, IsRequired = false, PredefinedValues = "XS,S,M,L,XL,XXL,XXXL", IsActive = true, CreatedAt = DateTime.UtcNow },
            new AttributeDefinition { Name = "Material", DataType = AttributeDataType.Text, IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
            new AttributeDefinition { Name = "Weight (kg)", DataType = AttributeDataType.Number, IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
            new AttributeDefinition { Name = "Storage Capacity", DataType = AttributeDataType.List, IsRequired = false, PredefinedValues = "64GB,128GB,256GB,512GB,1TB,2TB", IsActive = true, CreatedAt = DateTime.UtcNow },
            new AttributeDefinition { Name = "Voltage", DataType = AttributeDataType.List, IsRequired = false, PredefinedValues = "110V,220V,240V", IsActive = true, CreatedAt = DateTime.UtcNow },
            new AttributeDefinition { Name = "Fragile", DataType = AttributeDataType.Boolean, IsRequired = false, IsActive = true, CreatedAt = DateTime.UtcNow },
        };
        context.Set<AttributeDefinition>().AddRange(attributes);
        await context.SaveChangesAsync();
    }

    private static async Task SeedWarehousesAsync(AppDbContext context)
    {
        if (await context.Set<Warehouse>().AnyAsync()) return;

        var warehouses = new[]
        {
            new Warehouse { Name = "Main Warehouse", Code = "WH-MAIN", Location = "Building A, Ground Floor", ContactPerson = "John Doe", ContactPhone = "+1234567890", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Warehouse { Name = "Secondary Warehouse", Code = "WH-SEC", Location = "Building B, Floor 2", ContactPerson = "Jane Smith", ContactPhone = "+0987654321", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Warehouse { Name = "Returns Warehouse", Code = "WH-RET", Location = "Building C, Ground Floor", IsActive = true, CreatedAt = DateTime.UtcNow },
        };
        context.Set<Warehouse>().AddRange(warehouses);
        await context.SaveChangesAsync();
    }
}
