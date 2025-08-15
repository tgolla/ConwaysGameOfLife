using ConwaysGameOfLife.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Drawing;

namespace ConwaysGameOfLife.Services
{
    /// <summary>
    /// Provides services and utilities for implementing and managing Conway's Game of Life simulations.
    /// </summary>
    /// <remarks>
    /// This class serves as a central point for operations related to Conway's Game of Life, such as initializing
    /// game states (Seed), applying game rules (Transition), and managing the simulation lifecycle (End). Use this
    /// class to interact with and manipulate the game grid and its evolution over time. 
    /// </remarks>
    public class ConwaysGameOfLifeServices
    {
        private readonly ConwaysGameOfLifeApiDbContext conwaysGameOfLifeApiDbContext;

        private HashSet<BoardPoint> livePoints;
        private HashSet<BoardPoint> deadNeighbors = new HashSet<BoardPoint>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConwaysGameOfLifeServices"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor sets up the necessary services for managing and simulating Conway's Game of Life.
        /// </remarks>
        public ConwaysGameOfLifeServices(ILogger<ConwaysGameOfLifeServices> logger, IConfiguration configuration, ConwaysGameOfLifeApiDbContext conwaysGameOfLifeApiDbContext)
        {
            this.conwaysGameOfLifeApiDbContext = conwaysGameOfLifeApiDbContext; // Fix: assign parameter to field
            livePoints = new HashSet<BoardPoint>();
        }

        /// <summary>
        /// Initializes (seeds) the set of live points for the simulation.
        /// </summary>
        /// <param name="livePoints">A list of points representing the initial live points for the board.</param>
        /// <returns>The unique identifier of the created board.</returns>

        public Guid Seed(List<Point> livePoints)
        {
            this.livePoints = livePoints
                .Select(p => new BoardPoint(p.X, p.Y))
                .ToHashSet(); // Convert Point to BoardPoint and eliminate duplicates.

            // Create and save the board.
            Guid boardId = Guid.NewGuid();

            var board = new Board
            {
                Id = boardId,
                Expires = DateTime.UtcNow
            };

            conwaysGameOfLifeApiDbContext.Boards.Add(board);

            var livePointEntities = this.livePoints.Select(p => new LivePoint
            {
                BoardId = boardId,
                X = p.X,
                Y = p.Y
            }).ToList();

            conwaysGameOfLifeApiDbContext.LivePoints.AddRange(livePointEntities);

            conwaysGameOfLifeApiDbContext.SaveChanges();

            return boardId;
        }
    }
}
