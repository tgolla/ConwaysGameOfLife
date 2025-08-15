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
    /// <remarks>The <see cref="BoardPoint"/> structure is immutable and provides methods for comparing points and
    /// obtaining their string representation. Instances of <see cref="BoardPoint"/> are defined by their <see cref="X"/> and
    /// <see cref="Y"/> coordinates.</remarks>
    public class BoardPoint : IEquatable<BoardPoint>
    {
        public int X { get; }
        public int Y { get; }
        public int Neighbours { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoardPoint"/> class with the specified coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate of the point.</param>
        /// <param name="y">The y-coordinate of the point.</param>
        public BoardPoint(int x, int y)
        {
            X = x;
            Y = y;
            Neighbours = 0;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        public bool Equals(BoardPoint other) => X == other.X && Y == other.Y;
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="BoardPoint"/> instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified object is a <see cref="BoardPoint"/> and has the same coordinates; otherwise, <c>false</c>.</returns>
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override bool Equals(object obj) => obj is BoardPoint other && Equals(other);
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).

        /// <summary>
        /// Returns a hash code for this <see cref="BoardPoint"/> instance, based on its <see cref="X"/> and <see cref="Y"/> coordinates.
        /// </summary>
        public override int GetHashCode() => HashCode.Combine(X, Y);

        /// <summary>
        /// Returns a string that represents the current <see cref="BoardPoint"/> in the format "(X, Y)".
        /// </summary>
        public override string ToString() => $"({X}, {Y})";
    }
}
