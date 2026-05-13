using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.HR.DTOs.Performance;
using uOrgHub.HR.Features.Performance.Commands;
using uOrgHub.HR.Features.Performance.Queries;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.HR;

[Authorize]
public class PerformanceController : BaseController
{
    private readonly IMediator _mediator;

    public PerformanceController(IMediator mediator) => _mediator = mediator;

    [HttpGet("review-cycles")]
    public async Task<IActionResult> GetReviewCycles([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetReviewCyclesQuery(request));
        return Ok(ApiResponse<PagedResult<ReviewCycleResponseDto>>.Ok(result));
    }

    [HttpPost("review-cycles")]
    public async Task<IActionResult> CreateReviewCycle([FromBody] CreateReviewCycleDto dto)
    {
        var result = await _mediator.Send(new CreateReviewCycleCommand(dto));
        return Ok(ApiResponse<ReviewCycleResponseDto>.Ok(result, "Review cycle created successfully."));
    }

    [HttpPut("review-cycles/{id:guid}")]
    public async Task<IActionResult> UpdateReviewCycle(Guid id, [FromBody] UpdateReviewCycleDto dto)
    {
        var result = await _mediator.Send(new UpdateReviewCycleCommand(id, dto));
        return Ok(ApiResponse<ReviewCycleResponseDto>.Ok(result, "Review cycle updated successfully."));
    }

    [HttpPost("kpis")]
    public async Task<IActionResult> CreateKPI([FromBody] CreateKPIDto dto)
    {
        var result = await _mediator.Send(new CreateKPICommand(dto));
        return Ok(ApiResponse<KPIResponseDto>.Ok(result, "KPI created successfully."));
    }

    [HttpGet("goals")]
    public async Task<IActionResult> GetGoals([FromQuery] PaginationRequest request, [FromQuery] Guid? employeeId = null, [FromQuery] Guid? reviewCycleId = null)
    {
        var result = await _mediator.Send(new GetGoalsQuery(request, employeeId, reviewCycleId));
        return Ok(ApiResponse<PagedResult<GoalResponseDto>>.Ok(result));
    }

    [HttpPost("goals")]
    public async Task<IActionResult> CreateGoal([FromBody] CreateGoalDto dto)
    {
        var result = await _mediator.Send(new CreateGoalCommand(dto));
        return Ok(ApiResponse<GoalResponseDto>.Ok(result, "Goal created successfully."));
    }

    [HttpPut("goals/{id:guid}")]
    public async Task<IActionResult> UpdateGoal(Guid id, [FromBody] UpdateGoalDto dto)
    {
        var result = await _mediator.Send(new UpdateGoalCommand(id, dto));
        return Ok(ApiResponse<GoalResponseDto>.Ok(result, "Goal updated successfully."));
    }

    [HttpGet("reviews")]
    public async Task<IActionResult> GetReviews([FromQuery] PaginationRequest request, [FromQuery] Guid? employeeId = null, [FromQuery] Guid? reviewCycleId = null)
    {
        var result = await _mediator.Send(new GetPerformanceReviewsQuery(request, employeeId, reviewCycleId));
        return Ok(ApiResponse<PagedResult<PerformanceReviewResponseDto>>.Ok(result));
    }

    [HttpPost("reviews")]
    public async Task<IActionResult> CreateReview([FromBody] CreatePerformanceReviewDto dto)
    {
        var result = await _mediator.Send(new CreatePerformanceReviewCommand(dto));
        return Ok(ApiResponse<PerformanceReviewResponseDto>.Ok(result, "Performance review created successfully."));
    }

    [HttpPut("reviews/{id:guid}/submit")]
    public async Task<IActionResult> SubmitReview(Guid id, [FromBody] UpdatePerformanceReviewDto dto)
    {
        var result = await _mediator.Send(new SubmitPerformanceReviewCommand(id, dto));
        return Ok(ApiResponse<PerformanceReviewResponseDto>.Ok(result, "Performance review submitted successfully."));
    }

    [HttpGet("training-programs")]
    public async Task<IActionResult> GetTrainingPrograms([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetTrainingProgramsQuery(request));
        return Ok(ApiResponse<PagedResult<TrainingProgramResponseDto>>.Ok(result));
    }

    [HttpPost("training-programs")]
    public async Task<IActionResult> CreateTrainingProgram([FromBody] CreateTrainingProgramDto dto)
    {
        var result = await _mediator.Send(new CreateTrainingProgramCommand(dto));
        return Ok(ApiResponse<TrainingProgramResponseDto>.Ok(result, "Training program created successfully."));
    }

    [HttpGet("employee-trainings")]
    public async Task<IActionResult> GetEmployeeTrainings([FromQuery] PaginationRequest request, [FromQuery] Guid? employeeId = null)
    {
        var result = await _mediator.Send(new GetEmployeeTrainingsQuery(request, employeeId));
        return Ok(ApiResponse<PagedResult<EmployeeTrainingResponseDto>>.Ok(result));
    }

    [HttpPost("employee-trainings")]
    public async Task<IActionResult> EnrollTraining([FromBody] CreateEmployeeTrainingDto dto)
    {
        var result = await _mediator.Send(new EnrollEmployeeTrainingCommand(dto));
        return Ok(ApiResponse<EmployeeTrainingResponseDto>.Ok(result, "Employee enrolled in training successfully."));
    }

    [HttpPut("employee-trainings/{id:guid}")]
    public async Task<IActionResult> UpdateTraining(Guid id, [FromBody] UpdateEmployeeTrainingDto dto)
    {
        var result = await _mediator.Send(new UpdateEmployeeTrainingCommand(id, dto));
        return Ok(ApiResponse<EmployeeTrainingResponseDto>.Ok(result, "Employee training updated successfully."));
    }
}
