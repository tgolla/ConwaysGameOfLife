using ConwaysGameOfLife.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ConwaysGameOfLife.Controllers
{
    /// <summary>
    /// Conway's Game of Life API calls.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ConwaysGameOfLifeController : ControllerBase
    {
        private readonly ILogger<ConwaysGameOfLifeController> logger;
        private readonly IConwaysGameOfLifeService conwaysGameOfLifeService;

        /// <summary>
        /// Initializes a new instance of ConwaysGameOfLifeController.
        /// </summary>
        /// <param name="logger">A generic interface for logging where the category name is derived from the specified TCategoryName type name used to enable activation of a named ILogger from dependency injection.</param>
        /// <param name="conwaysGameOfLifeService">The Conway's Game of Life service.</param>
        public ConwaysGameOfLifeController(ILogger<ConwaysGameOfLifeController> logger, IConwaysGameOfLifeService conwaysGameOfLifeService)
        {
            this.logger = logger;
            this.conwaysGameOfLifeService = conwaysGameOfLifeService;
        }

        /// <summary>
        /// Creates a new game board and seeds it with the specified live points.
        /// </summary>
        /// <remarks>
        /// This method initializes a new game board for Conway's Game of Life and seeds it with
        /// the provided live points. The board id of the created game board can be used to retrieve or 
        /// interact with the board in subsequent operations.
        /// </remarks>
        /// <param name="livePoints">A list of points representing the initial live cells on the game board. Cannot be null or empty.</param>
        /// <returns>
        /// Returns a <see cref="CreatedAtActionResult"/> containing the ID of the newly created game board if the
        /// operation is successful. Returns a <see cref="BadRequestObjectResult"/> if <paramref name="livePoints"/> is
        /// null or empty. Returns a <see cref="StatusCodeResult"/> with a status code of 500 if an internal error occurs.
        /// </returns>
        [HttpPost]
        public IActionResult Post([FromBody] List<Point> livePoints)
        {
            if (livePoints == null || !livePoints.Any())
                return BadRequest("Live points cannot be null or empty.");

            try
            {
                var boardId = conwaysGameOfLifeService.Seed(livePoints);
                return CreatedAtAction(nameof(Get), new { boardId }, new { boardId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error seeding live points.");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while seeding live points.");
            }
        }

        /// <summary>
        /// Retrieves the state of a game board after applying the specified number of iterations.
        /// </summary>
        /// <remarks>
        /// This method transitions the state of the specified game board through the given
        /// number of iterations using Conway's Game of Life rules. 
        /// </remarks>
        /// <param name="boardId">The unique identifier of the game board to retrieve.</param>
        /// <param name="iterations">The number of iterations to apply to the game board's state. Optional</param>
        /// <returns>An <see cref="IActionResult"/> containing the updated state of the game board if successful,  or an error response if the operation fails.</returns>
        [HttpGet]
        public IActionResult Get([FromQuery] Guid boardId, [FromQuery] uint iterations = 1)
        {
            try
            {
                var board = conwaysGameOfLifeService.Transition(boardId, iterations);
                return Ok(board);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error transitioning game board to next generation.");
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.ToString());
            }
        }


        [HttpDelete]
        public IActionResult Delete([FromQuery] Guid boardId)
        {
            try
            {
                var finalPoints = conwaysGameOfLifeService.End(boardId);
                return Ok(finalPoints);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error ending game session.");
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.ToString());
            }
        }
    }
}
