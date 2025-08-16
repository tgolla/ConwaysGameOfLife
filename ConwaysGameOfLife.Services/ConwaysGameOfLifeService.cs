using ConwaysGameOfLife.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
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
    public class ConwaysGameOfLifeService
    {
        private readonly ConwaysGameOfLifeSettings conwaysGameOfLifeSettings;
        private readonly ConwaysGameOfLifeApiDbContext conwaysGameOfLifeApiDbContext;

        private HashSet<BoardPoint> livePoints;
        private HashSet<BoardPoint> deadNeighbours = new HashSet<BoardPoint>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConwaysGameOfLifeService"/> class.
        /// </summary>
        /// <param name="logger">The logger used to log diagnostic and operational messages.</param>
        /// <param name="configuration">The configuration object used to retrieve application settings.</param>
        /// <param name="conwaysGameOfLifeApiDbContext">The database context for accessing Conway's Game of Life data.</param>
        public ConwaysGameOfLifeService(ILogger<ConwaysGameOfLifeService> logger, IConfiguration configuration, ConwaysGameOfLifeApiDbContext conwaysGameOfLifeApiDbContext)
        {
            conwaysGameOfLifeSettings = configuration.GetSection("ConwaysGameOfLifeSettings").Get<ConwaysGameOfLifeSettings>()
                ?? throw new InvalidOperationException("ConwaysGameOfLifeSettings section is missing or invalid in configuration.");

            this.conwaysGameOfLifeApiDbContext = conwaysGameOfLifeApiDbContext;
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
        /// Simulates the evolution of the game board for a specified number of periods and updates the database with the resulting state.
        /// </summary>
        /// <remarks>
        /// If there are no live points remaining during the simulation, the process will terminate early.
        /// The database is updated to reflect the final state of live points for the specified board.
        /// </remarks>
        /// <param name="boardId">The unique identifier of the game board to update.</param>
        /// <param name="iterations">The number of periods to simulate. Must be a non-negative integer.</param>
        /// <returns>A list of <see cref="Point"/> objects representing the coordinates of all live points after the simulation.</returns>
        public List<Point> Transition(Guid boardId, uint iterations)
        {
            // Get the current live points for the specified boardId.       
            livePoints = conwaysGameOfLifeApiDbContext.LivePoints
                .Where(lp => lp.BoardId == boardId)
                .Select(lp => new BoardPoint(lp.X, lp.Y))
                .ToHashSet();

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

        /// <summary>
        /// Ends the simulation for the specified board and returns the final set of live points.
        /// </summary>
        /// <remarks>
        /// This method simulates the evolution of the board until one of the following
        /// conditions is met: 
        /// <list type="bullet"> 
        /// <item><description>The population stabilizes or cycles for a predefined number of iterations.</description></item>
        /// <item><description>All live points are eliminated.</description></item>
        /// <item><description>The maximum generation limit is reached.</description></item>
        /// </list> 
        /// If the maximum generation limit is reached without stabilization or cycling, an exception is thrown.
        /// The method also removes the board and its associated live points from the database.
        /// </remarks>
        /// <param name="boardId">The unique identifier of the board to end the simulation for.</param>
        /// <returns>A list of <see cref="Point"/> objects representing the final set of live points on the board. If no live points remain, the list will be empty.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the maximum generation limit is reached before the simulation stabilizes or cycles.</exception>
        public List<Point> End(Guid boardId)
        {
            // Get the current live points for the specified boardId.       
            livePoints = conwaysGameOfLifeApiDbContext.LivePoints
                .Where(lp => lp.BoardId == boardId)
                .Select(lp => new BoardPoint(lp.X, lp.Y))
                .ToHashSet();

            bool maximunGenerationReached = true; // Determines if an error should be throw.
            Dictionary<int, uint> previousPopulation = new();
            Dictionary<int, uint> stablePopulationIterations = new();

            for (uint i = 0; i < conwaysGameOfLifeSettings.MaximunGenerationBeforeEnding; i++)
            {
                // If there are no live points left, we can stop the simulation early.
                if (livePoints.Count == 0)
                {
                    maximunGenerationReached = false; // Simulation ended without reaching the maximum generation.
                    break;
                }

                TransitionOnce();
                                
                // Test for a stable population over a set number of periods.
                var periods = new[] { 1, 2, 3, 4, 8, 14, 15, 30 };
                bool shouldBreakOutOfIterations = false;

                foreach (var period in periods)
                {
                    if (i % period == 0)
                    {
                        if (previousPopulation.TryGetValue(period, out var previousPopulationCount) && livePoints.Count == previousPopulationCount)
                        {
                            stablePopulationIterations[period]++;
                            if (stablePopulationIterations[period] >= conwaysGameOfLifeSettings.StablePopulationIterations)
                            {
                                // Population is stable, we can end the simulation.
                                maximunGenerationReached = false;
                                shouldBreakOutOfIterations = true;
                                break;
                            }
                        }
                        else
                        {
                            previousPopulation[period] = (uint)livePoints.Count;
                            stablePopulationIterations[period] = 0; // Reset if the population changed.
                        }
                    }
                }

                if (shouldBreakOutOfIterations)
                    break;
            }

            // Clear the live points from the database for the specified boardId.
            conwaysGameOfLifeApiDbContext.LivePoints.RemoveRange(conwaysGameOfLifeApiDbContext.LivePoints.Where(x => x.BoardId == boardId));

            conwaysGameOfLifeApiDbContext.SaveChanges();

            // Remove the board from the database.
            var board = conwaysGameOfLifeApiDbContext.Boards.Find(boardId);

            if (board != null)
            {
                conwaysGameOfLifeApiDbContext.Boards.Remove(board);
                conwaysGameOfLifeApiDbContext.SaveChanges();
            }

            if (maximunGenerationReached)
                throw new InvalidOperationException($"The maximum generation limit of {conwaysGameOfLifeSettings.MaximunGenerationBeforeEnding} was reached before the game could stabilize or cycle.");

            return livePoints.Select(p => new Point(p.X, p.Y)).ToList();
        }
    }
}
