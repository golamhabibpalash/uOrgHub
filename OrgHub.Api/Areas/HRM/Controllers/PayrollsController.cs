using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OrgHub.Api.Areas.HRM.Controllers
{
    /// <summary>
    /// Controller for managing payroll-related operations.
    /// </summary>
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("HRM")]
    public class PayrollsController : ControllerBase
    {
        /// <summary>
        /// Retrieves a list of payroll values.
        /// </summary>
        /// <returns>A collection of payroll values.</returns>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Retrieves a specific payroll value by ID.
        /// </summary>
        /// <param name="id">The ID of the payroll value to retrieve.</param>
        /// <returns>The payroll value.</returns>
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// Creates a new payroll value.
        /// </summary>
        /// <param name="value">The payroll value to create.</param>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        /// <summary>
        /// Updates an existing payroll value by ID.
        /// </summary>
        /// <param name="id">The ID of the payroll value to update.</param>
        /// <param name="value">The updated payroll value.</param>
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        /// <summary>
        /// Deletes a payroll value by ID.
        /// </summary>
        /// <param name="id">The ID of the payroll value to delete.</param>
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
