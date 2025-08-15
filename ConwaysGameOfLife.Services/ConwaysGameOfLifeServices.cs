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

            // Update the livePoints set with the newly counted live points.
            livePoints = updatedLivePoints;

            // Process dead neighbours to count their live neighbours.
            var updatedDeadNeighbours = new HashSet<BoardPoint>();

            foreach (var deadNeighbor in deadNeighbours.ToList())
            {
                var updatedDeadNeighbour = deadNeighbor;
                updatedDeadNeighbour.LiveNeighbours = CountNeighboursForPoint(deadNeighbor);
                updatedDeadNeighbours.Add(updatedDeadNeighbour);
            }

            // Update the deadNeighbours set with the newly counted dead neighbours.
            deadNeighbours = updatedDeadNeighbours;
        }
        
        private void TransitionOnce()
        {
            CountNeighbours();

            // Clean out livePoints based on the rules of Conway's Game of Life.

            // 1. Any live cell with fewer than two live neighbours dies, as if by underpopulation.
            livePoints.RemoveWhere(p => p.LiveNeighbours < 2);

            // 2. Any live cell with two or three live neighbours lives on to the next generation.
            // 3. Any live cell with more than three live neighbours dies, as if by overpopulation.
            livePoints.RemoveWhere(p => p.LiveNeighbours > 3);

            // 4. Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.
            foreach (var deadNeighbor in deadNeighbours)
            {
                if (deadNeighbor.LiveNeighbours == 3)
                {
                    livePoints.Add(deadNeighbor);
                }
            }
        }

        /// <summary>
        /// Simulates the evolution of the game board for a specified number of iterations and updates the database with the resulting state.
        /// </summary>
        /// <remarks>
        /// If there are no live points remaining during the simulation, the process will terminate early.
        /// The database is updated to reflect the final state of live points for the specified board.
        /// </remarks>
        /// <param name="boardId">The unique identifier of the game board to update.</param>
        /// <param name="iterations">The number of iterations to simulate. Must be a non-negative integer.</param>
        /// <returns>A list of <see cref="Point"/> objects representing the coordinates of all live points after the simulation.</returns>
        public List<Point> Transition(Guid boardId, uint iterations)
        {
            for (uint i = 0; i < iterations; i++)
            {
                // If there are no live points left, we can stop the simulation early.
                if (livePoints.Count == 0)
                    break;

                TransitionOnce();
            }

            // Update the database with the new state of live points.
            var livePointEntities = livePoints.Select(p => new LivePoint
            {
                BoardId = boardId, // Use the provided boardId
                X = p.X,
                Y = p.Y
            }).ToList();

            // Remove existing live points for the specified boardId before adding new ones.
            conwaysGameOfLifeApiDbContext.LivePoints.RemoveRange(conwaysGameOfLifeApiDbContext.LivePoints.Where(x => x.BoardId == boardId));

            conwaysGameOfLifeApiDbContext.LivePoints.AddRange(livePointEntities);

            conwaysGameOfLifeApiDbContext.SaveChanges();

            return livePoints.Select(p => new Point(p.X, p.Y)).ToList();
        }
    }
}
