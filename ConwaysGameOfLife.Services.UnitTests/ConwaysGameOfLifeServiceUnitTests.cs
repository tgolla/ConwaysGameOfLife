using ConwaysGameOfLife.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
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
    public class ConwaysGameOfLifeServiceUnitTests
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

        /// <summary>
        /// Creates and configures an <see cref="IConfiguration"/> instance using appsettings.json.
        /// </summary>
        /// <returns>An <see cref="IConfiguration"/> instance containing the configuration settings.</returns>
        private IConfiguration CreateConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            return builder.Build();
        }

        [Test]
        public void Seed_WithValidLivePoints_CreatesBoardAndLivePoints()
        {
            // Arrange
            var conwaysGameOfLifeDbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeService>>();
            var configuration = CreateConfiguration();
            var ConwaysGameOfLifeServiceInstance = new ConwaysGameOfLifeService(loggerMock.Object, configuration, conwaysGameOfLifeDbContext);

            var livePoints = new List<Point>
            {
                new Point(1, 2),
                new Point(3, 4),
                new Point(5, 6),
                new Point(1, 2) // Duplicate point to test deduplication.
            };

            // Act
            var boardId = ConwaysGameOfLifeServiceInstance.Seed(livePoints);

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
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeService>>();
            var configuration = CreateConfiguration();
            var ConwaysGameOfLifeServiceInstance = new ConwaysGameOfLifeService(loggerMock.Object, configuration, conwaysGameOfLifeDbContext);

            var livePoints = new List<Point>
            {
                new Point(1, 1),
                new Point(1, 2),
                new Point(2, 1)
            };
            ConwaysGameOfLifeServiceInstance.Seed(livePoints);

            // Use reflection to access the private method
            var boardPoint = new BoardPoint(1, 1);
            var method = typeof(ConwaysGameOfLifeService)
                .GetMethod("CountNeighboursForPoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            Assert.IsNotNull(method, "CountNeighboursForPoint method should not be null.");
            var result = method.Invoke(ConwaysGameOfLifeServiceInstance, new object[] { boardPoint });
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
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeService>>();
            var configuration = CreateConfiguration();
            var ConwaysGameOfLifeServiceInstance = new ConwaysGameOfLifeService(loggerMock.Object, configuration, conwaysGameOfLifeDbContext);

            var livePoints = new List<Point>
            {
                new Point(5, 5)
            };
            ConwaysGameOfLifeServiceInstance.Seed(livePoints);

            var boardPoint = new BoardPoint(5, 5);
            var method = typeof(ConwaysGameOfLifeService)
                .GetMethod("CountNeighboursForPoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            Assert.IsNotNull(method, "CountNeighboursForPoint method should not be null.");
            var result = method.Invoke(ConwaysGameOfLifeServiceInstance, new object[] { boardPoint });
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
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeService>>();
            var configuration = CreateConfiguration();
            var ConwaysGameOfLifeServiceInstance = new ConwaysGameOfLifeService(loggerMock.Object, configuration, conwaysGameOfLifeDbContext);

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
            ConwaysGameOfLifeServiceInstance.Seed(livePoints);

            var boardPoint = new BoardPoint(0, 0);
            var method = typeof(ConwaysGameOfLifeService)
                .GetMethod("CountNeighboursForPoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            Assert.IsNotNull(method, "CountNeighboursForPoint method should not be null.");
            var result = method.Invoke(ConwaysGameOfLifeServiceInstance, new object[] { boardPoint });
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
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeService>>();
            var configuration = CreateConfiguration();
            var ConwaysGameOfLifeServiceInstance = new ConwaysGameOfLifeService(loggerMock.Object, configuration, conwaysGameOfLifeDbContext);

            var livePoints = new List<Point>
            {
                new Point(0, 0),
                new Point(0, 1),
                new Point(1, 0),
                new Point(1, 1),
                new Point(-1, -1),
                new Point(2, 2),
            };
            ConwaysGameOfLifeServiceInstance.Seed(livePoints);

            // Act
            var method = typeof(ConwaysGameOfLifeService).GetMethod("CountNeighbours", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "CountNeighbours method should not be null.");
            method.Invoke(ConwaysGameOfLifeServiceInstance, null);

            // Assert
            var livePointsField = typeof(ConwaysGameOfLifeService).GetField("livePoints", BindingFlags.NonPublic | BindingFlags.Instance);
            var livePointsSet = (HashSet<BoardPoint>?)livePointsField?.GetValue(ConwaysGameOfLifeServiceInstance)
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


            var deadNeighborField = typeof(ConwaysGameOfLifeService).GetField("deadNeighbours", BindingFlags.NonPublic | BindingFlags.Instance);
            var deadNeighboursSet = (HashSet<BoardPoint>?)deadNeighborField?.GetValue(ConwaysGameOfLifeServiceInstance)
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
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeService>>();
            var configuration = CreateConfiguration();
            var ConwaysGameOfLifeServiceInstance = new ConwaysGameOfLifeService(loggerMock.Object, configuration, conwaysGameOfLifeDbContext);

            // Block pattern (Still Life)
            var initialLivePoints = new List<Point>
            {
                new Point(0, 0),
                new Point(1, 0),
                new Point(0, 1),
                new Point(1, 1),
            };
            var boardId = ConwaysGameOfLifeServiceInstance.Seed(initialLivePoints);

            // Act
            var result = ConwaysGameOfLifeServiceInstance.Transition(boardId, 1);

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
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeService>>();
            var configuration = CreateConfiguration();
            var ConwaysGameOfLifeServiceInstance = new ConwaysGameOfLifeService(loggerMock.Object, configuration, conwaysGameOfLifeDbContext);

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
            var boardId = ConwaysGameOfLifeServiceInstance.Seed(initialLivePoints);

            // Act
            var result = ConwaysGameOfLifeServiceInstance.Transition(boardId, 1);

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
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeService>>();
            var configuration = CreateConfiguration();
            var ConwaysGameOfLifeServiceInstance = new ConwaysGameOfLifeService(loggerMock.Object, configuration, conwaysGameOfLifeDbContext);

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
            var boardId = ConwaysGameOfLifeServiceInstance.Seed(initialLivePoints);

            // Act
            var result = ConwaysGameOfLifeServiceInstance.Transition(boardId, 1);

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
        public void Transition_SeedWithBoatPattern_TransitionsCorrectly()
        {
            // Arrange
            var conwaysGameOfLifeDbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeService>>();
            var configuration = CreateConfiguration();
            var ConwaysGameOfLifeServiceInstance = new ConwaysGameOfLifeService(loggerMock.Object, configuration, conwaysGameOfLifeDbContext);

            // Boat pattern (Still Life)
            var initialLivePoints = new List<Point>
            {
                new Point(1, 0),
                new Point(0, 1),
                new Point(2, 1),
                new Point(0, 2),
                new Point(1, 2)
            };
            var boardId = ConwaysGameOfLifeServiceInstance.Seed(initialLivePoints);

            // Act
            var result = ConwaysGameOfLifeServiceInstance.Transition(boardId, 1);

            // Assert
            // After one iteration a still life should be identical.
            var expected = initialLivePoints;

            Assert.That(result.Count, Is.EqualTo(5));
            CollectionAssert.AreEquivalent(expected, result);

            // Also check that the database reflects the new state.
            var dbLivePoints = conwaysGameOfLifeDbContext.LivePoints.Where(lp => lp.BoardId == boardId)
                .Select(lp => new Point(lp.X, lp.Y)).ToList();
            CollectionAssert.AreEquivalent(expected, dbLivePoints);
        }

        [Test]
        public void Transition_SeedWithBTubPattern_TransitionsCorrectly()
        {
            // Arrange
            var conwaysGameOfLifeDbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeService>>();
            var configuration = CreateConfiguration();
            var ConwaysGameOfLifeServiceInstance = new ConwaysGameOfLifeService(loggerMock.Object, configuration, conwaysGameOfLifeDbContext);

            // BTub pattern (Still Life)
            var initialLivePoints = new List<Point>
            {
                new Point(1, 0),
                new Point(0, 1),
                new Point(2, 1),
                new Point(1, 2)
            };
            var boardId = ConwaysGameOfLifeServiceInstance.Seed(initialLivePoints);

            // Act
            var result = ConwaysGameOfLifeServiceInstance.Transition(boardId, 1);

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
        public void Transition_SeedWithBlinkerPattern_TransitionsCorrectly()
        {
            // Arrange
            var conwaysGameOfLifeDbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeService>>();
            var configuration = CreateConfiguration();
            var ConwaysGameOfLifeServiceInstance = new ConwaysGameOfLifeService(loggerMock.Object, configuration, conwaysGameOfLifeDbContext);

            // Blinker pattern (Oscillator): three vertical cells at (1,0), (1,1), (1,2)
            var initialLivePoints = new List<Point>
            {
                new Point(1, 0),
                new Point(1, 1),
                new Point(1, 2)
            };
            var boardId = ConwaysGameOfLifeServiceInstance.Seed(initialLivePoints);

            // Act
            var result = ConwaysGameOfLifeServiceInstance.Transition(boardId, 1);

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
            result = ConwaysGameOfLifeServiceInstance.Transition(boardId, 1);

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

        [Test]
        public void Transition_SeedWithToadPattern_TransitionsCorrectly()
        {
            // Arrange
            var conwaysGameOfLifeDbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeService>>();
            var configuration = CreateConfiguration();
            var ConwaysGameOfLifeServiceInstance = new ConwaysGameOfLifeService(loggerMock.Object, configuration, conwaysGameOfLifeDbContext);

            // Toad pattern (Oscillator)
            var initialLivePoints = new List<Point>
            {
                new Point(0, 1),
                new Point(1, 1),
                new Point(2, 1),
                new Point(1, 2),
                new Point(2, 2),
                new Point(3, 2)
            };
            var boardId = ConwaysGameOfLifeServiceInstance.Seed(initialLivePoints);

            // Act
            var result = ConwaysGameOfLifeServiceInstance.Transition(boardId, 1);

            // Assert
            var expected = new List<Point>
            {
                new Point(1, 0),
                new Point(0, 1),
                new Point(3, 1),
                new Point(0, 2),
                new Point(3, 2),
                new Point(2, 3)
            };

            Assert.That(result.Count, Is.EqualTo(6));
            CollectionAssert.AreEquivalent(expected, result);

            // Also check that the database reflects the new state.
            var dbLivePoints = conwaysGameOfLifeDbContext.LivePoints.Where(lp => lp.BoardId == boardId)
                .Select(lp => new Point(lp.X, lp.Y)).ToList();
            CollectionAssert.AreEquivalent(expected, dbLivePoints);

            // Act
            result = ConwaysGameOfLifeServiceInstance.Transition(boardId, 1);

            // Assert
            // After two iterations, toad should return back to orginal pattern.
            expected = initialLivePoints;

            Assert.That(result.Count, Is.EqualTo(6));
            CollectionAssert.AreEquivalent(expected, result);

            // Also check that the database reflects the new state.
            dbLivePoints = conwaysGameOfLifeDbContext.LivePoints.Where(lp => lp.BoardId == boardId)
                .Select(lp => new Point(lp.X, lp.Y)).ToList();
            CollectionAssert.AreEquivalent(expected, dbLivePoints);
        }
        [Test]
        public void Transition_SeedWithBeaconPattern_TransitionsCorrectly()
        {
            // Arrange
            var conwaysGameOfLifeDbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeService>>();
            var configuration = CreateConfiguration();
            var ConwaysGameOfLifeServiceInstance = new ConwaysGameOfLifeService(loggerMock.Object, configuration, conwaysGameOfLifeDbContext);

            // Beacon pattern (Oscillator)
            var initialLivePoints = new List<Point>
            {
                new Point(2, 0),
                new Point(3, 0),
                new Point(2, 1),
                new Point(3, 1),
                new Point(0, 2),
                new Point(1, 2),
                new Point(0, 3),
                new Point(1, 3)
            };
            var boardId = ConwaysGameOfLifeServiceInstance.Seed(initialLivePoints);

            // Act
            var result = ConwaysGameOfLifeServiceInstance.Transition(boardId, 1);

            // Assert
            var expected = new List<Point>
            {
                new Point(2, 0),
                new Point(3, 0),
                new Point(3, 1),
                new Point(0, 2),
                new Point(0, 3),
                new Point(1, 3)
            };

            Assert.That(result.Count, Is.EqualTo(6));
            CollectionAssert.AreEquivalent(expected, result);

            // Also check that the database reflects the new state.
            var dbLivePoints = conwaysGameOfLifeDbContext.LivePoints.Where(lp => lp.BoardId == boardId)
                .Select(lp => new Point(lp.X, lp.Y)).ToList();
            CollectionAssert.AreEquivalent(expected, dbLivePoints);

            // Act
            result = ConwaysGameOfLifeServiceInstance.Transition(boardId, 1);

            // Assert
            // After two iterations, toad should return back to orginal pattern.
            expected = initialLivePoints;

            Assert.That(result.Count, Is.EqualTo(8));
            CollectionAssert.AreEquivalent(expected, result);

            // Also check that the database reflects the new state.
            dbLivePoints = conwaysGameOfLifeDbContext.LivePoints.Where(lp => lp.BoardId == boardId)
                .Select(lp => new Point(lp.X, lp.Y)).ToList();
            CollectionAssert.AreEquivalent(expected, dbLivePoints);
        }

        //TODO: Add more tests for other patterns like Pulsar, Pentadecathlon, Glider, Lightweight Spaceship (LWSS), Rpentomino, Diehard, Acorn, etc.


        [Test]
        public void End_EmptyBoard_ReturnsEmptyList()
        {
            // Arrange
            var conwaysGameOfLifeDbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeService>>();
            var configuration = CreateConfiguration();
            var ConwaysGameOfLifeServiceInstance = new ConwaysGameOfLifeService(loggerMock.Object, configuration, conwaysGameOfLifeDbContext);
            var boardId = Guid.NewGuid();

            conwaysGameOfLifeDbContext.Boards.Add(new Board { Id = boardId, Expires = DateTime.UtcNow });
            conwaysGameOfLifeDbContext.SaveChanges();

            // Act
            var result = ConwaysGameOfLifeServiceInstance.End(boardId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
            Assert.IsNull(conwaysGameOfLifeDbContext.Boards.Find(boardId));
        }

        [Test]
        public void End_SingleLiveCell_Dies_ReturnsEmptyList()
        {
            // Arrange
            var conwaysGameOfLifeDbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeService>>();
            var configuration = CreateConfiguration();
            var ConwaysGameOfLifeServiceInstance = new ConwaysGameOfLifeService(loggerMock.Object, configuration, conwaysGameOfLifeDbContext);
            var boardId = Guid.NewGuid();

            conwaysGameOfLifeDbContext.Boards.Add(new Board { Id = boardId, Expires = DateTime.UtcNow });
            conwaysGameOfLifeDbContext.LivePoints.Add(new LivePoint { BoardId = boardId, X = 0, Y = 0 });
            conwaysGameOfLifeDbContext.SaveChanges();

            // Act
            var result = ConwaysGameOfLifeServiceInstance.End(boardId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
            Assert.IsNull(conwaysGameOfLifeDbContext.Boards.Find(boardId));
        }

        [Test]
        public void End_StableBlockPattern_EndsEarlyWithBlock()
        {
            // Arrange
            var conwaysGameOfLifeDbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeService>>();
            var configuration = CreateConfiguration();
            var ConwaysGameOfLifeServiceInstance = new ConwaysGameOfLifeService(loggerMock.Object, configuration, conwaysGameOfLifeDbContext);
            var boardId = Guid.NewGuid();

            conwaysGameOfLifeDbContext.Boards.Add(new Board { Id = boardId, Expires = DateTime.UtcNow });

            // Block pattern
            conwaysGameOfLifeDbContext.LivePoints.AddRange(new[] {
                            new LivePoint { BoardId = boardId, X = 0, Y = 0 },
                            new LivePoint { BoardId = boardId, X = 0, Y = 1 },
                            new LivePoint { BoardId = boardId, X = 1, Y = 0 },
                            new LivePoint { BoardId = boardId, X = 1, Y = 1 }
                        });
            conwaysGameOfLifeDbContext.SaveChanges();

            // Act
            var result = ConwaysGameOfLifeServiceInstance.End(boardId);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(4));
            var expected = new HashSet<Point> { new(0, 0), new(0, 1), new(1, 0), new(1, 1) };
            CollectionAssert.AreEquivalent(expected, result);
            Assert.IsNull(conwaysGameOfLifeDbContext.Boards.Find(boardId));
        }

        [Test]
        public void End_OscillatorBeaconPattern_EndsEarlyWithBlock()
        {
            // Arrange
            var conwaysGameOfLifeDbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeService>>();
            var configuration = CreateConfiguration();
            var ConwaysGameOfLifeServiceInstance = new ConwaysGameOfLifeService(loggerMock.Object, configuration, conwaysGameOfLifeDbContext);
            var boardId = Guid.NewGuid();

            conwaysGameOfLifeDbContext.Boards.Add(new Board { Id = boardId, Expires = DateTime.UtcNow });

            // Beacon pattern
            conwaysGameOfLifeDbContext.LivePoints.AddRange(new[] {
                            new LivePoint { BoardId = boardId, X = 2, Y = 0 },
                            new LivePoint { BoardId = boardId, X = 3, Y = 0 },
                            new LivePoint { BoardId = boardId, X = 2, Y = 1 },
                            new LivePoint { BoardId = boardId, X = 3, Y = 1 },
                            new LivePoint { BoardId = boardId, X = 0, Y = 2 },
                            new LivePoint { BoardId = boardId, X = 1, Y = 2 },
                            new LivePoint { BoardId = boardId, X = 0, Y = 3 },
                            new LivePoint { BoardId = boardId, X = 1, Y = 3 }
                        });

            conwaysGameOfLifeDbContext.SaveChanges();

            // Act
            var result = ConwaysGameOfLifeServiceInstance.End(boardId);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(6));
            var expected = new HashSet<Point> { new(2, 0), new(3, 0), new(3, 1), new(0, 2), new(0, 3), new(1, 3) };
            CollectionAssert.AreEquivalent(expected, result);
            Assert.IsNull(conwaysGameOfLifeDbContext.Boards.Find(boardId));
        }

        [Test]
        public void End_RpentominoPattern_EndsEarlyWithBlock()
        {
            // Arrange
            var conwaysGameOfLifeDbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeService>>();
            var configuration = CreateConfiguration();
            var ConwaysGameOfLifeServiceInstance = new ConwaysGameOfLifeService(loggerMock.Object, configuration, conwaysGameOfLifeDbContext);
            var boardId = Guid.NewGuid();

            conwaysGameOfLifeDbContext.Boards.Add(new Board { Id = boardId, Expires = DateTime.UtcNow });

            // R-pentomino pattern
            conwaysGameOfLifeDbContext.LivePoints.AddRange(new[] {
                            new LivePoint { BoardId = boardId, X = 1, Y = 0 },
                            new LivePoint { BoardId = boardId, X = 0, Y = 2 },
                            new LivePoint { BoardId = boardId, X = 1, Y = 2 },
                            new LivePoint { BoardId = boardId, X = 1, Y = 3 },
                            new LivePoint { BoardId = boardId, X = 2, Y = 3 }
                        });

            conwaysGameOfLifeDbContext.SaveChanges();

            // Act
            var result = ConwaysGameOfLifeServiceInstance.End(boardId);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(7));
            var expected = new HashSet<Point> { new(1, 4), new(0, 1), new(2, 3), new(0, 4), new(-1, 2), new(1, 2), new(-1, 3) };
            CollectionAssert.AreEquivalent(expected, result);
            Assert.IsNull(conwaysGameOfLifeDbContext.Boards.Find(boardId));
        }

        //TODO: Add test for Pulsar, Pentadecathlon, Diehard, Acorn and infinte growth (single block-laying switch engine).

        [Test]
        public void End_MaxGenerationReached_ThrowsInvalidOperationException()
        {
            // Arrange
            var conwaysGameOfLifeDbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeService>>();
            var configuration = CreateConfiguration();
            var ConwaysGameOfLifeServiceInstance = new ConwaysGameOfLifeService(loggerMock.Object, configuration, conwaysGameOfLifeDbContext);
            var boardId = Guid.NewGuid();

            conwaysGameOfLifeDbContext.Boards.Add(new Board { Id = boardId, Expires = DateTime.UtcNow });

            // Blinker pattern (period 2 oscillator)
            conwaysGameOfLifeDbContext.LivePoints.AddRange(new[] {
                            new LivePoint { BoardId = boardId, X = 0, Y = 1 },
                            new LivePoint { BoardId = boardId, X = 1, Y = 1 },
                            new LivePoint { BoardId = boardId, X = 2, Y = 1 }
                        });
            conwaysGameOfLifeDbContext.SaveChanges();

            // Set settings to require a very high number of stable iterations to force max generation
            // Replace this block:
            var inMemorySettings = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("ConwaysGameOfLifeSettings:MaximunGenerationBeforeEnding", "3"),
                new KeyValuePair<string, string>("ConwaysGameOfLifeSettings:StablePopulationIterations", "100")
            };

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            loggerMock = new Mock<ILogger<ConwaysGameOfLifeService>>();
            var customService = new ConwaysGameOfLifeService(loggerMock.Object, configuration, conwaysGameOfLifeDbContext);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => customService.End(boardId));
            StringAssert.Contains("maximum generation limit", ex.Message);
            Assert.IsNull(conwaysGameOfLifeDbContext.Boards.Find(boardId));
        }
    }
}