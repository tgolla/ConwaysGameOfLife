using System;
using System.Collections.Generic;
using System.Drawing;

namespace ConwaysGameOfLife.Services
{
    /// <summary>
    /// Defines the contract for Conway's Game of Life services.
    /// </summary>
    public interface IConwaysGameOfLifeServices
    {
        /// <summary>
        /// Initializes (seeds) the set of live points for the simulation.
        /// </summary>
        /// <param name="livePoints">A list of points representing the initial live points for the board.</param>
        /// <returns>The unique identifier of the created board.</returns>
        Guid Seed(List<Point> livePoints);
    }
}
