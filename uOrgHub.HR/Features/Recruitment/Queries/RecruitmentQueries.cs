using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs.Recruitment;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Models.Entities;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Models;

namespace uOrgHub.HR.Features.Recruitment.Queries;

public record GetJobPostingsQuery(PaginationRequest Request, JobPostingStatus? Status = null) : IQuery<PagedResult<JobPostingResponseDto>>;
public record GetCandidatesQuery(PaginationRequest Request) : IQuery<PagedResult<CandidateResponseDto>>;
public record GetJobApplicationsQuery(PaginationRequest Request, Guid? JobPostingId = null) : IQuery<PagedResult<JobApplicationResponseDto>>;
public record GetInterviewSchedulesQuery(PaginationRequest Request, Guid? JobApplicationId = null) : IQuery<PagedResult<InterviewScheduleResponseDto>>;
public record GetOnboardingChecklistsQuery(PaginationRequest Request) : IQuery<PagedResult<OnboardingChecklistResponseDto>>;
public record GetEmployeeOnboardingsQuery(PaginationRequest Request, Guid? EmployeeId = null) : IQuery<PagedResult<EmployeeOnboardingResponseDto>>;

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
            query = query.Where(x => x.Title.Contains(request.Request.Search) || x.JobCode.Contains(request.Request.Search));

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

public class GetCandidatesQueryHandler : IRequestHandler<GetCandidatesQuery, PagedResult<CandidateResponseDto>>
{
    private readonly AppDbContext _context;
    public GetCandidatesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<CandidateResponseDto>> Handle(GetCandidatesQuery request, CancellationToken ct)
    {
        var query = _context.Set<Candidate>().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.FirstName.Contains(request.Request.Search)
                || x.LastName.Contains(request.Request.Search)
                || x.Email.Contains(request.Request.Search));

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
            query = query.Where(x => x.Name.Contains(request.Request.Search));

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
