using Riok.Mapperly.Abstractions;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Models.Entities;

namespace uOrgHub.Procurement.Mappings;

[Mapper]
public partial class VendorMapper
{
    [MapperIgnoreTarget(nameof(Vendor.Id))]
    [MapperIgnoreTarget(nameof(Vendor.VendorCode))]
    [MapperIgnoreTarget(nameof(Vendor.CreatedAt))]
    [MapperIgnoreTarget(nameof(Vendor.CreatedBy))]
    [MapperIgnoreTarget(nameof(Vendor.UpdatedAt))]
    [MapperIgnoreTarget(nameof(Vendor.UpdatedBy))]
    [MapperIgnoreTarget(nameof(Vendor.IsDeleted))]
    [MapperIgnoreTarget(nameof(Vendor.DeletedAt))]
    [MapperIgnoreTarget(nameof(Vendor.DeletedBy))]
    [MapperIgnoreTarget(nameof(Vendor.Status))]
    public partial Vendor ToEntity(CreateVendorDto dto);

    [MapperIgnoreTarget(nameof(Vendor.Id))]
    [MapperIgnoreTarget(nameof(Vendor.VendorCode))]
    [MapperIgnoreTarget(nameof(Vendor.CreatedAt))]
    [MapperIgnoreTarget(nameof(Vendor.CreatedBy))]
    [MapperIgnoreTarget(nameof(Vendor.UpdatedAt))]
    [MapperIgnoreTarget(nameof(Vendor.UpdatedBy))]
    [MapperIgnoreTarget(nameof(Vendor.IsDeleted))]
    [MapperIgnoreTarget(nameof(Vendor.DeletedAt))]
    [MapperIgnoreTarget(nameof(Vendor.DeletedBy))]
    public partial void UpdateEntity(UpdateVendorDto dto, Vendor entity);
}
