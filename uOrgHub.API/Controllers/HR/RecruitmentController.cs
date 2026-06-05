using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.HR.DTOs.Recruitment;
using uOrgHub.HR.Features.Recruitment.Commands;
using uOrgHub.HR.Features.Recruitment.Queries;
using uOrgHub.HR.Models.Enums;
using uOrgHub.HR.Reporting.ExportColumns;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.HR;

[Authorize]
[Route("api/v1/recruitment")]
public class RecruitmentController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;

    public RecruitmentController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet("job-postings")]
    [RequireClaim(Claims.HR.JobPostings.View)]
    public async Task<IActionResult> GetJobPostings([FromQuery] PaginationRequest request, [FromQuery] JobPostingStatus? status = null)
    {
        var result = await _mediator.Send(new GetJobPostingsQuery(request, status));
        return Ok(ApiResponse<PagedResult<JobPostingResponseDto>>.Ok(result));
    }

    [HttpGet("job-postings/export")]
    [RequireClaim(Claims.HR.JobPostings.Export)]
    public async Task<IActionResult> ExportJobPostings([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllJobPostingsQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, JobPostingExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Job Postings"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost("job-postings")]
    [RequireClaim(Claims.HR.JobPostings.Create)]
    public async Task<IActionResult> CreateJobPosting([FromBody] CreateJobPostingDto dto)
    {
        var result = await _mediator.Send(new CreateJobPostingCommand(dto));
        return Ok(ApiResponse<JobPostingResponseDto>.Ok(result, "Job posting created successfully."));
    }

    [HttpPut("job-postings/{id:guid}")]
    [RequireClaim(Claims.HR.JobPostings.Edit)]
    public async Task<IActionResult> UpdateJobPosting(Guid id, [FromBody] UpdateJobPostingDto dto)
    {
        var result = await _mediator.Send(new UpdateJobPostingCommand(id, dto));
        return Ok(ApiResponse<JobPostingResponseDto>.Ok(result, "Job posting updated successfully."));
    }

    [HttpGet("candidates")]
    [RequireClaim(Claims.HR.Candidates.View)]
    public async Task<IActionResult> GetCandidates([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetCandidatesQuery(request));
        return Ok(ApiResponse<PagedResult<CandidateResponseDto>>.Ok(result));
    }

    [HttpGet("candidates/export")]
    [RequireClaim(Claims.HR.Candidates.Export)]
    public async Task<IActionResult> ExportCandidates([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllCandidatesQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, CandidateExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Candidates"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost("candidates")]
    [RequireClaim(Claims.HR.Candidates.Create)]
    public async Task<IActionResult> CreateCandidate([FromBody] CreateCandidateDto dto)
    {
        var result = await _mediator.Send(new CreateCandidateCommand(dto));
        return Ok(ApiResponse<CandidateResponseDto>.Ok(result, "Candidate created successfully."));
    }

    [HttpGet("applications")]
    [RequireClaim(Claims.HR.Applications.View)]
    public async Task<IActionResult> GetApplications([FromQuery] PaginationRequest request, [FromQuery] Guid? jobPostingId = null)
    {
        var result = await _mediator.Send(new GetJobApplicationsQuery(request, jobPostingId));
        return Ok(ApiResponse<PagedResult<JobApplicationResponseDto>>.Ok(result));
    }

    [HttpGet("applications/export")]
    [RequireClaim(Claims.HR.Applications.Export)]
    public async Task<IActionResult> ExportApplications([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllJobApplicationsQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, JobApplicationExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Applications"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost("applications")]
    [RequireClaim(Claims.HR.Applications.Create)]
    public async Task<IActionResult> CreateApplication([FromBody] CreateJobApplicationDto dto)
    {
        var result = await _mediator.Send(new CreateJobApplicationCommand(dto));
        return Ok(ApiResponse<JobApplicationResponseDto>.Ok(result, "Application submitted successfully."));
    }

    [HttpPut("applications/{id:guid}")]
    [RequireClaim(Claims.HR.Applications.Edit)]
    public async Task<IActionResult> UpdateApplication(Guid id, [FromBody] UpdateJobApplicationDto dto)
    {
        var result = await _mediator.Send(new UpdateJobApplicationCommand(id, dto));
        return Ok(ApiResponse<JobApplicationResponseDto>.Ok(result, "Application updated successfully."));
    }

    [HttpGet("interviews")]
    [RequireClaim(Claims.HR.Interviews.View)]
    public async Task<IActionResult> GetInterviews([FromQuery] PaginationRequest request, [FromQuery] Guid? jobApplicationId = null)
    {
        var result = await _mediator.Send(new GetInterviewSchedulesQuery(request, jobApplicationId));
        return Ok(ApiResponse<PagedResult<InterviewScheduleResponseDto>>.Ok(result));
    }

    [HttpGet("interviews/export")]
    [RequireClaim(Claims.HR.Interviews.Export)]
    public async Task<IActionResult> ExportInterviews([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllInterviewSchedulesQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, InterviewScheduleExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Interviews"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost("interviews")]
    [RequireClaim(Claims.HR.Interviews.Create)]
    public async Task<IActionResult> ScheduleInterview([FromBody] CreateInterviewScheduleDto dto)
    {
        var result = await _mediator.Send(new ScheduleInterviewCommand(dto));
        return Ok(ApiResponse<InterviewScheduleResponseDto>.Ok(result, "Interview scheduled successfully."));
    }

    [HttpGet("onboarding-checklists")]
    [RequireClaim(Claims.HR.Applications.View)]
    public async Task<IActionResult> GetChecklists([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetOnboardingChecklistsQuery(request));
        return Ok(ApiResponse<PagedResult<OnboardingChecklistResponseDto>>.Ok(result));
    }

    [HttpGet("onboarding-checklists/export")]
    [RequireClaim(Claims.HR.OnboardingChecklists.Export)]
    public async Task<IActionResult> ExportChecklists([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllOnboardingChecklistsQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, OnboardingChecklistExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Onboarding Checklists"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost("onboarding-checklists")]
    [RequireClaim(Claims.HR.Applications.Edit)]
    public async Task<IActionResult> CreateChecklist([FromBody] CreateOnboardingChecklistDto dto)
    {
        var result = await _mediator.Send(new CreateOnboardingChecklistCommand(dto));
        return Ok(ApiResponse<OnboardingChecklistResponseDto>.Ok(result, "Onboarding checklist created successfully."));
    }

    [HttpGet("employee-onboardings")]
    [RequireClaim(Claims.HR.Applications.View)]
    public async Task<IActionResult> GetEmployeeOnboardings([FromQuery] PaginationRequest request, [FromQuery] Guid? employeeId = null)
    {
        var result = await _mediator.Send(new GetEmployeeOnboardingsQuery(request, employeeId));
        return Ok(ApiResponse<PagedResult<EmployeeOnboardingResponseDto>>.Ok(result));
    }

    [HttpGet("employee-onboardings/export")]
    [RequireClaim(Claims.HR.EmployeeOnboardings.Export)]
    public async Task<IActionResult> ExportEmployeeOnboardings([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllEmployeeOnboardingsQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, EmployeeOnboardingExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Employee Onboardings"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost("employee-onboardings")]
    [RequireClaim(Claims.HR.Applications.Edit)]
    public async Task<IActionResult> CreateEmployeeOnboarding([FromBody] CreateEmployeeOnboardingDto dto)
    {
        var result = await _mediator.Send(new CreateEmployeeOnboardingCommand(dto));
        return Ok(ApiResponse<EmployeeOnboardingResponseDto>.Ok(result, "Employee onboarding initiated successfully."));
    }
}
