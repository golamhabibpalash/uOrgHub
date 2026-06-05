using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.HR.DTOs.Payroll;
using uOrgHub.HR.Features.Payroll.Commands;
using uOrgHub.HR.Features.Payroll.Queries;
using uOrgHub.HR.Reporting.ExportColumns;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.HR;

[Authorize]
[Route("api/v1/payroll")]
public class PayrollController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;

    public PayrollController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet("salary-grades")]
    [RequireClaim(Claims.HR.SalaryGrades.View)]
    public async Task<IActionResult> GetSalaryGrades([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetSalaryGradesQuery(request));
        return Ok(ApiResponse<PagedResult<SalaryGradeResponseDto>>.Ok(result));
    }

    [HttpGet("salary-grades/all")]
    [RequireClaim(Claims.HR.SalaryGrades.View)]
    public async Task<IActionResult> GetAllSalaryGradesForDropdown()
    {
        var result = await _mediator.Send(new GetAllSalaryGradesQuery());
        return Ok(ApiResponse<List<SalaryGradeResponseDto>>.Ok(result));
    }

    [HttpGet("salary-grades/export")]
    [RequireClaim(Claims.HR.SalaryGrades.Export)]
    public async Task<IActionResult> ExportSalaryGrades([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllSalaryGradesQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, SalaryGradeExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Salary Grades"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost("salary-grades")]
    [RequireClaim(Claims.HR.SalaryGrades.Create)]
    public async Task<IActionResult> CreateSalaryGrade([FromBody] CreateSalaryGradeDto dto)
    {
        var result = await _mediator.Send(new CreateSalaryGradeCommand(dto));
        return Ok(ApiResponse<SalaryGradeResponseDto>.Ok(result, "Salary grade created successfully."));
    }

    [HttpPut("salary-grades/{id:guid}")]
    [RequireClaim(Claims.HR.SalaryGrades.Edit)]
    public async Task<IActionResult> UpdateSalaryGrade(Guid id, [FromBody] UpdateSalaryGradeDto dto)
    {
        var result = await _mediator.Send(new UpdateSalaryGradeCommand(id, dto));
        return Ok(ApiResponse<SalaryGradeResponseDto>.Ok(result, "Salary grade updated successfully."));
    }

    [HttpDelete("salary-grades/{id:guid}")]
    [RequireClaim(Claims.HR.SalaryGrades.Delete)]
    public async Task<IActionResult> DeleteSalaryGrade(Guid id)
    {
        await _mediator.Send(new DeleteSalaryGradeCommand(id));
        return Ok(ApiResponse<object>.Ok(null!, "Salary grade deleted successfully."));
    }

    [HttpGet("salary-components")]
    [RequireClaim(Claims.HR.SalaryComponents.View)]
    public async Task<IActionResult> GetSalaryComponents([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetSalaryComponentsQuery(request));
        return Ok(ApiResponse<PagedResult<SalaryComponentResponseDto>>.Ok(result));
    }

    [HttpGet("salary-components/export")]
    [RequireClaim(Claims.HR.SalaryComponents.Export)]
    public async Task<IActionResult> ExportSalaryComponents([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllSalaryComponentsQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, SalaryComponentExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Salary Components"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost("salary-components")]
    [RequireClaim(Claims.HR.SalaryComponents.Create)]
    public async Task<IActionResult> CreateSalaryComponent([FromBody] CreateSalaryComponentDto dto)
    {
        var result = await _mediator.Send(new CreateSalaryComponentCommand(dto));
        return Ok(ApiResponse<SalaryComponentResponseDto>.Ok(result, "Salary component created successfully."));
    }

    [HttpGet("cycles")]
    [RequireClaim(Claims.HR.PayrollCycles.View)]
    public async Task<IActionResult> GetCycles([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetPayrollCyclesQuery(request));
        return Ok(ApiResponse<PagedResult<PayrollCycleResponseDto>>.Ok(result));
    }

    [HttpGet("cycles/export")]
    [RequireClaim(Claims.HR.PayrollCycles.Export)]
    public async Task<IActionResult> ExportCycles([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllPayrollCyclesQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, PayrollCycleExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Payroll Cycles"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost("cycles")]
    [RequireClaim(Claims.HR.PayrollCycles.Create)]
    public async Task<IActionResult> CreateCycle([FromBody] CreatePayrollCycleDto dto)
    {
        var result = await _mediator.Send(new CreatePayrollCycleCommand(dto));
        return Ok(ApiResponse<PayrollCycleResponseDto>.Ok(result, "Payroll cycle created successfully."));
    }

    [HttpPut("cycles/{id:guid}")]
    [RequireClaim(Claims.HR.PayrollCycles.Edit)]
    public async Task<IActionResult> UpdateCycle(Guid id, [FromBody] UpdatePayrollCycleDto dto)
    {
        var result = await _mediator.Send(new UpdatePayrollCycleCommand(id, dto));
        return Ok(ApiResponse<PayrollCycleResponseDto>.Ok(result, "Payroll cycle updated successfully."));
    }

    [HttpGet("cycles/{cycleId:guid}/entries")]
    [RequireClaim(Claims.HR.PayrollCycles.View)]
    public async Task<IActionResult> GetEntries(Guid cycleId, [FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetPayrollEntriesQuery(cycleId, request));
        return Ok(ApiResponse<PagedResult<PayrollEntryResponseDto>>.Ok(result));
    }

    [HttpGet("cycles/{cycleId:guid}/entries/export")]
    [RequireClaim(Claims.HR.PayrollEntries.Export)]
    public async Task<IActionResult> ExportEntries(Guid cycleId, [FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllPayrollEntriesQuery(cycleId));
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, PayrollEntryExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Payroll Entries"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost("overtime-rules")]
    [RequireClaim(Claims.HR.PayrollCycles.Edit)]
    public async Task<IActionResult> CreateOvertimeRule([FromBody] CreateOvertimeRuleDto dto)
    {
        var result = await _mediator.Send(new CreateOvertimeRuleCommand(dto));
        return Ok(ApiResponse<OvertimeRuleResponseDto>.Ok(result, "Overtime rule created successfully."));
    }

    [HttpGet("expenses")]
    [RequireClaim(Claims.HR.ExpenseRequests.View)]
    public async Task<IActionResult> GetExpenses([FromQuery] PaginationRequest request, [FromQuery] Guid? employeeId = null)
    {
        var result = await _mediator.Send(new GetExpenseRequestsQuery(request, employeeId));
        return Ok(ApiResponse<PagedResult<ExpenseRequestResponseDto>>.Ok(result));
    }

    [HttpGet("expenses/export")]
    [RequireClaim(Claims.HR.ExpenseRequests.Export)]
    public async Task<IActionResult> ExportExpenses([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllExpenseRequestsQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, ExpenseRequestExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Expense Requests"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost("expenses")]
    [RequireClaim(Claims.HR.ExpenseRequests.Create)]
    public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseRequestDto dto)
    {
        var result = await _mediator.Send(new CreateExpenseRequestCommand(dto));
        return Ok(ApiResponse<ExpenseRequestResponseDto>.Ok(result, "Expense request submitted successfully."));
    }

    [HttpPut("expenses/{id:guid}/approve")]
    [RequireClaim(Claims.HR.ExpenseRequests.Approve)]
    public async Task<IActionResult> ApproveExpense(Guid id, [FromBody] ApproveExpenseDto dto)
    {
        var result = await _mediator.Send(new ApproveExpenseRequestCommand(id, dto));
        return Ok(ApiResponse<ExpenseRequestResponseDto>.Ok(result, "Expense request processed successfully."));
    }
}
