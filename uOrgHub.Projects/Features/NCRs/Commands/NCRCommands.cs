using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.NCRs.Commands;

public record CreateNCRCommand(CreateNCRDto Dto) : ICommand<NCRResponseDto>;
public record UpdateNCRCommand(Guid Id, UpdateNCRDto Dto) : ICommand<NCRResponseDto>;
public record VerifyNCRCommand(Guid Id, VerifyNCRDto Dto) : ICommand<NCRResponseDto>;
public record CloseNCRCommand(Guid Id) : ICommand<NCRResponseDto>;
public record VoidNCRCommand(Guid Id) : ICommand<NCRResponseDto>;
public record DeleteNCRCommand(Guid Id) : ICommand<Unit>;

public class CreateNCRCommandHandler : IRequestHandler<CreateNCRCommand, NCRResponseDto>
{
    private readonly AppDbContext _context;
    public CreateNCRCommandHandler(AppDbContext context) => _context = context;

    public async Task<NCRResponseDto> Handle(CreateNCRCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var _ = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), dto.ProjectId);

        var count = await _context.Set<NonConformanceReport>().CountAsync(ct);
        var ncrNumber = $"NCR-{DateTime.UtcNow.Year}-{(count + 1):D4}";

        var entity = new NonConformanceReport
        {
            ProjectId = dto.ProjectId,
            WBSId = dto.WBSId,
            NCRNumber = ncrNumber,
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            Severity = dto.Severity,
            RaisedById = dto.RaisedById,
            RaisedDate = dto.RaisedDate,
            ResponsibleParty = dto.ResponsibleParty,
            Status = NCRStatus.Open,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<NonConformanceReport>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return NCRMapper.ToDto(entity);
    }
}

public class UpdateNCRCommandHandler : IRequestHandler<UpdateNCRCommand, NCRResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateNCRCommandHandler(AppDbContext context) => _context = context;

    public async Task<NCRResponseDto> Handle(UpdateNCRCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<NonConformanceReport>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(NonConformanceReport), request.Id);

        if (entity.Status == NCRStatus.Closed || entity.Status == NCRStatus.Voided)
            throw new AppException("Closed or voided NCRs cannot be modified.");

        var dto = request.Dto;
        entity.WBSId = dto.WBSId;
        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.Category = dto.Category;
        entity.Severity = dto.Severity;
        entity.ResponsibleParty = dto.ResponsibleParty;
        entity.CorrectiveAction = dto.CorrectiveAction;
        entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return NCRMapper.ToDto(entity);
    }
}

public class VerifyNCRCommandHandler : IRequestHandler<VerifyNCRCommand, NCRResponseDto>
{
    private readonly AppDbContext _context;
    public VerifyNCRCommandHandler(AppDbContext context) => _context = context;

    public async Task<NCRResponseDto> Handle(VerifyNCRCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<NonConformanceReport>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(NonConformanceReport), request.Id);

        if (entity.Status != NCRStatus.Responded)
            throw new AppException("Only responded NCRs can be verified.");

        entity.Status = NCRStatus.Verified;
        entity.VerifiedById = request.Dto.VerifiedById;
        entity.VerifiedDate = request.Dto.VerifiedDate;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return NCRMapper.ToDto(entity);
    }
}

public class CloseNCRCommandHandler : IRequestHandler<CloseNCRCommand, NCRResponseDto>
{
    private readonly AppDbContext _context;
    public CloseNCRCommandHandler(AppDbContext context) => _context = context;

    public async Task<NCRResponseDto> Handle(CloseNCRCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<NonConformanceReport>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(NonConformanceReport), request.Id);

        if (entity.Status != NCRStatus.Verified)
            throw new AppException("Only verified NCRs can be closed.");

        entity.Status = NCRStatus.Closed;
        entity.ClosedDate = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return NCRMapper.ToDto(entity);
    }
}

public class VoidNCRCommandHandler : IRequestHandler<VoidNCRCommand, NCRResponseDto>
{
    private readonly AppDbContext _context;
    public VoidNCRCommandHandler(AppDbContext context) => _context = context;

    public async Task<NCRResponseDto> Handle(VoidNCRCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<NonConformanceReport>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(NonConformanceReport), request.Id);

        if (entity.Status == NCRStatus.Closed)
            throw new AppException("Closed NCRs cannot be voided.");

        entity.Status = NCRStatus.Voided;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return NCRMapper.ToDto(entity);
    }
}

public class DeleteNCRCommandHandler : IRequestHandler<DeleteNCRCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteNCRCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteNCRCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<NonConformanceReport>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(NonConformanceReport), request.Id);

        if (entity.Status == NCRStatus.Closed)
            throw new AppException("Closed NCRs cannot be deleted.");

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public static class NCRMapper
{
    public static NCRResponseDto ToDto(NonConformanceReport e) => new()
    {
        Id = e.Id,
        ProjectId = e.ProjectId,
        WBSId = e.WBSId,
        NCRNumber = e.NCRNumber,
        Title = e.Title,
        Description = e.Description,
        Category = e.Category,
        Severity = e.Severity,
        RaisedById = e.RaisedById,
        RaisedDate = e.RaisedDate,
        ResponsibleParty = e.ResponsibleParty,
        CorrectiveAction = e.CorrectiveAction,
        VerifiedById = e.VerifiedById,
        VerifiedDate = e.VerifiedDate,
        ClosedDate = e.ClosedDate,
        Status = e.Status,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt
    };
}
