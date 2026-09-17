using Riok.Mapperly.Abstractions;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Procurement.Mappings;

// The DTOs keep their original "CompanyName" field name (Procurement's own historical shape, so the
// frontend needs no changes) even though the now-shared Vendor entity's field is "Name" — explicit
// MapProperty bridges the rename.
[Mapper]
public partial class VendorMapper
{
    [MapProperty(nameof(CreateVendorDto.CompanyName), nameof(Vendor.Name))]
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
    [MapperIgnoreTarget(nameof(Vendor.PayableAccountId))]
    public partial Vendor ToEntity(CreateVendorDto dto);

    [MapProperty(nameof(UpdateVendorDto.CompanyName), nameof(Vendor.Name))]
    [MapperIgnoreTarget(nameof(Vendor.Id))]
    [MapperIgnoreTarget(nameof(Vendor.VendorCode))]
    [MapperIgnoreTarget(nameof(Vendor.CreatedAt))]
    [MapperIgnoreTarget(nameof(Vendor.CreatedBy))]
    [MapperIgnoreTarget(nameof(Vendor.UpdatedAt))]
    [MapperIgnoreTarget(nameof(Vendor.UpdatedBy))]
    [MapperIgnoreTarget(nameof(Vendor.IsDeleted))]
    [MapperIgnoreTarget(nameof(Vendor.DeletedAt))]
    [MapperIgnoreTarget(nameof(Vendor.DeletedBy))]
    [MapperIgnoreTarget(nameof(Vendor.PayableAccountId))]
    public partial void UpdateEntity(UpdateVendorDto dto, Vendor entity);
}
