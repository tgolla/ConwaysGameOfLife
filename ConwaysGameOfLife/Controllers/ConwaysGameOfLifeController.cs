using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ConwaysGameOfLife.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConwaysGameOfLifeController : ControllerBase
    {
        private readonly ILogger<ConwaysGameOfLifeController> logger;

        /// <summary>
        /// Initializes a new instance of ConwaysGameOfLifeController.
        /// </summary>
        /// <param name="logger">A generic interface for logging where the category name is derived from the specified TCategoryName type name used to enable activation of a named ILogger from dependency injection.</param>
        public ConwaysGameOfLifeController(ILogger<ConwaysGameOfLifeController> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Handles HTTP GET requests for this endpoint. This endpoint is not implemented yet.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> indicating that the endpoint is not implemented. The response has a status
        /// code of <see cref="HttpStatusCode.NotImplemented"/> (501) and includes a message stating that the endpoint
        /// is not implemented.
        /// </returns>
        [HttpGet]
        public IActionResult Get()
        {
            return StatusCode((int)HttpStatusCode.NotImplemented, "This endpoint is not implemented yet.");
        }

        /// <summary>
        /// Handles HTTP POST requests for this endpoint. This endpoint is not implemented yet.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> indicating that the endpoint is not implemented. The response has a status
        /// code of <see cref="HttpStatusCode.NotImplemented"/> (501) and includes a message stating that the endpoint
        /// is not implemented.
        /// </returns>
        [HttpPost]
        public IActionResult Post()
        {
            return StatusCode((int)HttpStatusCode.NotImplemented, "This endpoint is not implemented yet.");
        }

        /// <summary>
        /// Handles HTTP DELETE requests for this endpoint. This endpoint is not implemented yet.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> indicating that the endpoint is not implemented. The response has a status
        /// code of <see cref="HttpStatusCode.NotImplemented"/> (501) and includes a message stating that the endpoint
        /// is not implemented.
        /// </returns>
        [HttpDelete]
        public IActionResult Delete()
        {
            return StatusCode((int)HttpStatusCode.NotImplemented, "This endpoint is not implemented yet.");
        }
    }
}
