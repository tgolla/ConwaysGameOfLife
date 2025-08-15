using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConwaysGameOfLife.Data
{
    /// <summary>
    /// Conway's Game of Life API board model.
    /// </summary>
    /// <remarks>
    /// This entity represents a board (grid) in the Game of Life.
    /// </remarks>
    public class Board
    {
        /// <summary>
        /// The unique identifier for the board.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The date/time when the board will expire. This is used to clean up old boards that we abandoned.
        /// </summary>
        public DateTime Expires { get; set; }
    }
}
