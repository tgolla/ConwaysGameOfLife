namespace ConwaysGameOfLife.Services
{
    /// <summary>
    /// Defines the contract for Conway's Game of Life services.
    /// </summary>
    public interface IConwaysGameOfLifeService
    {
        /// <summary>
        /// Initializes (seeds) the set of live points for the simulation.
        /// </summary>
        /// <param name="livePoints">A list of points representing the initial live points for the board.</param>
        /// <returns>The unique identifier of the created board.</returns>
        Guid Seed(List<Point> livePoints);

        /// <summary>
        /// Simulates the evolution of the game board for a specified number of iterations and updates the database with the resulting state.
        /// </summary>
        /// <param name="boardId">The unique identifier of the game board to update.</param>
        /// <param name="iterations">The number of iterations to simulate. Must be a non-negative integer.</param>
        /// <returns>A list of <see cref="Point"/> objects representing the coordinates of all live points after the simulation.</returns>
        List<Point> Transition(Guid boardId, uint iterations);

        /// <summary>
        /// Ends the current game session for the specified board and retrieves the final positions of all points.
        /// </summary>
        /// <param name="boardId">The unique identifier of the board whose game session is to be ended.</param>
        /// <returns>A list of <see cref="Point"/> objects representing the final positions on the board. The list will be empty if no points are present.</returns>
        List<Point> End(Guid boardId);        
    }
}
