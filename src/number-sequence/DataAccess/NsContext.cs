using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace number_sequence.DataAccess
{
    public sealed class NsContext : DbContext
    {
        private readonly ILoggerFactory loggerFactory;

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
    }
}
