using MediatR;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features._Common;
using uOrgHub.Procurement.Mappings;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Procurement.Repositories;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Procurement.Features.Vendors.Commands;

public record CreateVendorCommand(CreateVendorDto Dto) : ICommand<VendorResponseDto>;
public record UpdateVendorCommand(Guid Id, UpdateVendorDto Dto) : ICommand<VendorResponseDto>;
public record DeleteVendorCommand(Guid Id) : ICommand<Unit>;

public class CreateVendorCommandHandler : IRequestHandler<CreateVendorCommand, VendorResponseDto>
{
    private readonly IVendorRepository _repo;
    private readonly VendorMapper _mapper = new();

    public CreateVendorCommandHandler(IVendorRepository repo) => _repo = repo;

    public async Task<VendorResponseDto> Handle(CreateVendorCommand request, CancellationToken ct)
    {
        var entity = _mapper.ToEntity(request.Dto);
        entity.VendorCode = await _repo.GenerateVendorCodeAsync();
        entity.Status = VendorStatus.Active;
        entity.CreatedAt = DateTime.UtcNow;
        var created = await _repo.CreateAsync(entity);
        return BuildDto(created);
    }

    private static VendorResponseDto BuildDto(Models.Entities.Vendor v) => new()
    {
        Id = v.Id, VendorCode = v.VendorCode, CompanyName = v.CompanyName,
        ContactPerson = v.ContactPerson, Email = v.Email, Phone = v.Phone,
        Address = v.Address, TradeLicense = v.TradeLicense, TIN = v.TIN, BIN = v.BIN,
        VendorType = v.VendorType, Status = v.Status,
        CreditLimit = v.CreditLimit, PaymentTermDays = v.PaymentTermDays,
        Notes = v.Notes, CreatedAt = v.CreatedAt
    };
}

public class UpdateVendorCommandHandler : IRequestHandler<UpdateVendorCommand, VendorResponseDto>
{
    private readonly IVendorRepository _repo;
    private readonly VendorMapper _mapper = new();

    public UpdateVendorCommandHandler(IVendorRepository repo) => _repo = repo;

    public async Task<VendorResponseDto> Handle(UpdateVendorCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Models.Entities.Vendor), request.Id);

        _mapper.UpdateEntity(request.Dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        var updated = await _repo.UpdateAsync(entity);
        return new VendorResponseDto
        {
            Id = updated.Id, VendorCode = updated.VendorCode, CompanyName = updated.CompanyName,
            ContactPerson = updated.ContactPerson, Email = updated.Email, Phone = updated.Phone,
            Address = updated.Address, TradeLicense = updated.TradeLicense, TIN = updated.TIN, BIN = updated.BIN,
            VendorType = updated.VendorType, Status = updated.Status,
            CreditLimit = updated.CreditLimit, PaymentTermDays = updated.PaymentTermDays,
            Notes = updated.Notes, CreatedAt = updated.CreatedAt
        };
    }
}

public class DeleteVendorCommandHandler : IRequestHandler<DeleteVendorCommand, Unit>
{
    private readonly IVendorRepository _repo;
    public DeleteVendorCommandHandler(IVendorRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeleteVendorCommand request, CancellationToken ct)
    {
        if (!await _repo.ExistsAsync(request.Id))
            throw new NotFoundException(nameof(Models.Entities.Vendor), request.Id);
        await _repo.DeleteAsync(request.Id);
        return Unit.Value;
    }
}
