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
        private HashSet<BoardPoint> deadNeighbours = new HashSet<BoardPoint>();

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

        /// <summary>
        /// Counts the number of live neighboring cells surrounding the specified board point.
        /// </summary>
        /// <remarks>
        /// A neighbor is any of the eight cells immediately adjacent to the specified point,
        /// including diagonals. The specified point itself is not considered a neighbor.
        /// </remarks>
        /// <param name="boardPoint">The point on the board for which to count live neighbors.</param>
        /// <returns>The number of live neighbors surrounding the specified board point.</returns>
        private int CountNeighboursForPoint(BoardPoint boardPoint)
        {
            int liveNeighbours = 0;

            // Check all 8 possible neighbours.
            for (int x = boardPoint.X - 1; x <= boardPoint.X + 1; x++)
            {
                for (int y = boardPoint.Y - 1; y <= boardPoint.Y + 1; y++)
                {
                    if (x == boardPoint.X && y == boardPoint.Y) continue; // Skip the current cell.

                    var neighbor = new BoardPoint(x, y);
                    if (livePoints.Contains(neighbor))
                    {
                        liveNeighbours++;
                    }
                    else
                    {
                        deadNeighbours.Add(neighbor);
                    }
                }
            }

            return liveNeighbours;
        }

        /// <summary>
        /// Updates the live neighbor counts for all live and dead points on the board.
        /// </summary>
        /// <remarks>
        /// This method recalculates the number of live neighbors for each point in the current
        /// set of live points and their adjacent dead neighbors. The results are used to update the state of the
        /// board. It should only be called once before applying the game rules.
        /// </remarks>
        private void CountNeighbours()
        {
            // Reset deadNeighbours before counting to avoid accumulating from previous calls.
            deadNeighbours.Clear();

            // Count neighbours for all live points.
            var updatedLivePoints = new HashSet<BoardPoint>();

            foreach (var livePoint in livePoints)
            {
                var updatedLivePoint = livePoint;
                updatedLivePoint.LiveNeighbours = CountNeighboursForPoint(livePoint);
                updatedLivePoints.Add(updatedLivePoint);
            }

            // Process dead neighbours to count their live neighbours.
            foreach (var deadNeighbor in deadNeighbours.ToList())
            {
                var updatedLivePoint = deadNeighbor;
                updatedLivePoint.LiveNeighbours = CountNeighboursForPoint(deadNeighbor);
                updatedLivePoints.Add(updatedLivePoint);
            }

            livePoints = updatedLivePoints;
        }

        public List<Point> Transition(Guid boardId)
        {
            CountNeighbours();

            throw new NotImplementedException("Transition logic is not implemented yet. This method should apply the rules of Conway's Game of Life to update the state of livePoints based on their neighbours.");
        }
    }
}
