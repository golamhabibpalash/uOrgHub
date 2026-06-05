using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs.Recruitment;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Models.Entities;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.HR.Features.Recruitment.Queries;

public record GetJobPostingsQuery(PaginationRequest Request, JobPostingStatus? Status = null) : IQuery<PagedResult<JobPostingResponseDto>>;
public record GetAllJobPostingsQuery : IQuery<List<JobPostingResponseDto>>;
public record GetCandidatesQuery(PaginationRequest Request) : IQuery<PagedResult<CandidateResponseDto>>;
public record GetAllCandidatesQuery : IQuery<List<CandidateResponseDto>>;
public record GetJobApplicationsQuery(PaginationRequest Request, Guid? JobPostingId = null) : IQuery<PagedResult<JobApplicationResponseDto>>;
public record GetAllJobApplicationsQuery : IQuery<List<JobApplicationResponseDto>>;
public record GetInterviewSchedulesQuery(PaginationRequest Request, Guid? JobApplicationId = null) : IQuery<PagedResult<InterviewScheduleResponseDto>>;
public record GetAllInterviewSchedulesQuery : IQuery<List<InterviewScheduleResponseDto>>;
public record GetOnboardingChecklistsQuery(PaginationRequest Request) : IQuery<PagedResult<OnboardingChecklistResponseDto>>;
public record GetAllOnboardingChecklistsQuery : IQuery<List<OnboardingChecklistResponseDto>>;
public record GetEmployeeOnboardingsQuery(PaginationRequest Request, Guid? EmployeeId = null) : IQuery<PagedResult<EmployeeOnboardingResponseDto>>;
public record GetAllEmployeeOnboardingsQuery : IQuery<List<EmployeeOnboardingResponseDto>>;

public class GetJobPostingsQueryHandler : IRequestHandler<GetJobPostingsQuery, PagedResult<JobPostingResponseDto>>
{
    private readonly AppDbContext _context;
    public GetJobPostingsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<JobPostingResponseDto>> Handle(GetJobPostingsQuery request, CancellationToken ct)
    {
        var query = _context.Set<JobPosting>()
            .Include(x => x.Department).Include(x => x.Designation)
            .Where(x => !x.IsDeleted);

        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status);
        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.Title, x => x.JobCode);

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.PostedDate).ThenBy(x => x.Title)
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<JobPostingResponseDto>
        {
            Items = items.Select(e => new JobPostingResponseDto
            {
                Id = e.Id, JobCode = e.JobCode, Title = e.Title, Description = e.Description,
                Requirements = e.Requirements, RequiredCount = e.RequiredCount,
                ExperienceYearsMin = e.ExperienceYearsMin, ExperienceYearsMax = e.ExperienceYearsMax,
                SalaryMin = e.SalaryMin, SalaryMax = e.SalaryMax, Status = e.Status,
                PostedDate = e.PostedDate, ClosingDate = e.ClosingDate,
                DepartmentId = e.DepartmentId, DepartmentName = e.Department?.Name ?? string.Empty,
                DesignationId = e.DesignationId, DesignationName = e.Designation?.Name ?? string.Empty,
                CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAllJobPostingsQueryHandler : IRequestHandler<GetAllJobPostingsQuery, List<JobPostingResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllJobPostingsQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<JobPostingResponseDto>> Handle(GetAllJobPostingsQuery request, CancellationToken ct)
    {
        var items = await _context.Set<JobPosting>()
            .Include(x => x.Department).Include(x => x.Designation)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.PostedDate).ThenBy(x => x.Title)
            .Select(x => new JobPostingResponseDto
            {
                Id = x.Id, JobCode = x.JobCode, Title = x.Title, Description = x.Description,
                Requirements = x.Requirements, RequiredCount = x.RequiredCount,
                ExperienceYearsMin = x.ExperienceYearsMin, ExperienceYearsMax = x.ExperienceYearsMax,
                SalaryMin = x.SalaryMin, SalaryMax = x.SalaryMax, Status = x.Status,
                PostedDate = x.PostedDate, ClosingDate = x.ClosingDate,
                DepartmentId = x.DepartmentId, DepartmentName = x.Department != null ? x.Department.Name : string.Empty,
                DesignationId = x.DesignationId, DesignationName = x.Designation != null ? x.Designation.Name : string.Empty,
                CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
        return items;
    }
}

public class GetCandidatesQueryHandler : IRequestHandler<GetCandidatesQuery, PagedResult<CandidateResponseDto>>
{
    private readonly AppDbContext _context;
    public GetCandidatesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<CandidateResponseDto>> Handle(GetCandidatesQuery request, CancellationToken ct)
    {
        var query = _context.Set<Candidate>().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.FirstName, x => x.LastName, x => x.Email);

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<CandidateResponseDto>
        {
            Items = items.Select(e => new CandidateResponseDto
            {
                Id = e.Id, FirstName = e.FirstName, LastName = e.LastName, Email = e.Email,
                Phone = e.Phone, TotalExperienceYears = e.TotalExperienceYears,
                ExpectedSalary = e.ExpectedSalary, CurrentCompany = e.CurrentCompany,
                Skills = e.Skills, Gender = e.Gender, CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAllCandidatesQueryHandler : IRequestHandler<GetAllCandidatesQuery, List<CandidateResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllCandidatesQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<CandidateResponseDto>> Handle(GetAllCandidatesQuery request, CancellationToken ct)
    {
        var items = await _context.Set<Candidate>()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
            .Select(x => new CandidateResponseDto
            {
                Id = x.Id, FirstName = x.FirstName, LastName = x.LastName, Email = x.Email,
                Phone = x.Phone, TotalExperienceYears = x.TotalExperienceYears,
                ExpectedSalary = x.ExpectedSalary, CurrentCompany = x.CurrentCompany,
                Skills = x.Skills, Gender = x.Gender, CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
        return items;
    }
}

public class GetJobApplicationsQueryHandler : IRequestHandler<GetJobApplicationsQuery, PagedResult<JobApplicationResponseDto>>
{
    private readonly AppDbContext _context;
    public GetJobApplicationsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<JobApplicationResponseDto>> Handle(GetJobApplicationsQuery request, CancellationToken ct)
    {
        var query = _context.Set<JobApplication>()
            .Include(x => x.Candidate).Include(x => x.JobPosting)
            .Where(x => !x.IsDeleted);

        if (request.JobPostingId.HasValue) query = query.Where(x => x.JobPostingId == request.JobPostingId);

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.ApplicationDate)
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<JobApplicationResponseDto>
        {
            Items = items.Select(e => new JobApplicationResponseDto
            {
                Id = e.Id, CandidateId = e.CandidateId,
                CandidateName = e.Candidate != null ? $"{e.Candidate.FirstName} {e.Candidate.LastName}" : string.Empty,
                CandidateEmail = e.Candidate?.Email ?? string.Empty,
                JobPostingId = e.JobPostingId, JobPostingTitle = e.JobPosting?.Title ?? string.Empty,
                ApplicationDate = e.ApplicationDate, Status = e.Status,
                Notes = e.Notes, HiringScore = e.HiringScore, CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAllJobApplicationsQueryHandler : IRequestHandler<GetAllJobApplicationsQuery, List<JobApplicationResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllJobApplicationsQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<JobApplicationResponseDto>> Handle(GetAllJobApplicationsQuery request, CancellationToken ct)
    {
        var items = await _context.Set<JobApplication>()
            .Include(x => x.Candidate).Include(x => x.JobPosting)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.ApplicationDate)
            .Select(x => new JobApplicationResponseDto
            {
                Id = x.Id, CandidateId = x.CandidateId,
                CandidateName = x.Candidate != null ? $"{x.Candidate.FirstName} {x.Candidate.LastName}" : string.Empty,
                CandidateEmail = x.Candidate != null ? x.Candidate.Email : string.Empty,
                JobPostingId = x.JobPostingId, JobPostingTitle = x.JobPosting != null ? x.JobPosting.Title : string.Empty,
                ApplicationDate = x.ApplicationDate, Status = x.Status,
                Notes = x.Notes, HiringScore = x.HiringScore, CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
        return items;
    }
}

public class GetInterviewSchedulesQueryHandler : IRequestHandler<GetInterviewSchedulesQuery, PagedResult<InterviewScheduleResponseDto>>
{
    private readonly AppDbContext _context;
    public GetInterviewSchedulesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<InterviewScheduleResponseDto>> Handle(GetInterviewSchedulesQuery request, CancellationToken ct)
    {
        var query = _context.Set<InterviewSchedule>()
            .Include(x => x.JobApplication).ThenInclude(a => a.Candidate)
            .Where(x => !x.IsDeleted);

        if (request.JobApplicationId.HasValue) query = query.Where(x => x.JobApplicationId == request.JobApplicationId);

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.ScheduledAt)
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<InterviewScheduleResponseDto>
        {
            Items = items.Select(e => new InterviewScheduleResponseDto
            {
                Id = e.Id, JobApplicationId = e.JobApplicationId,
                CandidateName = e.JobApplication?.Candidate != null
                    ? $"{e.JobApplication.Candidate.FirstName} {e.JobApplication.Candidate.LastName}"
                    : string.Empty,
                InterviewType = e.InterviewType, ScheduledAt = e.ScheduledAt,
                DurationMinutes = e.DurationMinutes, Location = e.Location,
                MeetingLink = e.MeetingLink, Status = e.Status,
                Feedback = e.Feedback, Rating = e.Rating, CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAllInterviewSchedulesQueryHandler : IRequestHandler<GetAllInterviewSchedulesQuery, List<InterviewScheduleResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllInterviewSchedulesQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<InterviewScheduleResponseDto>> Handle(GetAllInterviewSchedulesQuery request, CancellationToken ct)
    {
        var items = await _context.Set<InterviewSchedule>()
            .Include(x => x.JobApplication).ThenInclude(a => a.Candidate)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.ScheduledAt)
            .Select(x => new InterviewScheduleResponseDto
            {
                Id = x.Id, JobApplicationId = x.JobApplicationId,
                CandidateName = x.JobApplication != null && x.JobApplication.Candidate != null
                    ? $"{x.JobApplication.Candidate.FirstName} {x.JobApplication.Candidate.LastName}"
                    : string.Empty,
                InterviewType = x.InterviewType, ScheduledAt = x.ScheduledAt,
                DurationMinutes = x.DurationMinutes, Location = x.Location,
                MeetingLink = x.MeetingLink, Status = x.Status,
                Feedback = x.Feedback, Rating = x.Rating, CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
        return items;
    }
}

public class GetOnboardingChecklistsQueryHandler : IRequestHandler<GetOnboardingChecklistsQuery, PagedResult<OnboardingChecklistResponseDto>>
{
    private readonly AppDbContext _context;
    public GetOnboardingChecklistsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<OnboardingChecklistResponseDto>> Handle(GetOnboardingChecklistsQuery request, CancellationToken ct)
    {
        var query = _context.Set<OnboardingChecklist>()
            .Include(x => x.Designation)
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.Name);

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderBy(x => x.Name)
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<OnboardingChecklistResponseDto>
        {
            Items = items.Select(e => new OnboardingChecklistResponseDto
            {
                Id = e.Id, Name = e.Name, Description = e.Description,
                IsDefault = e.IsDefault, IsActive = e.IsActive,
                DesignationId = e.DesignationId, DesignationName = e.Designation?.Name,
                CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetEmployeeOnboardingsQueryHandler : IRequestHandler<GetEmployeeOnboardingsQuery, PagedResult<EmployeeOnboardingResponseDto>>
{
    private readonly AppDbContext _context;
    public GetEmployeeOnboardingsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<EmployeeOnboardingResponseDto>> Handle(GetEmployeeOnboardingsQuery request, CancellationToken ct)
    {
        var query = _context.Set<EmployeeOnboarding>()
            .Include(x => x.Employee).Include(x => x.OnboardingChecklist)
            .Where(x => !x.IsDeleted);

        if (request.EmployeeId.HasValue) query = query.Where(x => x.EmployeeId == request.EmployeeId);

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.StartDate)
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<EmployeeOnboardingResponseDto>
        {
            Items = items.Select(e => new EmployeeOnboardingResponseDto
            {
                Id = e.Id, EmployeeId = e.EmployeeId,
                EmployeeName = e.Employee != null ? $"{e.Employee.FirstName} {e.Employee.LastName}" : string.Empty,
                OnboardingChecklistId = e.OnboardingChecklistId,
                ChecklistName = e.OnboardingChecklist?.Name ?? string.Empty,
                StartDate = e.StartDate, CompletionDate = e.CompletionDate,
                Status = e.Status, CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAllOnboardingChecklistsQueryHandler : IRequestHandler<GetAllOnboardingChecklistsQuery, List<OnboardingChecklistResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllOnboardingChecklistsQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<OnboardingChecklistResponseDto>> Handle(GetAllOnboardingChecklistsQuery request, CancellationToken ct)
    {
        var items = await _context.Set<OnboardingChecklist>()
            .Include(x => x.Designation)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(x => new OnboardingChecklistResponseDto
            {
                Id = x.Id, Name = x.Name, Description = x.Description,
                IsDefault = x.IsDefault, IsActive = x.IsActive,
                DesignationId = x.DesignationId, DesignationName = x.Designation != null ? x.Designation.Name : null,
                CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
        return items;
    }
}

public class GetAllEmployeeOnboardingsQueryHandler : IRequestHandler<GetAllEmployeeOnboardingsQuery, List<EmployeeOnboardingResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllEmployeeOnboardingsQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<EmployeeOnboardingResponseDto>> Handle(GetAllEmployeeOnboardingsQuery request, CancellationToken ct)
    {
        var items = await _context.Set<EmployeeOnboarding>()
            .Include(x => x.Employee).Include(x => x.OnboardingChecklist)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.StartDate)
            .Select(x => new EmployeeOnboardingResponseDto
            {
                Id = x.Id, EmployeeId = x.EmployeeId,
                EmployeeName = x.Employee != null ? $"{x.Employee.FirstName} {x.Employee.LastName}" : string.Empty,
                OnboardingChecklistId = x.OnboardingChecklistId,
                ChecklistName = x.OnboardingChecklist != null ? x.OnboardingChecklist.Name : string.Empty,
                StartDate = x.StartDate, CompletionDate = x.CompletionDate,
                Status = x.Status, CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
        return items;
    }
}
