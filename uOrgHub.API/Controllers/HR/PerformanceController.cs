using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.HR.DTOs.Performance;
using uOrgHub.HR.Features.Performance.Commands;
using uOrgHub.HR.Features.Performance.Queries;
using uOrgHub.HR.Reporting.ExportColumns;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.HR;

[Authorize]
[Route("api/v1/performance")]
public class PerformanceController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;

    public PerformanceController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet("review-cycles")]
    [RequireClaim(Claims.HR.ReviewCycles.View)]
    public async Task<IActionResult> GetReviewCycles([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetReviewCyclesQuery(request));
        return Ok(ApiResponse<PagedResult<ReviewCycleResponseDto>>.Ok(result));
    }

    [HttpGet("review-cycles/export")]
    [RequireClaim(Claims.HR.ReviewCycles.Export)]
    public async Task<IActionResult> ExportReviewCycles([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllReviewCyclesQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, ReviewCycleExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Review Cycles"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost("review-cycles")]
    [RequireClaim(Claims.HR.ReviewCycles.Create)]
    public async Task<IActionResult> CreateReviewCycle([FromBody] CreateReviewCycleDto dto)
    {
        var result = await _mediator.Send(new CreateReviewCycleCommand(dto));
        return Ok(ApiResponse<ReviewCycleResponseDto>.Ok(result, "Review cycle created successfully."));
    }

    [HttpPut("review-cycles/{id:guid}")]
    [RequireClaim(Claims.HR.ReviewCycles.Edit)]
    public async Task<IActionResult> UpdateReviewCycle(Guid id, [FromBody] UpdateReviewCycleDto dto)
    {
        var result = await _mediator.Send(new UpdateReviewCycleCommand(id, dto));
        return Ok(ApiResponse<ReviewCycleResponseDto>.Ok(result, "Review cycle updated successfully."));
    }

    [HttpPost("kpis")]
    [RequireClaim(Claims.HR.PerformanceReviews.Edit)]
    public async Task<IActionResult> CreateKPI([FromBody] CreateKPIDto dto)
    {
        var result = await _mediator.Send(new CreateKPICommand(dto));
        return Ok(ApiResponse<KPIResponseDto>.Ok(result, "KPI created successfully."));
    }

    [HttpGet("goals")]
    [RequireClaim(Claims.HR.Goals.View)]
    public async Task<IActionResult> GetGoals([FromQuery] PaginationRequest request, [FromQuery] Guid? employeeId = null, [FromQuery] Guid? reviewCycleId = null)
    {
        var result = await _mediator.Send(new GetGoalsQuery(request, employeeId, reviewCycleId));
        return Ok(ApiResponse<PagedResult<GoalResponseDto>>.Ok(result));
    }

    [HttpGet("goals/export")]
    [RequireClaim(Claims.HR.Goals.Export)]
    public async Task<IActionResult> ExportGoals([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllGoalsQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, GoalExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Goals"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost("goals")]
    [RequireClaim(Claims.HR.Goals.Create)]
    public async Task<IActionResult> CreateGoal([FromBody] CreateGoalDto dto)
    {
        var result = await _mediator.Send(new CreateGoalCommand(dto));
        return Ok(ApiResponse<GoalResponseDto>.Ok(result, "Goal created successfully."));
    }

    [HttpPut("goals/{id:guid}")]
    [RequireClaim(Claims.HR.Goals.Edit)]
    public async Task<IActionResult> UpdateGoal(Guid id, [FromBody] UpdateGoalDto dto)
    {
        var result = await _mediator.Send(new UpdateGoalCommand(id, dto));
        return Ok(ApiResponse<GoalResponseDto>.Ok(result, "Goal updated successfully."));
    }

    [HttpGet("reviews")]
    [RequireClaim(Claims.HR.PerformanceReviews.View)]
    public async Task<IActionResult> GetReviews([FromQuery] PaginationRequest request, [FromQuery] Guid? employeeId = null, [FromQuery] Guid? reviewCycleId = null)
    {
        var result = await _mediator.Send(new GetPerformanceReviewsQuery(request, employeeId, reviewCycleId));
        return Ok(ApiResponse<PagedResult<PerformanceReviewResponseDto>>.Ok(result));
    }

    [HttpGet("reviews/export")]
    [RequireClaim(Claims.HR.PerformanceReviews.Export)]
    public async Task<IActionResult> ExportReviews([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllPerformanceReviewsQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, PerformanceReviewExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Performance Reviews"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost("reviews")]
    [RequireClaim(Claims.HR.PerformanceReviews.Create)]
    public async Task<IActionResult> CreateReview([FromBody] CreatePerformanceReviewDto dto)
    {
        var result = await _mediator.Send(new CreatePerformanceReviewCommand(dto));
        return Ok(ApiResponse<PerformanceReviewResponseDto>.Ok(result, "Performance review created successfully."));
    }

    [HttpPut("reviews/{id:guid}/submit")]
    [RequireClaim(Claims.HR.PerformanceReviews.Edit)]
    public async Task<IActionResult> SubmitReview(Guid id, [FromBody] UpdatePerformanceReviewDto dto)
    {
        var result = await _mediator.Send(new SubmitPerformanceReviewCommand(id, dto));
        return Ok(ApiResponse<PerformanceReviewResponseDto>.Ok(result, "Performance review submitted successfully."));
    }

    [HttpGet("training-programs")]
    [RequireClaim(Claims.HR.PerformanceReviews.View)]
    public async Task<IActionResult> GetTrainingPrograms([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetTrainingProgramsQuery(request));
        return Ok(ApiResponse<PagedResult<TrainingProgramResponseDto>>.Ok(result));
    }

    [HttpGet("training-programs/export")]
    [RequireClaim(Claims.HR.TrainingPrograms.Export)]
    public async Task<IActionResult> ExportTrainingPrograms([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllTrainingProgramsQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, TrainingProgramExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Training Programs"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost("training-programs")]
    [RequireClaim(Claims.HR.PerformanceReviews.Edit)]
    public async Task<IActionResult> CreateTrainingProgram([FromBody] CreateTrainingProgramDto dto)
    {
        var result = await _mediator.Send(new CreateTrainingProgramCommand(dto));
        return Ok(ApiResponse<TrainingProgramResponseDto>.Ok(result, "Training program created successfully."));
    }

    [HttpGet("employee-trainings")]
    [RequireClaim(Claims.HR.PerformanceReviews.View)]
    public async Task<IActionResult> GetEmployeeTrainings([FromQuery] PaginationRequest request, [FromQuery] Guid? employeeId = null)
    {
        var result = await _mediator.Send(new GetEmployeeTrainingsQuery(request, employeeId));
        return Ok(ApiResponse<PagedResult<EmployeeTrainingResponseDto>>.Ok(result));
    }

    [HttpGet("employee-trainings/export")]
    [RequireClaim(Claims.HR.EmployeeTrainings.Export)]
    public async Task<IActionResult> ExportEmployeeTrainings([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllEmployeeTrainingsQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, EmployeeTrainingExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Employee Trainings"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost("employee-trainings")]
    [RequireClaim(Claims.HR.PerformanceReviews.Edit)]
    public async Task<IActionResult> EnrollTraining([FromBody] CreateEmployeeTrainingDto dto)
    {
        var result = await _mediator.Send(new EnrollEmployeeTrainingCommand(dto));
        return Ok(ApiResponse<EmployeeTrainingResponseDto>.Ok(result, "Employee enrolled in training successfully."));
    }

    [HttpPut("employee-trainings/{id:guid}")]
    [RequireClaim(Claims.HR.PerformanceReviews.Edit)]
    public async Task<IActionResult> UpdateTraining(Guid id, [FromBody] UpdateEmployeeTrainingDto dto)
    {
        var result = await _mediator.Send(new UpdateEmployeeTrainingCommand(id, dto));
        return Ok(ApiResponse<EmployeeTrainingResponseDto>.Ok(result, "Employee training updated successfully."));
    }
}
