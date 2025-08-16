using ConwaysGameOfLife.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Drawing;
using System.Reflection;

namespace ConwaysGameOfLife.Services.UnitTests
{
    /// <summary>
    /// Unit tests for the services related to Conway's Game of Life.
    /// </summary>
    /// <remarks>
    /// MethodName_StateUnderTest_ExpectedBehavior
    /// "AAA" Arrange, Act, and Assert
    /// </remarks>
    [TestFixture]
    public class ConwaysGameOfLifeServicesUnitTests
    {
        /// <summary>
        /// Creates a new instance of <see cref="ConwaysGameOfLifeApiDbContext"/> configured to use an in-memory
        /// database.
        /// </summary>
        /// <remarks>
        /// This method is intended for testing purposes and initializes the database with a unique name to ensure isolation between tests.
        /// </remarks>
        /// <returns>A new instance of <see cref="ConwaysGameOfLifeApiDbContext"/> backed by an in-memory database.</returns>
        private ConwaysGameOfLifeApiDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ConwaysGameOfLifeApiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ConwaysGameOfLifeApiDbContext(options);
        }

        [Test]
        public void Seed_WithValidLivePoints_CreatesBoardAndLivePoints()
        {
            // Arrange
            var conwaysGameOfLifeDbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeServices>>();
            var configurationMock = new Mock<IConfiguration>();
            var conwaysGameOfLifeServicesInstance = new ConwaysGameOfLifeServices(loggerMock.Object, configurationMock.Object, conwaysGameOfLifeDbContext);

            var livePoints = new List<Point>
            {
                new Point(1, 2),
                new Point(3, 4),
                new Point(5, 6),
                new Point(1, 2) // Duplicate point to test deduplication.
            };

            // Act
            var boardId = conwaysGameOfLifeServicesInstance.Seed(livePoints);

            // Assert
            var board = conwaysGameOfLifeDbContext.Boards.SingleOrDefault(b => b.Id == boardId);
            Assert.IsNotNull(board, "Board should be created in the database.");

            var storedLivePoints = conwaysGameOfLifeDbContext.LivePoints.Where(lp => lp.BoardId == boardId).ToList();
            Assert.That(storedLivePoints.Count, Is.EqualTo(3), "All live points should be stored.");

            Assert.IsTrue(storedLivePoints.Any(lp => lp.X == 1 && lp.Y == 2));
            Assert.IsTrue(storedLivePoints.Any(lp => lp.X == 3 && lp.Y == 4));
            Assert.IsTrue(storedLivePoints.Any(lp => lp.X == 5 && lp.Y == 6));
        }

        [Test]
        public void CountNeighboursForPoint_LivePointWithTwoLiveNeighbours_SetsLiveNeighboursTo2()
        {
            // Arrange
            var conwaysGameOfLifeDbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeServices>>();
            var configurationMock = new Mock<IConfiguration>();
            var conwaysGameOfLifeServicesInstance = new ConwaysGameOfLifeServices(loggerMock.Object, configurationMock.Object, conwaysGameOfLifeDbContext);

            var livePoints = new List<Point>
            {
                new Point(1, 1),
                new Point(1, 2),
                new Point(2, 1)
            };
            conwaysGameOfLifeServicesInstance.Seed(livePoints);

            // Use reflection to access the private method
            var boardPoint = new BoardPoint(1, 1);
            var method = typeof(ConwaysGameOfLifeServices)
                .GetMethod("CountNeighboursForPoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            Assert.IsNotNull(method, "CountNeighboursForPoint method should not be null.");
            var result = method.Invoke(conwaysGameOfLifeServicesInstance, new object[] { boardPoint });
            Assert.IsNotNull(result, "CountNeighboursForPoint should not return null.");
            int liveNeighbours = (int)result;

            // Assert
            Assert.That(liveNeighbours, Is.EqualTo(2));
        }

        [Test]
        public void CountNeighboursForPoint_LivePointWithNoLiveNeighbours_SetsLiveNeighboursTo0()
        {
            // Arrange
            var conwaysGameOfLifeDbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeServices>>();
            var configurationMock = new Mock<IConfiguration>();
            var conwaysGameOfLifeServicesInstance = new ConwaysGameOfLifeServices(loggerMock.Object, configurationMock.Object, conwaysGameOfLifeDbContext);

            var livePoints = new List<Point>
            {
                new Point(5, 5)
            };
            conwaysGameOfLifeServicesInstance.Seed(livePoints);

            var boardPoint = new BoardPoint(5, 5);
            var method = typeof(ConwaysGameOfLifeServices)
                .GetMethod("CountNeighboursForPoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            Assert.IsNotNull(method, "CountNeighboursForPoint method should not be null.");
            var result = method.Invoke(conwaysGameOfLifeServicesInstance, new object[] { boardPoint });
            Assert.IsNotNull(result, "CountNeighboursForPoint should not return null.");
            int liveNeighbours = (int)result;

            // Assert
            Assert.That(liveNeighbours, Is.EqualTo(0));
        }

        [Test]
        public void CountNeighboursForPoint_LivePointWithEightLiveNeighbours_SetsLiveNeighboursTo8()
        {
            // Arrange
            var conwaysGameOfLifeDbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeServices>>();
            var configurationMock = new Mock<IConfiguration>();
            var conwaysGameOfLifeServicesInstance = new ConwaysGameOfLifeServices(loggerMock.Object, configurationMock.Object, conwaysGameOfLifeDbContext);

            var center = new Point(0, 0);
            var neighbours = new List<Point>
            {
                new Point(-1, -1), 
                new Point(-1, 0), 
                new Point(-1, 1),
                new Point(0, -1),    
                new Point(0, 1),
                new Point(1, -1),  
                new Point(1, 0), 
                new Point(1, 1)
            };
            var livePoints = new List<Point>(neighbours) { center };
            conwaysGameOfLifeServicesInstance.Seed(livePoints);

            var boardPoint = new BoardPoint(0, 0);
            var method = typeof(ConwaysGameOfLifeServices)
                .GetMethod("CountNeighboursForPoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            Assert.IsNotNull(method, "CountNeighboursForPoint method should not be null.");
            var result = method.Invoke(conwaysGameOfLifeServicesInstance, new object[] { boardPoint });
            Assert.IsNotNull(result, "CountNeighboursForPoint should not return null.");
            int liveNeighbours = (int)result;

            // Assert
            Assert.That(liveNeighbours, Is.EqualTo(8));
        }

        [Test]
        public void CountNeighbours_LivePointsWithKnownNeighbours_CorrectLiveNeighboursCount()
        {
            // Arrange
            var conwaysGameOfLifeDbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeServices>>();
            var configurationMock = new Mock<IConfiguration>();
            var conwaysGameOfLifeServicesInstance = new ConwaysGameOfLifeServices(loggerMock.Object, configurationMock.Object, conwaysGameOfLifeDbContext);

            var livePoints = new List<Point>
            {
                new Point(0, 0),
                new Point(0, 1),
                new Point(1, 0),
                new Point(1, 1),
                new Point(-1, -1),
                new Point(2, 2),
            };
            conwaysGameOfLifeServicesInstance.Seed(livePoints);

            // Act
            var method = typeof(ConwaysGameOfLifeServices).GetMethod("CountNeighbours", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "CountNeighbours method should not be null.");
            method.Invoke(conwaysGameOfLifeServicesInstance, null);

            // Assert
            var livePointsField = typeof(ConwaysGameOfLifeServices).GetField("livePoints", BindingFlags.NonPublic | BindingFlags.Instance);
            var livePointsSet = (HashSet<BoardPoint>?)livePointsField?.GetValue(conwaysGameOfLifeServicesInstance)
                ?? throw new InvalidOperationException("livePoints field is null.");

            // (1,1) should have 4 neighbours
            var point = new BoardPoint(1, 1);
            Assert.That(livePointsSet.TryGetValue(point, out var foundPoint), Is.True);
            Assert.That(foundPoint.LiveNeighbours, Is.EqualTo(4), point.ToString() + " should have 4 neighbours.");

            // (0,0) should have 4 neighbours
            point = new BoardPoint(0, 0);
            Assert.That(livePointsSet.TryGetValue(point, out foundPoint), Is.True);
            Assert.That(foundPoint.LiveNeighbours, Is.EqualTo(4), point.ToString() + " should have 4 neighbours.");

            // (0,1) should have 3 neighbours
            point = new BoardPoint(0, 1);
            Assert.That(livePointsSet.TryGetValue(point, out foundPoint), Is.True);
            Assert.That(foundPoint.LiveNeighbours, Is.EqualTo(3), point.ToString() + " should have 3 neighbours.");

            // (1,0) should have 3 neighbours
            point = new BoardPoint(1, 0);
            Assert.That(livePointsSet.TryGetValue(point, out foundPoint), Is.True);
            Assert.That(foundPoint.LiveNeighbours, Is.EqualTo(3), point.ToString() + " should have 3 neighbours.");

            // (2,2) should have 1 neighbours
            point = new BoardPoint(2, 2);
            Assert.That(livePointsSet.TryGetValue(point, out foundPoint), Is.True);
            Assert.That(foundPoint.LiveNeighbours, Is.EqualTo(1), point.ToString() + " should have 1 neighbours.");

            // (-1,-1) should have 1 neighbours
            point = new BoardPoint(-1, -1);
            Assert.That(livePointsSet.TryGetValue(point, out foundPoint), Is.True);
            Assert.That(foundPoint.LiveNeighbours, Is.EqualTo(1), point.ToString() + " should have 1 neighbours.");


            var deadNeighborField = typeof(ConwaysGameOfLifeServices).GetField("deadNeighbours", BindingFlags.NonPublic | BindingFlags.Instance);
            var deadNeighboursSet = (HashSet<BoardPoint>?)deadNeighborField?.GetValue(conwaysGameOfLifeServicesInstance)
                ?? throw new InvalidOperationException("deadNeighbours field is null.");

            // (1,2) dead neighbour should have 3 neighbours
            point = new BoardPoint(1, 2);
            Assert.That(deadNeighboursSet.TryGetValue(point, out foundPoint), Is.True);
            Assert.That(foundPoint.LiveNeighbours, Is.EqualTo(3), "Dead neighbour " + point.ToString() + " should have 3 neighbours.");

            // (3,1) dead neighbour should have 1 neighbours
            point = new BoardPoint(3, 1);
            Assert.That(deadNeighboursSet.TryGetValue(point, out foundPoint), Is.True);
            Assert.That(foundPoint.LiveNeighbours, Is.EqualTo(1), "Dead neighbour " + point.ToString() + " should have 1 neighbours.");

            // (1,-1) dead neighbour should have 2 neighbours
            point = new BoardPoint(1, -1);
            Assert.That(deadNeighboursSet.TryGetValue(point, out foundPoint), Is.True);
            Assert.That(foundPoint.LiveNeighbours, Is.EqualTo(2), "Dead neighbour " + point.ToString() + " should have 2 neighbours.");
        }

        [Test]
        public void Transition_SeedWithBlockPattern_TransitionsCorrectly()
        {
            // Arrange
            var conwaysGameOfLifeDbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeServices>>();
            var configurationMock = new Mock<IConfiguration>();
            var conwaysGameOfLifeServicesInstance = new ConwaysGameOfLifeServices(loggerMock.Object, configurationMock.Object, conwaysGameOfLifeDbContext);

            // Block pattern (Still Life)
            var initialLivePoints = new List<Point>
            {
                new Point(0, 0),
                new Point(1, 0),
                new Point(0, 1),
                new Point(1, 1),
            };
            var boardId = conwaysGameOfLifeServicesInstance.Seed(initialLivePoints);

            // Act
            var result = conwaysGameOfLifeServicesInstance.Transition(boardId, 1);

            // Assert
            // After one iteration a still life should be identical.
            var expected = initialLivePoints;

            Assert.That(result.Count, Is.EqualTo(4));
            CollectionAssert.AreEquivalent(expected, result);

            // Also check that the database reflects the new state.
            var dbLivePoints = conwaysGameOfLifeDbContext.LivePoints.Where(lp => lp.BoardId == boardId)
                .Select(lp => new Point(lp.X, lp.Y)).ToList();
            CollectionAssert.AreEquivalent(expected, dbLivePoints);
        }

        [Test]
        public void Transition_SeedWithBeehivePattern_TransitionsCorrectly()
        {
            // Arrange
            var conwaysGameOfLifeDbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeServices>>();
            var configurationMock = new Mock<IConfiguration>();
            var conwaysGameOfLifeServicesInstance = new ConwaysGameOfLifeServices(loggerMock.Object, configurationMock.Object, conwaysGameOfLifeDbContext);

            // Beehive pattern (Still Life)
            var initialLivePoints = new List<Point>
            {
                new Point(1, 0),
                new Point(2, 0),
                new Point(0, 1),
                new Point(3, 1),
                new Point(1, 2),
                new Point(2, 2)
            };
            var boardId = conwaysGameOfLifeServicesInstance.Seed(initialLivePoints);

            // Act
            var result = conwaysGameOfLifeServicesInstance.Transition(boardId, 1);

            // Assert
            // After one iteration a still life should be identical.
            var expected = initialLivePoints;

            Assert.That(result.Count, Is.EqualTo(6));
            CollectionAssert.AreEquivalent(expected, result);

            // Also check that the database reflects the new state.
            var dbLivePoints = conwaysGameOfLifeDbContext.LivePoints.Where(lp => lp.BoardId == boardId)
                .Select(lp => new Point(lp.X, lp.Y)).ToList();
            CollectionAssert.AreEquivalent(expected, dbLivePoints);
        }

        [Test]
        public void Transition_SeedWithLoafPattern_TransitionsCorrectly()
        {
            // Arrange
            var conwaysGameOfLifeDbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeServices>>();
            var configurationMock = new Mock<IConfiguration>();
            var conwaysGameOfLifeServicesInstance = new ConwaysGameOfLifeServices(loggerMock.Object, configurationMock.Object, conwaysGameOfLifeDbContext);

            // Loaf pattern (Still Life)
            var initialLivePoints = new List<Point>
            {
                new Point(2, 0),
                new Point(1, 1),
                new Point(3, 1),
                new Point(0, 2),
                new Point(3, 2),
                new Point(1, 3),
                new Point(2, 3)
            };
            var boardId = conwaysGameOfLifeServicesInstance.Seed(initialLivePoints);

            // Act
            var result = conwaysGameOfLifeServicesInstance.Transition(boardId, 1);

            // Assert
            // After one iteration a still life should be identical.
            var expected = initialLivePoints;

            Assert.That(result.Count, Is.EqualTo(7));
            CollectionAssert.AreEquivalent(expected, result);

            // Also check that the database reflects the new state.
            var dbLivePoints = conwaysGameOfLifeDbContext.LivePoints.Where(lp => lp.BoardId == boardId)
                .Select(lp => new Point(lp.X, lp.Y)).ToList();
            CollectionAssert.AreEquivalent(expected, dbLivePoints);
        }

        [Test]
        public void Transition_SeedWithBlinkerPattern_TransitionsCorrectly()
        {
            // Arrange
            var conwaysGameOfLifeDbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeServices>>();
            var configurationMock = new Mock<IConfiguration>();
            var conwaysGameOfLifeServicesInstance = new ConwaysGameOfLifeServices(loggerMock.Object, configurationMock.Object, conwaysGameOfLifeDbContext);

            // Blinker pattern: three vertical cells at (1,0), (1,1), (1,2)
            var initialLivePoints = new List<Point>
            {
                new Point(1, 0),
                new Point(1, 1),
                new Point(1, 2)
            };
            var boardId = conwaysGameOfLifeServicesInstance.Seed(initialLivePoints);

            // Act
            var result = conwaysGameOfLifeServicesInstance.Transition(boardId, 1);

            // Assert
            // After one iteration, blinker should be horizontal: (0,1), (1,1), (2,1)
            var expected = new List<Point>
            {
                new Point(0, 1),
                new Point(1, 1),
                new Point(2, 1)
            };

            Assert.That(result.Count, Is.EqualTo(3));
            CollectionAssert.AreEquivalent(expected, result);

            // Also check that the database reflects the new state.
            var dbLivePoints = conwaysGameOfLifeDbContext.LivePoints.Where(lp => lp.BoardId == boardId)
                .Select(lp => new Point(lp.X, lp.Y)).ToList();
            CollectionAssert.AreEquivalent(expected, dbLivePoints);

            // Act
            result = conwaysGameOfLifeServicesInstance.Transition(boardId, 1);

            // Assert
            // After two iterations, blinker should return back to orginal vertical pattern: (1,0), (1,1), (1,2)
            expected = initialLivePoints;

            Assert.That(result.Count, Is.EqualTo(3));
            CollectionAssert.AreEquivalent(expected, result);

            // Also check that the database reflects the new state.
            dbLivePoints = conwaysGameOfLifeDbContext.LivePoints.Where(lp => lp.BoardId == boardId)
                .Select(lp => new Point(lp.X, lp.Y)).ToList();
            CollectionAssert.AreEquivalent(expected, dbLivePoints);
        }
    }
}