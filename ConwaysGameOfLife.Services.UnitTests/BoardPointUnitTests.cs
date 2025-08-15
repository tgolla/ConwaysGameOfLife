using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConwaysGameOfLife.Services.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="BoardPoint"/> structure.
    /// </summary>
    /// <remarks>
    /// MethodName_StateUnderTest_ExpectedBehavior
    /// "AAA" Arrange, Act, and Assert
    /// </remarks>
    [TestFixture]
    public class BoardPointUnitTests
    {
        [Test]
        public void Constructor_ValidCoordinates_PropertiesAreSet()
        {
            // Arrange
            int x = 2;
            int y = 3;

            // Act
            var point = new BoardPoint(x, y);

            // Assert
            Assert.That(point.X, Is.EqualTo(x));
            Assert.That(point.Y, Is.EqualTo(y));
            Assert.That(point.Neighbours, Is.EqualTo(0));
        }

        [Test]
        public void Equals_SameCoordinates_ReturnsTrue()
        {
            var p1 = new BoardPoint(1, 1);
            var p2 = new BoardPoint(1, 1);

            Assert.That(p1.Equals(p2), Is.True);
        }

        [Test]
        public void Equals_DifferentCoordinates_ReturnsFalse()
        {
            var p1 = new BoardPoint(1, 2);
            var p2 = new BoardPoint(2, 1);

            Assert.That(p1.Equals(p2), Is.False);
        }

        [Test]
        public void GetHashCode_SameCoordinates_SameHashCode()
        {
            var p1 = new BoardPoint(5, 6);
            var p2 = new BoardPoint(5, 6);

            Assert.That(p1.GetHashCode(), Is.EqualTo(p2.GetHashCode()));
        }

        [Test]
        public void ToString_AnyCoordinates_ReturnsExpectedFormat()
        {
            var point = new BoardPoint(7, 8);

            Assert.That(point.ToString(), Is.EqualTo("(7, 8)"));
        }
    }
}
