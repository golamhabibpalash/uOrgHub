using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrgHub.Application.Features.HRM.EmployeeAttendance.Commands;
using OrgHub.Application.Features.Others.Interfaces;
using OrgHub.Domain.Enums;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OrgHub.Api.Areas.HRM.Controllers;

/// <summary>
/// Controller for managing attendance-related operations.
/// </summary>
[Route("api/[area]/[controller]")]
[ApiController]
[Area("HRM")]
[Authorize]
public class AttendancesController(IMediator mediatR, ILoggingService loggingService) : ControllerBase
{
    private readonly IMediator _mediatR = mediatR;
    private readonly ILoggingService _loggingService = loggingService;

    /// <summary>
    /// Retrieves list of attendance records.
    /// </summary>
    /// <returns>A collection of attendance records.</returns>
    [HttpGet("get-all")]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }

    /// <summary>
    /// Retrieves Departmentwise list of attendance records.
    /// </summary>
    /// <returns>A collection of attendance records.</returns>
    [HttpGet("get-all-byDept")]
    public IEnumerable<string> Get(string deptCode)
    {
        return new string[] { "value1", "value2" };
    }

    /// <summary>
    /// Retrieves a specific attendance record by ID.
    /// </summary>
    /// <param name="id">The ID of the attendance record.</param>
    /// <returns>The attendance record.</returns>
    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }

    /// <summary>
    /// Creates a new attendance record.
    /// </summary>
    /// <param name="newAttendanceCommand">The attendance record to create.</param>
    [HttpPost]
    public async Task Post([FromBody] CreateAttendanceCommand newAttendanceCommand)
    {
        var result = await _mediatR.Send(newAttendanceCommand);
        if (result != null)
        {
            _loggingService.LogActivity(LogActivityAction.Insert.ToString(), $"New Attendance for EmployeeId={result.EmployeeId} added", null);
        }
    }

    /// <summary>
    /// Updates an existing attendance record by ID.
    /// </summary>
    /// <param name="id">The ID of the attendance record to update.</param>
    /// <param name="value">The updated attendance record.</param>
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    /// <summary>
    /// Deletes an attendance record by ID.
    /// </summary>
    /// <param name="id">The ID of the attendance record to delete.</param>
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}
