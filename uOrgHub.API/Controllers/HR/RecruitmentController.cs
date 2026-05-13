using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.HR.DTOs.Recruitment;
using uOrgHub.HR.Features.Recruitment.Commands;
using uOrgHub.HR.Features.Recruitment.Queries;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.HR;

[Authorize]
public class RecruitmentController : BaseController
{
    private readonly IMediator _mediator;

    public RecruitmentController(IMediator mediator) => _mediator = mediator;

    [HttpGet("job-postings")]
    public async Task<IActionResult> GetJobPostings([FromQuery] PaginationRequest request, [FromQuery] JobPostingStatus? status = null)
    {
        var result = await _mediator.Send(new GetJobPostingsQuery(request, status));
        return Ok(ApiResponse<PagedResult<JobPostingResponseDto>>.Ok(result));
    }

    [HttpPost("job-postings")]
    public async Task<IActionResult> CreateJobPosting([FromBody] CreateJobPostingDto dto)
    {
        var result = await _mediator.Send(new CreateJobPostingCommand(dto));
        return Ok(ApiResponse<JobPostingResponseDto>.Ok(result, "Job posting created successfully."));
    }

    [HttpPut("job-postings/{id:guid}")]
    public async Task<IActionResult> UpdateJobPosting(Guid id, [FromBody] UpdateJobPostingDto dto)
    {
        var result = await _mediator.Send(new UpdateJobPostingCommand(id, dto));
        return Ok(ApiResponse<JobPostingResponseDto>.Ok(result, "Job posting updated successfully."));
    }

    [HttpGet("candidates")]
    public async Task<IActionResult> GetCandidates([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetCandidatesQuery(request));
        return Ok(ApiResponse<PagedResult<CandidateResponseDto>>.Ok(result));
    }

    [HttpPost("candidates")]
    public async Task<IActionResult> CreateCandidate([FromBody] CreateCandidateDto dto)
    {
        var result = await _mediator.Send(new CreateCandidateCommand(dto));
        return Ok(ApiResponse<CandidateResponseDto>.Ok(result, "Candidate created successfully."));
    }

    [HttpGet("applications")]
    public async Task<IActionResult> GetApplications([FromQuery] PaginationRequest request, [FromQuery] Guid? jobPostingId = null)
    {
        var result = await _mediator.Send(new GetJobApplicationsQuery(request, jobPostingId));
        return Ok(ApiResponse<PagedResult<JobApplicationResponseDto>>.Ok(result));
    }

    [HttpPost("applications")]
    public async Task<IActionResult> CreateApplication([FromBody] CreateJobApplicationDto dto)
    {
        var result = await _mediator.Send(new CreateJobApplicationCommand(dto));
        return Ok(ApiResponse<JobApplicationResponseDto>.Ok(result, "Application submitted successfully."));
    }

    [HttpPut("applications/{id:guid}")]
    public async Task<IActionResult> UpdateApplication(Guid id, [FromBody] UpdateJobApplicationDto dto)
    {
        var result = await _mediator.Send(new UpdateJobApplicationCommand(id, dto));
        return Ok(ApiResponse<JobApplicationResponseDto>.Ok(result, "Application updated successfully."));
    }

    [HttpGet("interviews")]
    public async Task<IActionResult> GetInterviews([FromQuery] PaginationRequest request, [FromQuery] Guid? jobApplicationId = null)
    {
        var result = await _mediator.Send(new GetInterviewSchedulesQuery(request, jobApplicationId));
        return Ok(ApiResponse<PagedResult<InterviewScheduleResponseDto>>.Ok(result));
    }

    [HttpPost("interviews")]
    public async Task<IActionResult> ScheduleInterview([FromBody] CreateInterviewScheduleDto dto)
    {
        var result = await _mediator.Send(new ScheduleInterviewCommand(dto));
        return Ok(ApiResponse<InterviewScheduleResponseDto>.Ok(result, "Interview scheduled successfully."));
    }

    [HttpGet("onboarding-checklists")]
    public async Task<IActionResult> GetChecklists([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetOnboardingChecklistsQuery(request));
        return Ok(ApiResponse<PagedResult<OnboardingChecklistResponseDto>>.Ok(result));
    }

    [HttpPost("onboarding-checklists")]
    public async Task<IActionResult> CreateChecklist([FromBody] CreateOnboardingChecklistDto dto)
    {
        var result = await _mediator.Send(new CreateOnboardingChecklistCommand(dto));
        return Ok(ApiResponse<OnboardingChecklistResponseDto>.Ok(result, "Onboarding checklist created successfully."));
    }

    [HttpGet("employee-onboardings")]
    public async Task<IActionResult> GetEmployeeOnboardings([FromQuery] PaginationRequest request, [FromQuery] Guid? employeeId = null)
    {
        var result = await _mediator.Send(new GetEmployeeOnboardingsQuery(request, employeeId));
        return Ok(ApiResponse<PagedResult<EmployeeOnboardingResponseDto>>.Ok(result));
    }

    [HttpPost("employee-onboardings")]
    public async Task<IActionResult> CreateEmployeeOnboarding([FromBody] CreateEmployeeOnboardingDto dto)
    {
        var result = await _mediator.Send(new CreateEmployeeOnboardingCommand(dto));
        return Ok(ApiResponse<EmployeeOnboardingResponseDto>.Ok(result, "Employee onboarding initiated successfully."));
    }
}
