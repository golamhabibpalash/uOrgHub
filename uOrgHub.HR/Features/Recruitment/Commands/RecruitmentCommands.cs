using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs.Recruitment;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Models.Entities;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.HR.Features.Recruitment.Commands;

public record CreateJobPostingCommand(CreateJobPostingDto Dto) : ICommand<JobPostingResponseDto>;
public record UpdateJobPostingCommand(Guid Id, UpdateJobPostingDto Dto) : ICommand<JobPostingResponseDto>;
public record CreateCandidateCommand(CreateCandidateDto Dto) : ICommand<CandidateResponseDto>;
public record CreateJobApplicationCommand(CreateJobApplicationDto Dto) : ICommand<JobApplicationResponseDto>;
public record UpdateJobApplicationCommand(Guid Id, UpdateJobApplicationDto Dto) : ICommand<JobApplicationResponseDto>;
public record ScheduleInterviewCommand(CreateInterviewScheduleDto Dto) : ICommand<InterviewScheduleResponseDto>;
public record CreateOnboardingChecklistCommand(CreateOnboardingChecklistDto Dto) : ICommand<OnboardingChecklistResponseDto>;
public record CreateEmployeeOnboardingCommand(CreateEmployeeOnboardingDto Dto) : ICommand<EmployeeOnboardingResponseDto>;

public class CreateJobPostingCommandHandler : IRequestHandler<CreateJobPostingCommand, JobPostingResponseDto>
{
    private readonly AppDbContext _context;
    public CreateJobPostingCommandHandler(AppDbContext context) => _context = context;

    public async Task<JobPostingResponseDto> Handle(CreateJobPostingCommand request, CancellationToken ct)
    {
        var exists = await _context.Set<JobPosting>().AnyAsync(x => !x.IsDeleted && x.JobCode == request.Dto.JobCode, ct);
        if (exists) throw new AppException($"Job code '{request.Dto.JobCode}' already exists.");

        var entity = new JobPosting
        {
            JobCode = request.Dto.JobCode, Title = request.Dto.Title,
            Description = request.Dto.Description, Requirements = request.Dto.Requirements,
            RequiredCount = request.Dto.RequiredCount,
            ExperienceYearsMin = request.Dto.ExperienceYearsMin,
            ExperienceYearsMax = request.Dto.ExperienceYearsMax,
            SalaryMin = request.Dto.SalaryMin, SalaryMax = request.Dto.SalaryMax,
            Status = JobPostingStatus.Draft, PostedDate = request.Dto.PostedDate,
            ClosingDate = request.Dto.ClosingDate,
            DepartmentId = request.Dto.DepartmentId, DesignationId = request.Dto.DesignationId,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<JobPosting>().Add(entity);
        await _context.SaveChangesAsync(ct);

        var dept = await _context.Set<Department>().FindAsync(entity.DepartmentId);
        var desig = await _context.Set<Designation>().FindAsync(entity.DesignationId);
        return RecruitmentMappingHelper.MapJobPostingToDto(entity, dept?.Name ?? string.Empty, desig?.Name ?? string.Empty);
    }
}

public class UpdateJobPostingCommandHandler : IRequestHandler<UpdateJobPostingCommand, JobPostingResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateJobPostingCommandHandler(AppDbContext context) => _context = context;

    public async Task<JobPostingResponseDto> Handle(UpdateJobPostingCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<JobPosting>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(JobPosting), request.Id);

        entity.Title = request.Dto.Title; entity.Description = request.Dto.Description;
        entity.Requirements = request.Dto.Requirements; entity.RequiredCount = request.Dto.RequiredCount;
        entity.Status = request.Dto.Status; entity.ClosingDate = request.Dto.ClosingDate;
        entity.SalaryMin = request.Dto.SalaryMin; entity.SalaryMax = request.Dto.SalaryMax;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Set<JobPosting>().Update(entity);
        await _context.SaveChangesAsync(ct);

        var dept = await _context.Set<Department>().FindAsync(entity.DepartmentId);
        var desig = await _context.Set<Designation>().FindAsync(entity.DesignationId);
        return RecruitmentMappingHelper.MapJobPostingToDto(entity, dept?.Name ?? string.Empty, desig?.Name ?? string.Empty);
    }
}

public class CreateCandidateCommandHandler : IRequestHandler<CreateCandidateCommand, CandidateResponseDto>
{
    private readonly AppDbContext _context;
    public CreateCandidateCommandHandler(AppDbContext context) => _context = context;

    public async Task<CandidateResponseDto> Handle(CreateCandidateCommand request, CancellationToken ct)
    {
        var exists = await _context.Set<Candidate>().AnyAsync(x => !x.IsDeleted && x.Email == request.Dto.Email, ct);
        if (exists) throw new AppException($"Candidate with email '{request.Dto.Email}' already exists.");

        var entity = new Candidate
        {
            FirstName = request.Dto.FirstName, LastName = request.Dto.LastName,
            Email = request.Dto.Email, Phone = request.Dto.Phone,
            LinkedInProfile = request.Dto.LinkedInProfile, PortfolioUrl = request.Dto.PortfolioUrl,
            TotalExperienceYears = request.Dto.TotalExperienceYears,
            CurrentSalary = request.Dto.CurrentSalary, ExpectedSalary = request.Dto.ExpectedSalary,
            CurrentCompany = request.Dto.CurrentCompany, CurrentDesignation = request.Dto.CurrentDesignation,
            Source = request.Dto.Source, ReferredBy = request.Dto.ReferredBy,
            Gender = request.Dto.Gender, Skills = request.Dto.Skills,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<Candidate>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return new CandidateResponseDto
        {
            Id = entity.Id, FirstName = entity.FirstName, LastName = entity.LastName,
            Email = entity.Email, Phone = entity.Phone, TotalExperienceYears = entity.TotalExperienceYears,
            ExpectedSalary = entity.ExpectedSalary, CurrentCompany = entity.CurrentCompany,
            Skills = entity.Skills, Gender = entity.Gender, CreatedAt = entity.CreatedAt
        };
    }
}

public class CreateJobApplicationCommandHandler : IRequestHandler<CreateJobApplicationCommand, JobApplicationResponseDto>
{
    private readonly AppDbContext _context;
    public CreateJobApplicationCommandHandler(AppDbContext context) => _context = context;

    public async Task<JobApplicationResponseDto> Handle(CreateJobApplicationCommand request, CancellationToken ct)
    {
        var exists = await _context.Set<JobApplication>()
            .AnyAsync(x => !x.IsDeleted && x.CandidateId == request.Dto.CandidateId && x.JobPostingId == request.Dto.JobPostingId, ct);
        if (exists) throw new AppException("Candidate has already applied for this position.");

        var entity = new JobApplication
        {
            CandidateId = request.Dto.CandidateId, JobPostingId = request.Dto.JobPostingId,
            ApplicationDate = DateTime.UtcNow, Status = ApplicationStatus.Applied,
            CoverLetter = request.Dto.CoverLetter, Notes = request.Dto.Notes,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<JobApplication>().Add(entity);
        await _context.SaveChangesAsync(ct);

        var candidate = await _context.Set<Candidate>().FindAsync(entity.CandidateId);
        var posting = await _context.Set<JobPosting>().FindAsync(entity.JobPostingId);
        return new JobApplicationResponseDto
        {
            Id = entity.Id, CandidateId = entity.CandidateId,
            CandidateName = candidate != null ? $"{candidate.FirstName} {candidate.LastName}" : string.Empty,
            CandidateEmail = candidate?.Email ?? string.Empty,
            JobPostingId = entity.JobPostingId, JobPostingTitle = posting?.Title ?? string.Empty,
            ApplicationDate = entity.ApplicationDate, Status = entity.Status,
            Notes = entity.Notes, CreatedAt = entity.CreatedAt
        };
    }
}

public class UpdateJobApplicationCommandHandler : IRequestHandler<UpdateJobApplicationCommand, JobApplicationResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateJobApplicationCommandHandler(AppDbContext context) => _context = context;

    public async Task<JobApplicationResponseDto> Handle(UpdateJobApplicationCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<JobApplication>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(JobApplication), request.Id);

        entity.Status = request.Dto.Status; entity.Notes = request.Dto.Notes;
        entity.HiringScore = request.Dto.HiringScore; entity.UpdatedAt = DateTime.UtcNow;

        _context.Set<JobApplication>().Update(entity);
        await _context.SaveChangesAsync(ct);

        var candidate = await _context.Set<Candidate>().FindAsync(entity.CandidateId);
        var posting = await _context.Set<JobPosting>().FindAsync(entity.JobPostingId);
        return new JobApplicationResponseDto
        {
            Id = entity.Id, CandidateId = entity.CandidateId,
            CandidateName = candidate != null ? $"{candidate.FirstName} {candidate.LastName}" : string.Empty,
            CandidateEmail = candidate?.Email ?? string.Empty,
            JobPostingId = entity.JobPostingId, JobPostingTitle = posting?.Title ?? string.Empty,
            ApplicationDate = entity.ApplicationDate, Status = entity.Status,
            Notes = entity.Notes, HiringScore = entity.HiringScore, CreatedAt = entity.CreatedAt
        };
    }
}

public class ScheduleInterviewCommandHandler : IRequestHandler<ScheduleInterviewCommand, InterviewScheduleResponseDto>
{
    private readonly AppDbContext _context;
    public ScheduleInterviewCommandHandler(AppDbContext context) => _context = context;

    public async Task<InterviewScheduleResponseDto> Handle(ScheduleInterviewCommand request, CancellationToken ct)
    {
        var entity = new InterviewSchedule
        {
            JobApplicationId = request.Dto.JobApplicationId,
            InterviewType = request.Dto.InterviewType,
            ScheduledAt = request.Dto.ScheduledAt, DurationMinutes = request.Dto.DurationMinutes,
            Location = request.Dto.Location, MeetingLink = request.Dto.MeetingLink,
            InterviewerIds = request.Dto.InterviewerIds, Status = ApprovalStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<InterviewSchedule>().Add(entity);
        await _context.SaveChangesAsync(ct);

        var app = await _context.Set<JobApplication>().Include(x => x.Candidate)
            .FirstOrDefaultAsync(x => x.Id == entity.JobApplicationId, ct);
        var candidateName = app?.Candidate != null
            ? $"{app.Candidate.FirstName} {app.Candidate.LastName}"
            : string.Empty;

        return new InterviewScheduleResponseDto
        {
            Id = entity.Id, JobApplicationId = entity.JobApplicationId,
            CandidateName = candidateName, InterviewType = entity.InterviewType,
            ScheduledAt = entity.ScheduledAt, DurationMinutes = entity.DurationMinutes,
            Location = entity.Location, MeetingLink = entity.MeetingLink,
            Status = entity.Status, CreatedAt = entity.CreatedAt
        };
    }
}

public class CreateOnboardingChecklistCommandHandler : IRequestHandler<CreateOnboardingChecklistCommand, OnboardingChecklistResponseDto>
{
    private readonly AppDbContext _context;
    public CreateOnboardingChecklistCommandHandler(AppDbContext context) => _context = context;

    public async Task<OnboardingChecklistResponseDto> Handle(CreateOnboardingChecklistCommand request, CancellationToken ct)
    {
        var entity = new OnboardingChecklist
        {
            Name = request.Dto.Name, Description = request.Dto.Description,
            IsDefault = request.Dto.IsDefault, IsActive = request.Dto.IsActive,
            DesignationId = request.Dto.DesignationId, CreatedAt = DateTime.UtcNow
        };
        _context.Set<OnboardingChecklist>().Add(entity);
        await _context.SaveChangesAsync(ct);

        var desig = entity.DesignationId.HasValue
            ? await _context.Set<Designation>().FindAsync(entity.DesignationId)
            : null;
        return new OnboardingChecklistResponseDto
        {
            Id = entity.Id, Name = entity.Name, Description = entity.Description,
            IsDefault = entity.IsDefault, IsActive = entity.IsActive,
            DesignationId = entity.DesignationId, DesignationName = desig?.Name,
            CreatedAt = entity.CreatedAt
        };
    }
}

public class CreateEmployeeOnboardingCommandHandler : IRequestHandler<CreateEmployeeOnboardingCommand, EmployeeOnboardingResponseDto>
{
    private readonly AppDbContext _context;
    public CreateEmployeeOnboardingCommandHandler(AppDbContext context) => _context = context;

    public async Task<EmployeeOnboardingResponseDto> Handle(CreateEmployeeOnboardingCommand request, CancellationToken ct)
    {
        var entity = new EmployeeOnboarding
        {
            EmployeeId = request.Dto.EmployeeId,
            OnboardingChecklistId = request.Dto.OnboardingChecklistId,
            StartDate = request.Dto.StartDate, Status = OnboardingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<EmployeeOnboarding>().Add(entity);
        await _context.SaveChangesAsync(ct);

        var employee = await _context.Set<Employee>().FindAsync(entity.EmployeeId);
        var checklist = await _context.Set<OnboardingChecklist>().FindAsync(entity.OnboardingChecklistId);
        return new EmployeeOnboardingResponseDto
        {
            Id = entity.Id, EmployeeId = entity.EmployeeId,
            EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : string.Empty,
            OnboardingChecklistId = entity.OnboardingChecklistId,
            ChecklistName = checklist?.Name ?? string.Empty,
            StartDate = entity.StartDate, Status = entity.Status, CreatedAt = entity.CreatedAt
        };
    }
}

file static class RecruitmentMappingHelper
{
    internal static JobPostingResponseDto MapJobPostingToDto(JobPosting e, string deptName, string desigName) => new()
    {
        Id = e.Id, JobCode = e.JobCode, Title = e.Title, Description = e.Description,
        Requirements = e.Requirements, RequiredCount = e.RequiredCount,
        ExperienceYearsMin = e.ExperienceYearsMin, ExperienceYearsMax = e.ExperienceYearsMax,
        SalaryMin = e.SalaryMin, SalaryMax = e.SalaryMax, Status = e.Status,
        PostedDate = e.PostedDate, ClosingDate = e.ClosingDate,
        DepartmentId = e.DepartmentId, DepartmentName = deptName,
        DesignationId = e.DesignationId, DesignationName = desigName,
        CreatedAt = e.CreatedAt
    };
}
