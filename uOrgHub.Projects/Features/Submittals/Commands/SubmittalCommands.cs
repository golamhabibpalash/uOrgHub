using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.Submittals.Commands;

public record CreateSubmittalCommand(CreateSubmittalDto Dto) : ICommand<SubmittalResponseDto>;
public record UpdateSubmittalCommand(Guid Id, UpdateSubmittalDto Dto) : ICommand<SubmittalResponseDto>;
public record ReviewSubmittalCommand(Guid Id, ReviewSubmittalDto Dto) : ICommand<SubmittalResponseDto>;
public record DeleteSubmittalCommand(Guid Id) : ICommand<Unit>;

public class CreateSubmittalCommandHandler : IRequestHandler<CreateSubmittalCommand, SubmittalResponseDto>
{
    private readonly AppDbContext _context;
    public CreateSubmittalCommandHandler(AppDbContext context) => _context = context;

    public async Task<SubmittalResponseDto> Handle(CreateSubmittalCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var _ = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), dto.ProjectId);

        var count = await _context.Set<ProjectSubmittal>().CountAsync(ct);
        var submittalNumber = $"SUB-{DateTime.UtcNow.Year}-{(count + 1):D4}";

        var entity = new ProjectSubmittal
        {
            ProjectId = dto.ProjectId,
            SubmittalNumber = submittalNumber,
            Title = dto.Title,
            ContractorReference = dto.ContractorReference,
            Description = dto.Description,
            SubmittedById = dto.SubmittedById,
            SubmittedDate = dto.SubmittedDate,
            Status = dto.SubmittedDate.HasValue ? SubmittalStatus.Submitted : SubmittalStatus.Draft,
            FilePath = dto.FilePath,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<ProjectSubmittal>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return SubmittalMapper.ToDto(entity);
    }
}

public class UpdateSubmittalCommandHandler : IRequestHandler<UpdateSubmittalCommand, SubmittalResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateSubmittalCommandHandler(AppDbContext context) => _context = context;

    public async Task<SubmittalResponseDto> Handle(UpdateSubmittalCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectSubmittal>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectSubmittal), request.Id);

        if (entity.Status == SubmittalStatus.Approved)
            throw new AppException("Approved submittals cannot be modified.");

        var dto = request.Dto;
        entity.Title = dto.Title;
        entity.ContractorReference = dto.ContractorReference;
        entity.Description = dto.Description;
        entity.FilePath = dto.FilePath;
        entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return SubmittalMapper.ToDto(entity);
    }
}

public class ReviewSubmittalCommandHandler : IRequestHandler<ReviewSubmittalCommand, SubmittalResponseDto>
{
    private readonly AppDbContext _context;
    public ReviewSubmittalCommandHandler(AppDbContext context) => _context = context;

    public async Task<SubmittalResponseDto> Handle(ReviewSubmittalCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectSubmittal>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectSubmittal), request.Id);

        if (entity.Status != SubmittalStatus.Submitted && entity.Status != SubmittalStatus.UnderReview)
            throw new AppException("Only submitted or under-review submittals can be reviewed.");

        var dto = request.Dto;
        entity.Status = dto.Status;
        entity.ReviewedById = dto.ReviewedById;
        entity.ReviewDate = dto.ReviewDate;
        entity.ReviewComments = dto.ReviewComments;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return SubmittalMapper.ToDto(entity);
    }
}

public class DeleteSubmittalCommandHandler : IRequestHandler<DeleteSubmittalCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteSubmittalCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteSubmittalCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectSubmittal>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectSubmittal), request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public static class SubmittalMapper
{
    public static SubmittalResponseDto ToDto(ProjectSubmittal e) => new()
    {
        Id = e.Id,
        ProjectId = e.ProjectId,
        SubmittalNumber = e.SubmittalNumber,
        Title = e.Title,
        ContractorReference = e.ContractorReference,
        Description = e.Description,
        Status = e.Status,
        SubmittedById = e.SubmittedById,
        SubmittedDate = e.SubmittedDate,
        ReviewedById = e.ReviewedById,
        ReviewDate = e.ReviewDate,
        ReviewComments = e.ReviewComments,
        FilePath = e.FilePath,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt
    };
}
