using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OrgHub.Api.Areas.HRM.Controllers
{
    /// <summary>
    /// Controller for managing holiday-related operations in the HRM area.
    /// </summary>
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("HRM")]
    public class HolidaysController : ControllerBase
    {
        /// <summary>
        /// Retrieves a list of holiday values.
        /// </summary>
        /// <returns>A collection of holiday values.</returns>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Retrieves a specific holiday value by its ID.
        /// </summary>
        /// <param name="id">The ID of the holiday.</param>
        /// <returns>The holiday value.</returns>
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// Creates a new holiday value.
        /// </summary>
        /// <param name="value">The holiday value to create.</param>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        /// <summary>
        /// Updates an existing holiday value by its ID.
        /// </summary>
        /// <param name="id">The ID of the holiday to update.</param>
        /// <param name="value">The updated holiday value.</param>
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        /// <summary>
        /// Deletes a specific holiday value by its ID.
        /// </summary>
        /// <param name="id">The ID of the holiday to delete.</param>
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
