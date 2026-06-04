using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.HR.DTOs.Recruitment;
using uOrgHub.HR.Features.Recruitment.Commands;
using uOrgHub.HR.Features.Recruitment.Queries;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.HR;

[Authorize]
[Route("api/v1/recruitment")]
public class RecruitmentController : BaseController
{
    private readonly IMediator _mediator;

    public RecruitmentController(IMediator mediator) => _mediator = mediator;

    [HttpGet("job-postings")]
    [RequireClaim(Claims.HR.JobPostings.View)]
    public async Task<IActionResult> GetJobPostings([FromQuery] PaginationRequest request, [FromQuery] JobPostingStatus? status = null)
    {
        var result = await _mediator.Send(new GetJobPostingsQuery(request, status));
        return Ok(ApiResponse<PagedResult<JobPostingResponseDto>>.Ok(result));
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

    [HttpPost("employee-onboardings")]
    [RequireClaim(Claims.HR.Applications.Edit)]
    public async Task<IActionResult> CreateEmployeeOnboarding([FromBody] CreateEmployeeOnboardingDto dto)
    {
        var result = await _mediator.Send(new CreateEmployeeOnboardingCommand(dto));
        return Ok(ApiResponse<EmployeeOnboardingResponseDto>.Ok(result, "Employee onboarding initiated successfully."));
    }
}
