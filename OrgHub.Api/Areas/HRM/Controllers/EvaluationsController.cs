using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OrgHub.Api.Areas.HRM.Controllers
{
    /// <summary>
    /// Controller for managing evaluations.
    /// </summary>
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("HRM")]
    public class EvaluationsController : ControllerBase
    {
        /// <summary>
        /// Retrieves a list of evaluation values.
        /// </summary>
        /// <returns>A list of string values.</returns>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Retrieves a specific evaluation value by ID.
        /// </summary>
        /// <param name="id">The ID of the evaluation.</param>
        /// <returns>A string value representing the evaluation.</returns>
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// Creates a new evaluation.
        /// </summary>
        /// <param name="value">The value of the evaluation to create.</param>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        /// <summary>
        /// Updates an existing evaluation by ID.
        /// </summary>
        /// <param name="id">The ID of the evaluation to update.</param>
        /// <param name="value">The new value of the evaluation.</param>
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        /// <summary>
        /// Deletes an evaluation by ID.
        /// </summary>
        /// <param name="id">The ID of the evaluation to delete.</param>
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
