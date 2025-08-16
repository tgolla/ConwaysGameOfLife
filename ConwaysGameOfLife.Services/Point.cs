using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConwaysGameOfLife.Services
{
    /// <summary>
    /// Represents a point in a two-dimensional Cartesian coordinate system.
    /// </summary>
    /// <remarks>A <see cref="Point"/> is defined by its X and Y coordinates, which are immutable after the
    /// object is created. This class is commonly used to represent positions or vectors in 2D space.</remarks>
    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
