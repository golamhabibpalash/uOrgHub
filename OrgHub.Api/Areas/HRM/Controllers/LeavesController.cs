using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OrgHub.Api.Areas.HRM.Controllers
{
    /// <summary>
    /// Controller for managing leave-related operations.
    /// </summary>
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("HRM")]
    public class LeavesController : ControllerBase
    {
        /// <summary>
        /// Retrieves a list of leave records.
        /// </summary>
        /// <returns>A collection of leave records.</returns>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Retrieves a specific leave record by ID.
        /// </summary>
        /// <param name="id">The ID of the leave record.</param>
        /// <returns>The leave record.</returns>
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// Creates a new leave record.
        /// </summary>
        /// <param name="value">The leave record data.</param>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        /// <summary>
        /// Updates an existing leave record by ID.
        /// </summary>
        /// <param name="id">The ID of the leave record to update.</param>
        /// <param name="value">The updated leave record data.</param>
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        /// <summary>
        /// Deletes a leave record by ID.
        /// </summary>
        /// <param name="id">The ID of the leave record to delete.</param>
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
