using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ConwaysGameOfLife.Data;
using Microsoft.EntityFrameworkCore.InMemory;
using ConwaysGameOfLife.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

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
            var dbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<ConwaysGameOfLifeServices>>();
            var configMock = new Mock<IConfiguration>();
            var service = new ConwaysGameOfLifeServices(loggerMock.Object, configMock.Object, dbContext);

            var livePoints = new List<Point>
            {
                new Point(1, 2),
                new Point(3, 4),
                new Point(5, 6),
                new Point(1, 2) // Duplicate point to test deduplication.
            };

            // Act
            var boardId = service.Seed(livePoints);

            // Assert
            var board = dbContext.Boards.SingleOrDefault(b => b.Id == boardId);
            Assert.IsNotNull(board, "Board should be created in the database.");

            var storedLivePoints = dbContext.LivePoints.Where(lp => lp.BoardId == boardId).ToList();
            Assert.That(storedLivePoints.Count, Is.EqualTo(3), "All live points should be stored.");

            Assert.IsTrue(storedLivePoints.Any(lp => lp.X == 1 && lp.Y == 2));
            Assert.IsTrue(storedLivePoints.Any(lp => lp.X == 3 && lp.Y == 4));
            Assert.IsTrue(storedLivePoints.Any(lp => lp.X == 5 && lp.Y == 6));
        }
    }
}