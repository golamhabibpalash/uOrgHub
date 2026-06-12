using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.Clients.Commands;

public record CreateClientCommand(CreateClientDto Dto) : ICommand<ClientResponseDto>;
public record UpdateClientCommand(Guid Id, UpdateClientDto Dto) : ICommand<ClientResponseDto>;
public record DeleteClientCommand(Guid Id) : ICommand<Unit>;

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
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<Client>().Add(entity);
        await _context.SaveChangesAsync(ct);
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
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
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
        CreatedAt = e.CreatedAt
    };
}
