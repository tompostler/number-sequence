using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using number_sequence.Models;

namespace number_sequence.DataAccess
{
    public sealed class NsContext : DbContext
    {
        private readonly ILoggerFactory loggerFactory;

        public DbSet<EmailLatexDocument> EmailLatexDocuments { get; set; }
        public DbSet<LatexDocument> LatexDocuments { get; set; }
        public DbSet<LatexTemplate> LatexTemplates { get; set; }
        public DbSet<SynchronizedBackgroundService> SynchronizedBackgroundServices { get; set; }

        public NsContext(DbContextOptions<NsContext> dbContextOptions, ILoggerFactory loggerFactory)
            : base(dbContextOptions)
        {
            this.loggerFactory = loggerFactory;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            _ = optionsBuilder
                .UseLoggerFactory(this.loggerFactory)
                // Since it's just Ns, we can include the actual values of the parameters to all queries
                .EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<EmailLatexDocument>()
                .HasKey(x => x.Id);

            _ = modelBuilder.Entity<EmailLatexDocument>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            _ = modelBuilder.Entity<LatexDocument>()
            .HasKey(x => x.Id);

            _ = modelBuilder.Entity<LatexDocument>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            _ = modelBuilder.Entity<LatexTemplate>()
            .HasKey(x => x.Id);

            _ = modelBuilder.Entity<LatexTemplate>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            _ = modelBuilder.Entity<SynchronizedBackgroundService>()
                .HasKey(x => x.Name);

            base.OnModelCreating(modelBuilder);
        }
    }
}
