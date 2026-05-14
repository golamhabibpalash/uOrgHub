using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.ProjectExpenses.Commands;

public record CreateProjectExpenseCommand(CreateProjectExpenseDto Dto) : ICommand<ProjectExpenseResponseDto>;
public record UpdateProjectExpenseCommand(Guid Id, UpdateProjectExpenseDto Dto) : ICommand<ProjectExpenseResponseDto>;
public record DeleteProjectExpenseCommand(Guid Id) : ICommand<Unit>;
public record ApproveProjectExpenseCommand(Guid Id, ApproveExpenseDto Dto) : ICommand<ProjectExpenseResponseDto>;

public class CreateProjectExpenseCommandHandler : IRequestHandler<CreateProjectExpenseCommand, ProjectExpenseResponseDto>
{
    private readonly AppDbContext _context;
    public CreateProjectExpenseCommandHandler(AppDbContext context) => _context = context;

    public async Task<ProjectExpenseResponseDto> Handle(CreateProjectExpenseCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var _ = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.ProjectId, ct)
            ?? throw new AppException($"Project '{dto.ProjectId}' not found.", 404);

        var count = await _context.Set<ProjectExpense>().CountAsync(ct);
        var expenseNumber = $"EXP-{DateTime.UtcNow.Year}-{(count + 1):D4}";

        var entity = new ProjectExpense
        {
            ExpenseNumber = expenseNumber,
            ProjectId = dto.ProjectId,
            WBSId = dto.WBSId,
            ExpenseDate = dto.ExpenseDate,
            ExpenseType = dto.ExpenseType,
            Description = dto.Description,
            Amount = dto.Amount,
            VendorId = dto.VendorId,
            POId = dto.POId,
            InvoiceNumber = dto.InvoiceNumber,
            RecordedById = dto.RecordedById,
            Status = ExpenseStatus.Draft,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<ProjectExpense>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return ExpenseMapper.ToDto(entity);
    }
}

public class UpdateProjectExpenseCommandHandler : IRequestHandler<UpdateProjectExpenseCommand, ProjectExpenseResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateProjectExpenseCommandHandler(AppDbContext context) => _context = context;

    public async Task<ProjectExpenseResponseDto> Handle(UpdateProjectExpenseCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectExpense>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectExpense), request.Id);

        if (entity.Status != ExpenseStatus.Draft)
            throw new AppException("Only Draft expenses can be updated.");

        var dto = request.Dto;
        entity.WBSId = dto.WBSId;
        entity.ExpenseDate = dto.ExpenseDate;
        entity.ExpenseType = dto.ExpenseType;
        entity.Description = dto.Description;
        entity.Amount = dto.Amount;
        entity.VendorId = dto.VendorId;
        entity.POId = dto.POId;
        entity.InvoiceNumber = dto.InvoiceNumber;
        entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return ExpenseMapper.ToDto(entity);
    }
}

public class DeleteProjectExpenseCommandHandler : IRequestHandler<DeleteProjectExpenseCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteProjectExpenseCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteProjectExpenseCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectExpense>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectExpense), request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public class ApproveProjectExpenseCommandHandler : IRequestHandler<ApproveProjectExpenseCommand, ProjectExpenseResponseDto>
{
    private readonly AppDbContext _context;
    public ApproveProjectExpenseCommandHandler(AppDbContext context) => _context = context;

    public async Task<ProjectExpenseResponseDto> Handle(ApproveProjectExpenseCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectExpense>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectExpense), request.Id);

        if (entity.Status != ExpenseStatus.Draft)
            throw new AppException("Only Draft expenses can be approved.");

        entity.Status = ExpenseStatus.Approved;
        entity.ApprovedById = request.Dto.ApprovedById;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return ExpenseMapper.ToDto(entity);
    }
}

public static class ExpenseMapper
{
    public static ProjectExpenseResponseDto ToDto(ProjectExpense e) => new()
    {
        Id = e.Id,
        ExpenseNumber = e.ExpenseNumber,
        ProjectId = e.ProjectId,
        WBSId = e.WBSId,
        ExpenseDate = e.ExpenseDate,
        ExpenseType = e.ExpenseType,
        Description = e.Description,
        Amount = e.Amount,
        VendorId = e.VendorId,
        POId = e.POId,
        InvoiceNumber = e.InvoiceNumber,
        RecordedById = e.RecordedById,
        Status = e.Status,
        ApprovedById = e.ApprovedById,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt
    };
}
