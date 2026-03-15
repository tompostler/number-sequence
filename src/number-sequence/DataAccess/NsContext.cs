using Microsoft.EntityFrameworkCore;
using number_sequence.Models;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.DataAccess
{
    public sealed class NsContext : DbContext
    {
        private readonly ILoggerFactory loggerFactory;

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Token> Tokens { get; set; }

        public DbSet<Count> Counts { get; set; }

        public DbSet<DailySequenceValue> DailySequenceValues { get; set; }
        public DbSet<DailySequenceValueConfig> DailySequenceValueConfigs { get; set; }

        public DbSet<DaysSince> DaysSinces { get; set; }

        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Business> InvoiceBusinesses { get; set; }
        public DbSet<BusinessLogo> InvoiceBusinessLogos { get; set; }
        public DbSet<Customer> InvoiceCustomers { get; set; }
        public DbSet<InvoiceLine> InvoiceLines { get; set; }
        public DbSet<InvoiceLineDefault> InvoiceLineDefaults { get; set; }
        public DbSet<InvoicePayment> InvoicePayments { get; set; }
        public DbSet<Statement> Statements { get; set; }

        public DbSet<ChiroEmailBatch> ChiroEmailBatches { get; set; }
        public DbSet<EmailDocument> EmailDocuments { get; set; }
        public DbSet<PdfTemplate> PdfTemplates { get; set; }
        public DbSet<PdfTemplateSpreadsheetRow> PdfTemplateSpreadsheetRows { get; set; }

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

            #region Ledger

            _ = modelBuilder.HasSequence<long>("InvoiceIds");
            _ = modelBuilder.HasSequence<long>("InvoiceBusinessIds");
            _ = modelBuilder.HasSequence<long>("InvoiceCustomerIds");
            _ = modelBuilder.HasSequence<long>("InvoiceLineIds");
            _ = modelBuilder.HasSequence<long>("InvoiceLineDefaultIds");
            _ = modelBuilder.HasSequence<long>("InvoicePaymentIds");
            _ = modelBuilder.HasSequence<long>("StatementIds");

            _ = modelBuilder.Entity<Invoice>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.InvoiceIds");
            _ = modelBuilder.Entity<Invoice>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<Invoice>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            _ = modelBuilder.Entity<Statement>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.StatementIds");
            _ = modelBuilder.Entity<Statement>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<Statement>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            _ = modelBuilder.Entity<Business>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.InvoiceBusinessIds");
            _ = modelBuilder.Entity<Business>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<Business>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            _ = modelBuilder.Entity<BusinessLogo>()
                .HasKey(x => x.BusinessId);
            _ = modelBuilder.Entity<BusinessLogo>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<BusinessLogo>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<Business>()
                .HasOne(x => x.Logo)
                .WithOne(x => x.Business)
                .HasForeignKey<BusinessLogo>(x => x.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            _ = modelBuilder.Entity<Customer>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.InvoiceCustomerIds");
            _ = modelBuilder.Entity<Customer>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<Customer>()
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
            _ = modelBuilder.Entity<InvoiceLine>()
                .ToTable("InvoiceLines");

            _ = modelBuilder.Entity<InvoicePayment>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.InvoicePaymentIds");
            _ = modelBuilder.Entity<InvoicePayment>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<InvoicePayment>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<InvoicePayment>()
                .Property(x => x.Amount)
                .HasPrecision(18, 2);
            _ = modelBuilder.Entity<InvoicePayment>()
                .HasOne(x => x.Invoice)
                .WithMany(x => x.Payments)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

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
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            _ = modelBuilder.Entity<Invoice>()
                .HasOne(x => x.Customer)
                .WithMany(x => x.Invoices)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            _ = modelBuilder.Entity<Statement>()
                .HasOne(x => x.Business)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            _ = modelBuilder.Entity<Statement>()
                .HasOne(x => x.Customer)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            #endregion // Ledger

            #region Pdf

            _ = modelBuilder.Entity<ChiroEmailBatch>()
                .HasKey(x => x.Id);

            _ = modelBuilder.Entity<ChiroEmailBatch>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            _ = modelBuilder.Entity<EmailDocument>()
                .HasKey(x => x.Id);

            _ = modelBuilder.Entity<EmailDocument>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            _ = modelBuilder.Entity<PdfTemplate>()
                .HasKey(x => x.Id);

            _ = modelBuilder.Entity<PdfTemplate>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            _ = modelBuilder.Entity<PdfTemplateSpreadsheetRow>()
                .HasKey(x => x.DocumentId);

            _ = modelBuilder.Entity<PdfTemplateSpreadsheetRow>()
                .Property(x => x.ProcessedAt)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            #endregion // Pdf

            _ = modelBuilder.Entity<Count>()
                .HasKey(x => new { x.Account, x.Name });
            _ = modelBuilder.Entity<Count>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<Count>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            _ = modelBuilder.Entity<DailySequenceValue>()
                .HasKey(x => new { x.Account, x.Category, x.EventDate });
            _ = modelBuilder.Entity<DailySequenceValue>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<DailySequenceValue>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<DailySequenceValue>()
                .Property(x => x.Value)
                .HasPrecision(27, 5);
            _ = modelBuilder.Entity<DailySequenceValue>()
                .Property(x => x.OriginalValue)
                .HasPrecision(27, 5);

            _ = modelBuilder.Entity<DailySequenceValueConfig>()
                .HasKey(x => new { x.Account, x.Category });
            _ = modelBuilder.Entity<DailySequenceValueConfig>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<DailySequenceValueConfig>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<DailySequenceValueConfig>()
                .Property(x => x.NegativeDeltaMax)
                .HasPrecision(27, 5);
            _ = modelBuilder.Entity<DailySequenceValueConfig>()
                .Property(x => x.NegativeDeltaMin)
                .HasPrecision(27, 5);
            _ = modelBuilder.Entity<DailySequenceValueConfig>()
                .Property(x => x.PositiveDeltaMax)
                .HasPrecision(27, 5);
            _ = modelBuilder.Entity<DailySequenceValueConfig>()
                .Property(x => x.PositiveDeltaMin)
                .HasPrecision(27, 5);

            _ = modelBuilder.Entity<DaysSince>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<DaysSince>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            _ = modelBuilder.HasSequence<long>("DaysSinceEventIds");
            _ = modelBuilder.Entity<DaysSinceEvent>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.DaysSinceEventIds");
            _ = modelBuilder.Entity<DaysSinceEvent>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<DaysSinceEvent>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

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
