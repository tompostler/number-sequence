using Microsoft.EntityFrameworkCore;
using number_sequence.Models;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Invoicing;

namespace number_sequence.DataAccess
{
    public sealed class NsContext : DbContext
    {
        private readonly ILoggerFactory loggerFactory;

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Token> Tokens { get; set; }

        public DbSet<Count> Counts { get; set; }

        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceBusiness> InvoiceBusinesses { get; set; }
        public DbSet<InvoiceCustomer> InvoiceCustomers { get; set; }
        public DbSet<InvoiceLineDefault> InvoiceLineDefaults { get; set; }

        public DbSet<EmailLatexDocument> EmailLatexDocuments { get; set; }
        public DbSet<LatexDocument> LatexDocuments { get; set; }
        public DbSet<LatexTemplate> LatexTemplates { get; set; }
        public DbSet<LatexTemplateSpreadsheetRow> LatexTemplateSpreadsheetRows { get; set; }
        
        public DbSet<Redirect> Redirects { get; set; }

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
            #region Accounts

            _ = modelBuilder.Entity<Account>()
                .HasKey(x => x.Name);
            _ = modelBuilder.Entity<Account>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<Account>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            _ = modelBuilder.Entity<Token>()
                .HasKey(x => new { x.Account, x.Name });
            _ = modelBuilder.Entity<Token>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            #endregion // Accounts

            #region Invoicing

            _ = modelBuilder.HasSequence<long>("InvoiceIds");
            _ = modelBuilder.HasSequence<long>("InvoiceBusinessIds");
            _ = modelBuilder.HasSequence<long>("InvoiceCustomerIds");
            _ = modelBuilder.HasSequence<long>("InvoiceLineIds");
            _ = modelBuilder.HasSequence<long>("InvoiceLineDefaultIds");

            _ = modelBuilder.Entity<Invoice>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.InvoiceIds");
            _ = modelBuilder.Entity<Invoice>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<Invoice>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            _ = modelBuilder.Entity<InvoiceBusiness>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.InvoiceBusinessIds");
            _ = modelBuilder.Entity<InvoiceBusiness>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<InvoiceBusiness>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            _ = modelBuilder.Entity<InvoiceCustomer>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.InvoiceCustomerIds");
            _ = modelBuilder.Entity<InvoiceCustomer>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<InvoiceCustomer>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            _ = modelBuilder.Entity<InvoiceLine>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.InvoiceLineIds");
            _ = modelBuilder.Entity<InvoiceLine>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<InvoiceLine>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<InvoiceLine>()
                .Property(x => x.Quantity)
                .HasPrecision(10, 2);
            _ = modelBuilder.Entity<InvoiceLine>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);

            _ = modelBuilder.Entity<InvoiceLineDefault>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.InvoiceLineDefaultIds");
            _ = modelBuilder.Entity<InvoiceLineDefault>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<InvoiceLineDefault>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<InvoiceLineDefault>()
                .Property(x => x.Quantity)
                .HasPrecision(10, 2);
            _ = modelBuilder.Entity<InvoiceLineDefault>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);

            _ = modelBuilder.Entity<Invoice>()
                .HasOne(x => x.Business)
                .WithMany(x => x.Invoices)
                .OnDelete(DeleteBehavior.Restrict);
            _ = modelBuilder.Entity<Invoice>()
                .HasOne(x => x.Customer)
                .WithMany(x => x.Invoices)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion // Invoicing

            #region Latex

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

            _ = modelBuilder.Entity<LatexTemplateSpreadsheetRow>()
                .HasKey(x => x.LatexDocumentId);

            _ = modelBuilder.Entity<LatexTemplateSpreadsheetRow>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            #endregion // Latex

            _ = modelBuilder.Entity<Count>()
                .HasKey(x => new { x.Account, x.Name });
            _ = modelBuilder.Entity<Count>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<Count>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            _ = modelBuilder.Entity<Redirect>()
                .HasKey(x => new { x.Id });
            _ = modelBuilder.Entity<Redirect>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<Redirect>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            _ = modelBuilder.Entity<SynchronizedBackgroundService>()
                .HasKey(x => x.Name);

            base.OnModelCreating(modelBuilder);
        }
    }
}
