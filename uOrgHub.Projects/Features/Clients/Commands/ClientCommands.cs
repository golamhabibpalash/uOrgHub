using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.AR;
using uOrgHub.Accounts.Features.AR;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.Clients.Commands;

public record CreateClientCommand(CreateClientDto Dto) : ICommand<ClientResponseDto>;
public record UpdateClientCommand(Guid Id, UpdateClientDto Dto) : ICommand<ClientResponseDto>;
public record DeleteClientCommand(Guid Id) : ICommand<Unit>;
public record CreateCustomerFromClientCommand(Guid ClientId, CreateCustomerFromClientDto Dto) : ICommand<ClientResponseDto>;

public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, ClientResponseDto>
{
    private readonly AppDbContext _context;
    public CreateClientCommandHandler(AppDbContext context) => _context = context;

    public async Task<ClientResponseDto> Handle(CreateClientCommand request, CancellationToken ct)
    {
        var dto = request.Dto;
        var count = await _context.Set<Client>().IgnoreQueryFilters().CountAsync(ct);
        var code = $"CLT-{DateTime.UtcNow.Year}-{(count + 1):D4}";

        var entity = new Client
        {
            ClientCode = code,
            CompanyName = dto.CompanyName,
            ContactPerson = dto.ContactPerson,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            ClientType = dto.ClientType,
            Status = dto.Status,
            Notes = dto.Notes,
            CustomerId = await ClientCustomerLink.ValidateAsync(_context, dto.CustomerId, null, ct),
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<Client>().Add(entity);
        await _context.SaveChangesAsync(ct);
        await _context.Entry(entity).Reference(x => x.Customer).LoadAsync(ct);
        return ClientMapper.ToDto(entity);
    }
}

public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, ClientResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateClientCommandHandler(AppDbContext context) => _context = context;

    public async Task<ClientResponseDto> Handle(UpdateClientCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Client>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Client), request.Id);

        var dto = request.Dto;
        entity.CompanyName = dto.CompanyName;
        entity.ContactPerson = dto.ContactPerson;
        entity.Email = dto.Email;
        entity.Phone = dto.Phone;
        entity.Address = dto.Address;
        entity.ClientType = dto.ClientType;
        entity.Status = dto.Status;
        entity.Notes = dto.Notes;
        entity.CustomerId = await ClientCustomerLink.ValidateAsync(_context, dto.CustomerId, entity.Id, ct);
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        await _context.Entry(entity).Reference(x => x.Customer).LoadAsync(ct);
        return ClientMapper.ToDto(entity);
    }
}

public class DeleteClientCommandHandler : IRequestHandler<DeleteClientCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteClientCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteClientCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Client>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Client), request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

/// <summary>
/// Creates the client's AR customer through Accounts' own CreateCustomerCommand (so code generation
/// and validation stay in one place), copying the client's details, then links it.
/// </summary>
public class CreateCustomerFromClientCommandHandler : IRequestHandler<CreateCustomerFromClientCommand, ClientResponseDto>
{
    private readonly AppDbContext _context;
    private readonly ISender _sender;

    public CreateCustomerFromClientCommandHandler(AppDbContext context, ISender sender)
    {
        _context = context;
        _sender = sender;
    }

    public async Task<ClientResponseDto> Handle(CreateCustomerFromClientCommand request, CancellationToken ct)
    {
        var client = await _context.Set<Client>()
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.ClientId, ct)
            ?? throw new NotFoundException(nameof(Client), request.ClientId);

        if (client.Customer is { IsDeleted: false } existing)
            throw new AppException($"{client.CompanyName} is already linked to customer {existing.CustomerCode} – {existing.Name}.");

        var customer = await _sender.Send(new CreateCustomerCommand(new CreateCustomerDto
        {
            Name = client.CompanyName,
            ContactPerson = client.ContactPerson,
            Email = client.Email,
            Phone = client.Phone,
            Address = client.Address,
            PaymentTermsDays = request.Dto.PaymentTermsDays,
            ReceivableAccountId = request.Dto.ReceivableAccountId,
        }), ct);

        client.CustomerId = customer.Id;
        client.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        await _context.Entry(client).Reference(x => x.Customer).LoadAsync(ct);
        return ClientMapper.ToDto(client);
    }
}

internal static class ClientCustomerLink
{
    /// <summary>
    /// A customer may back only one client — two clients sharing one AR account would make each
    /// client's outstanding balance unreadable.
    /// </summary>
    public static async Task<Guid?> ValidateAsync(AppDbContext context, Guid? customerId, Guid? clientId, CancellationToken ct)
    {
        if (customerId is not { } id || id == Guid.Empty)
            return null;

        if (!await context.Set<Customer>().AnyAsync(c => c.Id == id && !c.IsDeleted, ct))
            throw new AppException("The selected customer does not exist.");

        var other = await context.Set<Client>()
            .Where(c => !c.IsDeleted && c.CustomerId == id && c.Id != clientId)
            .Select(c => c.CompanyName)
            .FirstOrDefaultAsync(ct);
        if (other != null)
            throw new AppException($"That customer is already linked to client '{other}'.");

        return id;
    }
}

public static class ClientMapper
{
    public static ClientResponseDto ToDto(Client e) => new()
    {
        Id = e.Id,
        ClientCode = e.ClientCode,
        CompanyName = e.CompanyName,
        ContactPerson = e.ContactPerson,
        Email = e.Email,
        Phone = e.Phone,
        Address = e.Address,
        ClientType = e.ClientType,
        Status = e.Status,
        Notes = e.Notes,
        CustomerId = e.CustomerId,
        CustomerCode = e.Customer?.CustomerCode,
        CustomerName = e.Customer?.Name,
        CreatedAt = e.CreatedAt
    };
}
