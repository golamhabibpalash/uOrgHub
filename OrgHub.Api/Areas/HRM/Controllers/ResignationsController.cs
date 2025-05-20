using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OrgHub.Api.Areas.HRM.Controllers
{
    /// <summary>
    /// Controller for managing employee resignations.
    /// </summary>
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("HRM")]
    public class ResignationsController : ControllerBase
    {
        /// <summary>
        /// Retrieves a list of resignation records.
        /// </summary>
        /// <returns>A collection of resignation records.</returns>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Retrieves a specific resignation record by ID.
        /// </summary>
        /// <param name="id">The ID of the resignation record.</param>
        /// <returns>The resignation record.</returns>
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// Creates a new resignation record.
        /// </summary>
        /// <param name="value">The resignation record data.</param>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        /// <summary>
        /// Updates an existing resignation record by ID.
        /// </summary>
        /// <param name="id">The ID of the resignation record.</param>
        /// <param name="value">The updated resignation record data.</param>
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        /// <summary>
        /// Deletes a resignation record by ID.
        /// </summary>
        /// <param name="id">The ID of the resignation record.</param>
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
