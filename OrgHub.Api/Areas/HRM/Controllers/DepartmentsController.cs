using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OrgHub.Api.Areas.HRM.Controllers;

/// <summary>
/// Controller for managing department-related operations in the HRM area.
/// </summary>
[Route("api/[area]/[controller]")]
[ApiController]
[Area("HRM")]
public class DepartmentsController : ControllerBase
{
    /// <summary>
    /// Retrieves a list of department values.
    /// </summary>
    /// <returns>A collection of department values.</returns>
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }

    /// <summary>
    /// Retrieves a specific department value by its ID.
    /// </summary>
    /// <param name="id">The ID of the department.</param>
    /// <returns>The department value.</returns>
    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }

    /// <summary>
    /// Creates a new department value.
    /// </summary>
    /// <param name="value">The department value to create.</param>
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    /// <summary>
    /// Updates an existing department value by its ID.
    /// </summary>
    /// <param name="id">The ID of the department to update.</param>
    /// <param name="value">The updated department value.</param>
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    /// <summary>
    /// Deletes a specific department value by its ID.
    /// </summary>
    /// <param name="id">The ID of the department to delete.</param>
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}
