using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.AP;
using uOrgHub.Accounts.Features._Common;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Accounts.Features.AP;

public record GetVendorsQuery(PaginationRequest Request) : IQuery<PagedResult<VendorResponseDto>>;
public record GetVendorByIdQuery(Guid Id) : IQuery<VendorResponseDto>;
public record CreateVendorCommand(CreateVendorDto Dto) : ICommand<VendorResponseDto>;
public record UpdateVendorCommand(Guid Id, UpdateVendorDto Dto) : ICommand<VendorResponseDto>;
public record DeleteVendorCommand(Guid Id) : ICommand<Unit>;

public class GetVendorsQueryHandler : IRequestHandler<GetVendorsQuery, PagedResult<VendorResponseDto>>
{
    private readonly AppDbContext _context;
    public GetVendorsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<VendorResponseDto>> Handle(GetVendorsQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.Vendor>()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.Name.Contains(request.Request.Search)
                || x.VendorCode.Contains(request.Request.Search)
                || (x.Email != null && x.Email.Contains(request.Request.Search)));

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.Name)
            : query.OrderBy(x => x.Name);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<VendorResponseDto>
        {
            Items = items.Select(VendorMappingHelper.ToDto).ToList(),
            TotalCount = totalCount,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetVendorByIdQueryHandler : IRequestHandler<GetVendorByIdQuery, VendorResponseDto>
{
    private readonly AppDbContext _context;
    public GetVendorByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<VendorResponseDto> Handle(GetVendorByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Set<Models.Entities.Vendor>()
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Vendor), request.Id);

        return VendorMappingHelper.ToDto(e);
    }
}

public class CreateVendorCommandHandler : IRequestHandler<CreateVendorCommand, VendorResponseDto>
{
    private readonly AppDbContext _context;
    public CreateVendorCommandHandler(AppDbContext context) => _context = context;

    public async Task<VendorResponseDto> Handle(CreateVendorCommand request, CancellationToken ct)
    {
        if (await _context.Set<Models.Entities.Vendor>().AnyAsync(x => x.VendorCode == request.Dto.VendorCode && !x.IsDeleted, ct))
            throw new AppException($"Vendor code '{request.Dto.VendorCode}' already exists.");

        var entity = new Models.Entities.Vendor
        {
            VendorCode = request.Dto.VendorCode,
            Name = request.Dto.Name,
            ContactPerson = request.Dto.ContactPerson,
            Email = request.Dto.Email,
            Phone = request.Dto.Phone,
            Address = request.Dto.Address,
            TIN = request.Dto.TIN,
            BIN = request.Dto.BIN,
            PaymentTermsDays = request.Dto.PaymentTermsDays,
            PayableAccountId = request.Dto.PayableAccountId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<Models.Entities.Vendor>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return VendorMappingHelper.ToDto(entity);
    }
}

public class UpdateVendorCommandHandler : IRequestHandler<UpdateVendorCommand, VendorResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateVendorCommandHandler(AppDbContext context) => _context = context;

    public async Task<VendorResponseDto> Handle(UpdateVendorCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.Vendor>()
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Vendor), request.Id);

        entity.Name = request.Dto.Name;
        entity.ContactPerson = request.Dto.ContactPerson;
        entity.Email = request.Dto.Email;
        entity.Phone = request.Dto.Phone;
        entity.Address = request.Dto.Address;
        entity.TIN = request.Dto.TIN;
        entity.BIN = request.Dto.BIN;
        entity.PaymentTermsDays = request.Dto.PaymentTermsDays;
        entity.IsActive = request.Dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return VendorMappingHelper.ToDto(entity);
    }
}

public class DeleteVendorCommandHandler : IRequestHandler<DeleteVendorCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteVendorCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteVendorCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.Vendor>()
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Vendor), request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

file static class VendorMappingHelper
{
    public static VendorResponseDto ToDto(Models.Entities.Vendor e) => new()
    {
        Id = e.Id,
        VendorCode = e.VendorCode,
        Name = e.Name,
        ContactPerson = e.ContactPerson,
        Email = e.Email,
        Phone = e.Phone,
        Address = e.Address,
        TIN = e.TIN,
        BIN = e.BIN,
        PaymentTermsDays = e.PaymentTermsDays,
        IsActive = e.IsActive,
        PayableAccountId = e.PayableAccountId
    };
}
