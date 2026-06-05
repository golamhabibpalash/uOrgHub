using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.AR;
using uOrgHub.Accounts.Features._Common;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Accounts.Features.AR;

public record GetCustomersQuery(PaginationRequest Request) : IQuery<PagedResult<CustomerResponseDto>>;
public record GetAllCustomersQuery(string? Search = null) : IQuery<List<CustomerResponseDto>>;
public record GetCustomerByIdQuery(Guid Id) : IQuery<CustomerResponseDto>;
public record CreateCustomerCommand(CreateCustomerDto Dto) : ICommand<CustomerResponseDto>;
public record UpdateCustomerCommand(Guid Id, UpdateCustomerDto Dto) : ICommand<CustomerResponseDto>;
public record DeleteCustomerCommand(Guid Id) : ICommand<Unit>;

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PagedResult<CustomerResponseDto>>
{
    private readonly AppDbContext _context;
    public GetCustomersQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<CustomerResponseDto>> Handle(GetCustomersQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.Customer>()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.Name.Contains(request.Request.Search)
                || x.CustomerCode.Contains(request.Request.Search)
                || (x.Email != null && x.Email.Contains(request.Request.Search)));

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.Name)
            : query.OrderBy(x => x.Name);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<CustomerResponseDto>
        {
            Items = items.Select(CustomerMappingHelper.ToDto).ToList(),
            TotalCount = totalCount,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, List<CustomerResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllCustomersQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<CustomerResponseDto>> Handle(GetAllCustomersQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.Customer>().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(x => x.Name.Contains(request.Search)
                || x.CustomerCode.Contains(request.Search)
                || (x.Email != null && x.Email.Contains(request.Search)));

        query = query.OrderBy(x => x.Name);
        var items = await query.ToListAsync(ct);
        return items.Select(CustomerMappingHelper.ToDto).ToList();
    }
}

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerResponseDto>
{
    private readonly AppDbContext _context;
    public GetCustomerByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<CustomerResponseDto> Handle(GetCustomerByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Set<Models.Entities.Customer>()
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Customer), request.Id);

        return CustomerMappingHelper.ToDto(e);
    }
}

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerResponseDto>
{
    private readonly AppDbContext _context;
    public CreateCustomerCommandHandler(AppDbContext context) => _context = context;

    public async Task<CustomerResponseDto> Handle(CreateCustomerCommand request, CancellationToken ct)
    {
        if (await _context.Set<Models.Entities.Customer>().AnyAsync(x => x.CustomerCode == request.Dto.CustomerCode && !x.IsDeleted, ct))
            throw new AppException($"Customer code '{request.Dto.CustomerCode}' already exists.");

        var entity = new Models.Entities.Customer
        {
            CustomerCode = request.Dto.CustomerCode,
            Name = request.Dto.Name,
            ContactPerson = request.Dto.ContactPerson,
            Email = request.Dto.Email,
            Phone = request.Dto.Phone,
            Address = request.Dto.Address,
            TIN = request.Dto.TIN,
            BIN = request.Dto.BIN,
            CreditLimit = request.Dto.CreditLimit,
            PaymentTermsDays = request.Dto.PaymentTermsDays,
            ReceivableAccountId = request.Dto.ReceivableAccountId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<Models.Entities.Customer>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return CustomerMappingHelper.ToDto(entity);
    }
}

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateCustomerCommandHandler(AppDbContext context) => _context = context;

    public async Task<CustomerResponseDto> Handle(UpdateCustomerCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.Customer>()
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Customer), request.Id);

        entity.Name = request.Dto.Name;
        entity.ContactPerson = request.Dto.ContactPerson;
        entity.Email = request.Dto.Email;
        entity.Phone = request.Dto.Phone;
        entity.Address = request.Dto.Address;
        entity.TIN = request.Dto.TIN;
        entity.BIN = request.Dto.BIN;
        entity.CreditLimit = request.Dto.CreditLimit;
        entity.PaymentTermsDays = request.Dto.PaymentTermsDays;
        entity.IsActive = request.Dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return CustomerMappingHelper.ToDto(entity);
    }
}

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteCustomerCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteCustomerCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.Customer>()
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Customer), request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

file static class CustomerMappingHelper
{
    public static CustomerResponseDto ToDto(Models.Entities.Customer e) => new()
    {
        Id = e.Id,
        CustomerCode = e.CustomerCode,
        Name = e.Name,
        ContactPerson = e.ContactPerson,
        Email = e.Email,
        Phone = e.Phone,
        Address = e.Address,
        TIN = e.TIN,
        BIN = e.BIN,
        CreditLimit = e.CreditLimit,
        PaymentTermsDays = e.PaymentTermsDays,
        IsActive = e.IsActive,
        ReceivableAccountId = e.ReceivableAccountId
    };
}
