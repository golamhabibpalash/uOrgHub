using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OrgHub.Api.Areas.HRM.Controllers
{
    /// <summary>
    /// Controller for managing recruitment-related operations.
    /// </summary>
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("HRM")]
    public class RecruitmentsController : ControllerBase
    {
        /// <summary>
        /// Retrieves a list of recruitment values.
        /// </summary>
        /// <returns>A collection of recruitment values.</returns>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Retrieves a specific recruitment value by ID.
        /// </summary>
        /// <param name="id">The ID of the recruitment value.</param>
        /// <returns>The recruitment value.</returns>
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// Creates a new recruitment value.
        /// </summary>
        /// <param name="value">The recruitment value to create.</param>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        /// <summary>
        /// Updates an existing recruitment value by ID.
        /// </summary>
        /// <param name="id">The ID of the recruitment value to update.</param>
        /// <param name="value">The updated recruitment value.</param>
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        /// <summary>
        /// Deletes a recruitment value by ID.
        /// </summary>
        /// <param name="id">The ID of the recruitment value to delete.</param>
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
