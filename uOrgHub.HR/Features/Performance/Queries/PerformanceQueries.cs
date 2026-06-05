using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs.Performance;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.HR.Features.Performance.Queries;

public record GetReviewCyclesQuery(PaginationRequest Request) : IQuery<PagedResult<ReviewCycleResponseDto>>;
public record GetAllReviewCyclesQuery : IQuery<List<ReviewCycleResponseDto>>;
public record GetGoalsQuery(PaginationRequest Request, Guid? EmployeeId = null, Guid? ReviewCycleId = null) : IQuery<PagedResult<GoalResponseDto>>;
public record GetAllGoalsQuery : IQuery<List<GoalResponseDto>>;
public record GetPerformanceReviewsQuery(PaginationRequest Request, Guid? EmployeeId = null, Guid? ReviewCycleId = null) : IQuery<PagedResult<PerformanceReviewResponseDto>>;
public record GetAllPerformanceReviewsQuery : IQuery<List<PerformanceReviewResponseDto>>;
public record GetTrainingProgramsQuery(PaginationRequest Request) : IQuery<PagedResult<TrainingProgramResponseDto>>;
public record GetAllTrainingProgramsQuery : IQuery<List<TrainingProgramResponseDto>>;
public record GetEmployeeTrainingsQuery(PaginationRequest Request, Guid? EmployeeId = null) : IQuery<PagedResult<EmployeeTrainingResponseDto>>;
public record GetAllEmployeeTrainingsQuery : IQuery<List<EmployeeTrainingResponseDto>>;

public class GetReviewCyclesQueryHandler : IRequestHandler<GetReviewCyclesQuery, PagedResult<ReviewCycleResponseDto>>
{
    private readonly AppDbContext _context;
    public GetReviewCyclesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<ReviewCycleResponseDto>> Handle(GetReviewCyclesQuery request, CancellationToken ct)
    {
        var query = _context.Set<ReviewCycle>().Where(x => !x.IsDeleted);
        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.StartDate)
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<ReviewCycleResponseDto>
        {
            Items = items.Select(e => new ReviewCycleResponseDto
            {
                Id = e.Id, Name = e.Name, Type = e.Type, StartDate = e.StartDate,
                EndDate = e.EndDate, Status = e.Status, Description = e.Description, CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAllReviewCyclesQueryHandler : IRequestHandler<GetAllReviewCyclesQuery, List<ReviewCycleResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllReviewCyclesQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<ReviewCycleResponseDto>> Handle(GetAllReviewCyclesQuery request, CancellationToken ct)
    {
        var items = await _context.Set<ReviewCycle>()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.StartDate)
            .Select(x => new ReviewCycleResponseDto
            {
                Id = x.Id, Name = x.Name, Type = x.Type, StartDate = x.StartDate,
                EndDate = x.EndDate, Status = x.Status, Description = x.Description, CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
        return items;
    }
}

public class GetGoalsQueryHandler : IRequestHandler<GetGoalsQuery, PagedResult<GoalResponseDto>>
{
    private readonly AppDbContext _context;
    public GetGoalsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<GoalResponseDto>> Handle(GetGoalsQuery request, CancellationToken ct)
    {
        var query = _context.Set<Goal>()
            .Include(x => x.Employee).Include(x => x.ReviewCycle).Include(x => x.KPI)
            .Where(x => !x.IsDeleted);

        if (request.EmployeeId.HasValue) query = query.Where(x => x.EmployeeId == request.EmployeeId);
        if (request.ReviewCycleId.HasValue) query = query.Where(x => x.ReviewCycleId == request.ReviewCycleId);

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderBy(x => x.Title)
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<GoalResponseDto>
        {
            Items = items.Select(e => new GoalResponseDto
            {
                Id = e.Id, EmployeeId = e.EmployeeId,
                EmployeeName = e.Employee != null ? $"{e.Employee.FirstName} {e.Employee.LastName}" : string.Empty,
                ReviewCycleId = e.ReviewCycleId, ReviewCycleName = e.ReviewCycle?.Name ?? string.Empty,
                KPIId = e.KPIId, KPIName = e.KPI?.Name, Title = e.Title, Description = e.Description,
                TargetValue = e.TargetValue, AchievedValue = e.AchievedValue, Weight = e.Weight,
                Status = e.Status, DueDate = e.DueDate, Remarks = e.Remarks, CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAllGoalsQueryHandler : IRequestHandler<GetAllGoalsQuery, List<GoalResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllGoalsQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<GoalResponseDto>> Handle(GetAllGoalsQuery request, CancellationToken ct)
    {
        var items = await _context.Set<Goal>()
            .Include(x => x.Employee).Include(x => x.ReviewCycle).Include(x => x.KPI)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Title)
            .Select(x => new GoalResponseDto
            {
                Id = x.Id, EmployeeId = x.EmployeeId,
                EmployeeName = x.Employee != null ? $"{x.Employee.FirstName} {x.Employee.LastName}" : string.Empty,
                ReviewCycleId = x.ReviewCycleId, ReviewCycleName = x.ReviewCycle != null ? x.ReviewCycle.Name : string.Empty,
                KPIId = x.KPIId, KPIName = x.KPI != null ? x.KPI.Name : null, Title = x.Title, Description = x.Description,
                TargetValue = x.TargetValue, AchievedValue = x.AchievedValue, Weight = x.Weight,
                Status = x.Status, DueDate = x.DueDate, Remarks = x.Remarks, CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
        return items;
    }
}

public class GetPerformanceReviewsQueryHandler : IRequestHandler<GetPerformanceReviewsQuery, PagedResult<PerformanceReviewResponseDto>>
{
    private readonly AppDbContext _context;
    public GetPerformanceReviewsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<PerformanceReviewResponseDto>> Handle(GetPerformanceReviewsQuery request, CancellationToken ct)
    {
        var query = _context.Set<PerformanceReview>()
            .Include(x => x.Employee).Include(x => x.Reviewer).Include(x => x.ReviewCycle)
            .Where(x => !x.IsDeleted);

        if (request.EmployeeId.HasValue) query = query.Where(x => x.EmployeeId == request.EmployeeId);
        if (request.ReviewCycleId.HasValue) query = query.Where(x => x.ReviewCycleId == request.ReviewCycleId);

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.DueDate)
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<PerformanceReviewResponseDto>
        {
            Items = items.Select(e => new PerformanceReviewResponseDto
            {
                Id = e.Id, ReviewCycleId = e.ReviewCycleId,
                ReviewCycleName = e.ReviewCycle?.Name ?? string.Empty,
                EmployeeId = e.EmployeeId,
                EmployeeName = e.Employee != null ? $"{e.Employee.FirstName} {e.Employee.LastName}" : string.Empty,
                ReviewerId = e.ReviewerId,
                ReviewerName = e.Reviewer != null ? $"{e.Reviewer.FirstName} {e.Reviewer.LastName}" : string.Empty,
                ReviewType = e.ReviewType, OverallRating = e.OverallRating, Comments = e.Comments,
                Strengths = e.Strengths, AreasForImprovement = e.AreasForImprovement,
                Status = e.Status, DueDate = e.DueDate, SubmittedDate = e.SubmittedDate, CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAllPerformanceReviewsQueryHandler : IRequestHandler<GetAllPerformanceReviewsQuery, List<PerformanceReviewResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllPerformanceReviewsQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<PerformanceReviewResponseDto>> Handle(GetAllPerformanceReviewsQuery request, CancellationToken ct)
    {
        var items = await _context.Set<PerformanceReview>()
            .Include(x => x.Employee).Include(x => x.Reviewer).Include(x => x.ReviewCycle)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.DueDate)
            .Select(x => new PerformanceReviewResponseDto
            {
                Id = x.Id, ReviewCycleId = x.ReviewCycleId,
                ReviewCycleName = x.ReviewCycle != null ? x.ReviewCycle.Name : string.Empty,
                EmployeeId = x.EmployeeId,
                EmployeeName = x.Employee != null ? $"{x.Employee.FirstName} {x.Employee.LastName}" : string.Empty,
                ReviewerId = x.ReviewerId,
                ReviewerName = x.Reviewer != null ? $"{x.Reviewer.FirstName} {x.Reviewer.LastName}" : string.Empty,
                ReviewType = x.ReviewType, OverallRating = x.OverallRating, Comments = x.Comments,
                Strengths = x.Strengths, AreasForImprovement = x.AreasForImprovement,
                Status = x.Status, DueDate = x.DueDate, SubmittedDate = x.SubmittedDate, CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
        return items;
    }
}

public class GetTrainingProgramsQueryHandler : IRequestHandler<GetTrainingProgramsQuery, PagedResult<TrainingProgramResponseDto>>
{
    private readonly AppDbContext _context;
    public GetTrainingProgramsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<TrainingProgramResponseDto>> Handle(GetTrainingProgramsQuery request, CancellationToken ct)
    {
        var query = _context.Set<TrainingProgram>().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.Title);

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.StartDate)
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<TrainingProgramResponseDto>
        {
            Items = items.Select(e => new TrainingProgramResponseDto
            {
                Id = e.Id, Title = e.Title, Description = e.Description, Category = e.Category,
                Mode = e.Mode, DurationHours = e.DurationHours, Provider = e.Provider,
                Location = e.Location, MaxParticipants = e.MaxParticipants, Cost = e.Cost,
                StartDate = e.StartDate, EndDate = e.EndDate, Status = e.Status,
                HasCertificate = e.HasCertificate, CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAllTrainingProgramsQueryHandler : IRequestHandler<GetAllTrainingProgramsQuery, List<TrainingProgramResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllTrainingProgramsQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<TrainingProgramResponseDto>> Handle(GetAllTrainingProgramsQuery request, CancellationToken ct)
    {
        var items = await _context.Set<TrainingProgram>()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.StartDate)
            .Select(x => new TrainingProgramResponseDto
            {
                Id = x.Id, Title = x.Title, Description = x.Description, Category = x.Category,
                Mode = x.Mode, DurationHours = x.DurationHours, Provider = x.Provider,
                Location = x.Location, MaxParticipants = x.MaxParticipants, Cost = x.Cost,
                StartDate = x.StartDate, EndDate = x.EndDate, Status = x.Status,
                HasCertificate = x.HasCertificate, CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
        return items;
    }
}

public class GetEmployeeTrainingsQueryHandler : IRequestHandler<GetEmployeeTrainingsQuery, PagedResult<EmployeeTrainingResponseDto>>
{
    private readonly AppDbContext _context;
    public GetEmployeeTrainingsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<EmployeeTrainingResponseDto>> Handle(GetEmployeeTrainingsQuery request, CancellationToken ct)
    {
        var query = _context.Set<EmployeeTraining>()
            .Include(x => x.Employee).Include(x => x.TrainingProgram)
            .Where(x => !x.IsDeleted);

        if (request.EmployeeId.HasValue) query = query.Where(x => x.EmployeeId == request.EmployeeId);

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.EnrollmentDate)
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<EmployeeTrainingResponseDto>
        {
            Items = items.Select(e => new EmployeeTrainingResponseDto
            {
                Id = e.Id, EmployeeId = e.EmployeeId,
                EmployeeName = e.Employee != null ? $"{e.Employee.FirstName} {e.Employee.LastName}" : string.Empty,
                TrainingProgramId = e.TrainingProgramId, TrainingTitle = e.TrainingProgram?.Title ?? string.Empty,
                EnrollmentDate = e.EnrollmentDate, CompletionDate = e.CompletionDate,
                Status = e.Status, Score = e.Score, CertificatePath = e.CertificatePath,
                Remarks = e.Remarks, CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAllEmployeeTrainingsQueryHandler : IRequestHandler<GetAllEmployeeTrainingsQuery, List<EmployeeTrainingResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllEmployeeTrainingsQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<EmployeeTrainingResponseDto>> Handle(GetAllEmployeeTrainingsQuery request, CancellationToken ct)
    {
        var items = await _context.Set<EmployeeTraining>()
            .Include(x => x.Employee).Include(x => x.TrainingProgram)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.EnrollmentDate)
            .Select(x => new EmployeeTrainingResponseDto
            {
                Id = x.Id, EmployeeId = x.EmployeeId,
                EmployeeName = x.Employee != null ? $"{x.Employee.FirstName} {x.Employee.LastName}" : string.Empty,
                TrainingProgramId = x.TrainingProgramId, TrainingTitle = x.TrainingProgram != null ? x.TrainingProgram.Title : string.Empty,
                EnrollmentDate = x.EnrollmentDate, CompletionDate = x.CompletionDate,
                Status = x.Status, Score = x.Score, CertificatePath = x.CertificatePath,
                Remarks = x.Remarks, CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
        return items;
    }
}
