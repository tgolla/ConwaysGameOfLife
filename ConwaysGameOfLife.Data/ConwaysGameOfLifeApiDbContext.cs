using Microsoft.EntityFrameworkCore;

namespace ConwaysGameOfLife.Data
{
    /// <summary>
    /// Conway's Game of Life API DB context.
    /// </summary>
    /// <remarks>
    /// add-migration InitialCreate -context ConwaysGameOfLifeApiDbContext
    /// update-database -context ConwaysGameOfLifeApiDbContext
    /// 
    /// "ConnectionStrings": {
    ///   "ConwaysGameOfLifeApiDb": "Data Source=localhost\\sqlexpress;Database=ConwaysGameOfLifeApiDb;Integrated Security=false;Encrypt=false;User ID=xxxxxxxx;Password=xxxxxxxx;"
    /// }
    /// </remarks>
    public class ConwaysGameOfLifeApiDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of ConwaysGameOfLifeApiDbContext.
        /// </summary>
        /// <param name="options">The options to be used by a DbContext.</param>
        public ConwaysGameOfLifeApiDbContext(DbContextOptions<ConwaysGameOfLifeApiDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the collection of boards in the database.
        /// </summary>
        public DbSet<Board> Boards { get; set; }

        /// <summary>
        /// Gets or sets the collection of live points in the database.
        /// </summary>
        public DbSet<LivePoint> LivePoints { get; set; }

        /// <summary>
        /// Configures the schema needed for the identity framework.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Board>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<Board>()
                .Property(x => x.Expires)
                .IsRequired();

            modelBuilder.Entity<LivePoint>()
                .HasKey(x => new { x.BoardId, x.X, x.Y });
            modelBuilder.Entity<LivePoint>()
                .HasOne<Board>()
                .WithMany()
                .HasForeignKey(x => x.BoardId);
        }
    }
}
