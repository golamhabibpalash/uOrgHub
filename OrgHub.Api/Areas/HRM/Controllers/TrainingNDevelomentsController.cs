using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OrgHub.Api.Areas.HRM.Controllers
{
    /// <summary>
    /// Controller for managing Training and Development.
    /// </summary>
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("HRM")]
    public class TrainingNDevelomentsController : ControllerBase
    {
        /// <summary>
        /// Retrieves a list of Training and Development.
        /// </summary>
        /// <returns>A list of strings representing Training and Development.</returns>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Retrieves a specific Training and Development by ID.
        /// </summary>
        /// <param name="id">The ID of the Training and Development.</param>
        /// <returns>A string representing the Training and Development.</returns>
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// Creates a new Training and Development.
        /// </summary>
        /// <param name="value">The value of the Training and Development to create.</param>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        /// <summary>
        /// Updates an existing Training and Development by ID.
        /// </summary>
        /// <param name="id">The ID of the Training and Development to update.</param>
        /// <param name="value">The new value of the Training and Development.</param>
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        /// <summary>
        /// Deletes a specific Training and Development by ID.
        /// </summary>
        /// <param name="id">The ID of the Training and Development to delete.</param>
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
