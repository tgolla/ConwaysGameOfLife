using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConwaysGameOfLife.Data
{
    /// <summary>
    /// Conway's Game of Life API Live point model.
    /// </summary>
    /// <remarks>
    /// This entity represents a live point in the Game of Life.
    /// </remarks>
    public class LivePoint
    {
        /// <summary>
        /// The board Id of the live point.
        /// </summary>
        public Guid BoardId { get; set; }

        /// <summary>
        /// Represents the x-axis coordinate of the live point.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Represents the y-axis coordinate of the live point.
        /// </summary>
        public int Y { get; set; }
    }
}
