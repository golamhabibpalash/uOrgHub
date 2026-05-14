using MediatR;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features._Common;
using uOrgHub.Inventory.Repositories;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Inventory.Features.Catalog.Commands;

public record CreateAttributeDefinitionCommand(CreateAttributeDefinitionDto Dto) : ICommand<AttributeDefinitionResponseDto>;
public record UpdateAttributeDefinitionCommand(Guid Id, UpdateAttributeDefinitionDto Dto) : ICommand<AttributeDefinitionResponseDto>;
public record DeleteAttributeDefinitionCommand(Guid Id) : ICommand<Unit>;

public class CreateAttributeDefinitionCommandHandler : IRequestHandler<CreateAttributeDefinitionCommand, AttributeDefinitionResponseDto>
{
    private readonly IAttributeDefinitionRepository _repo;
    private readonly AppDbContext _context;

    public CreateAttributeDefinitionCommandHandler(IAttributeDefinitionRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<AttributeDefinitionResponseDto> Handle(CreateAttributeDefinitionCommand request, CancellationToken ct)
    {
        var entity = new Models.Entities.AttributeDefinition
        {
            Name = request.Dto.Name,
            DataType = request.Dto.DataType,
            CategoryId = request.Dto.CategoryId,
            IsRequired = request.Dto.IsRequired,
            PredefinedValues = request.Dto.PredefinedValues,
            CreatedAt = DateTime.UtcNow
        };
        var created = await _repo.CreateAsync(entity);
        return await ToDto(created, ct);
    }

    private async Task<AttributeDefinitionResponseDto> ToDto(Models.Entities.AttributeDefinition e, CancellationToken ct)
    {
        Models.Entities.InventoryCategory? cat = null;
        if (e.CategoryId.HasValue)
            cat = await _context.Set<Models.Entities.InventoryCategory>().FindAsync(new object[] { e.CategoryId.Value }, ct);

        return new AttributeDefinitionResponseDto
        {
            Id = e.Id, Name = e.Name, DataType = e.DataType,
            CategoryId = e.CategoryId, CategoryName = cat?.Name,
            IsRequired = e.IsRequired, PredefinedValues = e.PredefinedValues,
            IsActive = e.IsActive, CreatedAt = e.CreatedAt
        };
    }
}

public class UpdateAttributeDefinitionCommandHandler : IRequestHandler<UpdateAttributeDefinitionCommand, AttributeDefinitionResponseDto>
{
    private readonly IAttributeDefinitionRepository _repo;
    private readonly AppDbContext _context;

    public UpdateAttributeDefinitionCommandHandler(IAttributeDefinitionRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<AttributeDefinitionResponseDto> Handle(UpdateAttributeDefinitionCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Models.Entities.AttributeDefinition), request.Id);

        entity.Name = request.Dto.Name;
        entity.DataType = request.Dto.DataType;
        entity.CategoryId = request.Dto.CategoryId;
        entity.IsRequired = request.Dto.IsRequired;
        entity.PredefinedValues = request.Dto.PredefinedValues;
        entity.IsActive = request.Dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        var updated = await _repo.UpdateAsync(entity);

        Models.Entities.InventoryCategory? cat = null;
        if (updated.CategoryId.HasValue)
            cat = await _context.Set<Models.Entities.InventoryCategory>().FindAsync(new object[] { updated.CategoryId.Value }, ct);

        return new AttributeDefinitionResponseDto
        {
            Id = updated.Id, Name = updated.Name, DataType = updated.DataType,
            CategoryId = updated.CategoryId, CategoryName = cat?.Name,
            IsRequired = updated.IsRequired, PredefinedValues = updated.PredefinedValues,
            IsActive = updated.IsActive, CreatedAt = updated.CreatedAt
        };
    }
}

public class DeleteAttributeDefinitionCommandHandler : IRequestHandler<DeleteAttributeDefinitionCommand, Unit>
{
    private readonly IAttributeDefinitionRepository _repo;
    public DeleteAttributeDefinitionCommandHandler(IAttributeDefinitionRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeleteAttributeDefinitionCommand request, CancellationToken ct)
    {
        if (!await _repo.ExistsAsync(request.Id))
            throw new NotFoundException(nameof(Models.Entities.AttributeDefinition), request.Id);
        await _repo.DeleteAsync(request.Id);
        return Unit.Value;
    }
}
