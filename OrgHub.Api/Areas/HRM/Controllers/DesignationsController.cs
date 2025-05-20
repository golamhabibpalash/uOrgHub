using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OrgHub.Api.Areas.HRM.Controllers
{
    /// <summary>
    /// Controller for managing designations.
    /// </summary>
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("HRM")]
    public class DesignationsController : ControllerBase
    {
        /// <summary>
        /// Gets a list of designations.
        /// </summary>
        /// <returns>A list of designation names.</returns>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Gets a specific designation by ID.
        /// </summary>
        /// <param name="id">The ID of the designation.</param>
        /// <returns>The name of the designation.</returns>
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// Creates a new designation.
        /// </summary>
        /// <param name="value">The name of the new designation.</param>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        /// <summary>
        /// Updates an existing designation.
        /// </summary>
        /// <param name="id">The ID of the designation to update.</param>
        /// <param name="value">The new name of the designation.</param>
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        /// <summary>
        /// Deletes a designation by ID.
        /// </summary>
        /// <param name="id">The ID of the designation to delete.</param>
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
