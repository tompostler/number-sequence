using Microsoft.EntityFrameworkCore;
using number_sequence.Models;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Ledger;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.DataAccess
{
    public sealed class NsContext : DbContext
    {
        private readonly ILoggerFactory loggerFactory;

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Token> Tokens { get; set; }

        public DbSet<Count> Counts { get; set; }
        public DbSet<CountEvent> CountEvents { get; set; }

        public DbSet<DaysSince> DaysSinces { get; set; }

        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Business> InvoiceBusinesses { get; set; }
        public DbSet<BusinessLogo> InvoiceBusinessLogos { get; set; }
        public DbSet<Customer> InvoiceCustomers { get; set; }
        public DbSet<InvoiceLine> InvoiceLines { get; set; }
        public DbSet<InvoiceLineDefault> InvoiceLineDefaults { get; set; }
        public DbSet<InvoicePayment> InvoicePayments { get; set; }
        public DbSet<Statement> Statements { get; set; }

        public DbSet<Library> Libraries { get; set; }
        public DbSet<LibraryCopy> LibraryCopies { get; set; }
        public DbSet<LibraryCopyEvent> LibraryCopyEvents { get; set; }
        public DbSet<LibraryItem> LibraryItems { get; set; }
        public DbSet<LibraryLoan> LibraryLoans { get; set; }
        public DbSet<LibraryLocation> LibraryLocations { get; set; }
        public DbSet<LibraryScanEntry> LibraryScanEntries { get; set; }
        public DbSet<LibraryScanSession> LibraryScanSessions { get; set; }
        public DbSet<LibraryShare> LibraryShares { get; set; }

        public DbSet<ChiroEmailBatch> ChiroEmailBatches { get; set; }
        public DbSet<ChiroRecord> ChiroRecords { get; set; }
        public DbSet<EmailDocument> EmailDocuments { get; set; }
        public DbSet<PdfTemplate> PdfTemplates { get; set; }

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

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            _ = configurationBuilder
                .Properties<Dictionary<string, string>>()
                .HaveConversion<JsonDictionaryConverter, JsonDictionaryComparer>()
                .HaveMaxLength(4000);
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

            #region Library

            // Cascades run Library -> Item -> Copy -> Loan/Event, plus Library -> Location/Share/ScanSession -> ScanEntry.
            // Every other path into a copy or location is NoAction, because SQL Server rejects a table that can be reached by
            // two cascade paths (Library -> Copy directly and via Item would be two). Event location/loan ids and scan entry
            // copy ids are plain columns with no foreign key at all, so history outlives the rows it mentions.

            _ = modelBuilder.HasSequence<long>("LibraryIds");
            _ = modelBuilder.HasSequence<long>("LibraryCopyIds");
            _ = modelBuilder.HasSequence<long>("LibraryCopyEventIds");
            _ = modelBuilder.HasSequence<long>("LibraryItemIds");
            _ = modelBuilder.HasSequence<long>("LibraryLoanIds");
            _ = modelBuilder.HasSequence<long>("LibraryLocationIds");
            _ = modelBuilder.HasSequence<long>("LibraryScanEntryIds");
            _ = modelBuilder.HasSequence<long>("LibraryScanSessionIds");

            _ = modelBuilder.Entity<Library>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.LibraryIds");
            _ = modelBuilder.Entity<Library>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<Library>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<Library>()
                .HasIndex(x => x.AccountName);

            _ = modelBuilder.Entity<LibraryShare>()
                .HasKey(x => new { x.LibraryId, x.AccountName });
            _ = modelBuilder.Entity<LibraryShare>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<LibraryShare>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<LibraryShare>()
                .HasOne(x => x.Library)
                .WithMany(x => x.Shares)
                .HasForeignKey(x => x.LibraryId)
                .OnDelete(DeleteBehavior.Cascade);
            // "Libraries shared with me" is looked up by grantee, not by library.
            _ = modelBuilder.Entity<LibraryShare>()
                .HasIndex(x => x.AccountName);

            _ = modelBuilder.Entity<LibraryLocation>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.LibraryLocationIds");
            _ = modelBuilder.Entity<LibraryLocation>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<LibraryLocation>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<LibraryLocation>()
                .HasOne<Library>()
                .WithMany()
                .HasForeignKey(x => x.LibraryId)
                .OnDelete(DeleteBehavior.Cascade);
            _ = modelBuilder.Entity<LibraryLocation>()
                .HasOne<LibraryLocation>()
                .WithMany()
                .HasForeignKey(x => x.ParentLocationId)
                .OnDelete(DeleteBehavior.NoAction);

            _ = modelBuilder.Entity<LibraryItem>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.LibraryItemIds");
            _ = modelBuilder.Entity<LibraryItem>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<LibraryItem>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<LibraryItem>()
                .Property(x => x.SeriesNumber)
                .HasPrecision(8, 2);
            _ = modelBuilder.Entity<LibraryItem>()
                .HasOne<Library>()
                .WithMany()
                .HasForeignKey(x => x.LibraryId)
                .OnDelete(DeleteBehavior.Cascade);
            _ = modelBuilder.Entity<LibraryItem>()
                .HasIndex(x => new { x.LibraryId, x.Title });

            _ = modelBuilder.Entity<LibraryCopy>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.LibraryCopyIds");
            _ = modelBuilder.Entity<LibraryCopy>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<LibraryCopy>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<LibraryCopy>()
                .Property(x => x.PricePaid)
                .HasPrecision(18, 2);
            _ = modelBuilder.Entity<LibraryCopy>()
                .HasOne(x => x.Item)
                .WithMany(x => x.Copies)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
            _ = modelBuilder.Entity<LibraryCopy>()
                .HasOne<Library>()
                .WithMany()
                .HasForeignKey(x => x.LibraryId)
                .OnDelete(DeleteBehavior.NoAction);
            _ = modelBuilder.Entity<LibraryCopy>()
                .HasOne(x => x.Location)
                .WithMany()
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.NoAction);
            // The "do I own this?" lookup and every scan resolve by barcode.
            _ = modelBuilder.Entity<LibraryCopy>()
                .HasIndex(x => new { x.LibraryId, x.Barcode });

            _ = modelBuilder.Entity<LibraryLoan>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.LibraryLoanIds");
            _ = modelBuilder.Entity<LibraryLoan>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<LibraryLoan>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<LibraryLoan>()
                .HasOne(x => x.Copy)
                .WithMany(x => x.Loans)
                .HasForeignKey(x => x.CopyId)
                .OnDelete(DeleteBehavior.Cascade);

            _ = modelBuilder.Entity<LibraryCopyEvent>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.LibraryCopyEventIds");
            _ = modelBuilder.Entity<LibraryCopyEvent>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<LibraryCopyEvent>()
                .HasOne<LibraryCopy>()
                .WithMany(x => x.Events)
                .HasForeignKey(x => x.CopyId)
                .OnDelete(DeleteBehavior.Cascade);

            _ = modelBuilder.Entity<LibraryScanSession>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.LibraryScanSessionIds");
            _ = modelBuilder.Entity<LibraryScanSession>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<LibraryScanSession>()
                .HasOne<Library>()
                .WithMany()
                .HasForeignKey(x => x.LibraryId)
                .OnDelete(DeleteBehavior.Cascade);
            _ = modelBuilder.Entity<LibraryScanSession>()
                .HasOne<LibraryLocation>()
                .WithMany()
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.NoAction);

            _ = modelBuilder.Entity<LibraryScanEntry>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.LibraryScanEntryIds");
            _ = modelBuilder.Entity<LibraryScanEntry>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<LibraryScanEntry>()
                .HasOne<LibraryScanSession>()
                .WithMany(x => x.Entries)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion // Library

            #region Pdf

            _ = modelBuilder.HasSequence<long>("ChiroEmailBatchIds").StartsAt(500);

            _ = modelBuilder.Entity<ChiroEmailBatch>()
                .HasKey(x => x.Id);

            _ = modelBuilder.Entity<ChiroEmailBatch>()
                .Property(x => x.Id)
                .HasDefaultValueSql("CAST(NEXT VALUE FOR dbo.ChiroEmailBatchIds AS nvarchar(20))");

            _ = modelBuilder.Entity<ChiroEmailBatch>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            _ = modelBuilder.Entity<ChiroRecord>()
                .HasKey(x => x.RowId);
            _ = modelBuilder.Entity<ChiroRecord>()
                .Property(x => x.RecordedAt)
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

            #endregion // Pdf

            _ = modelBuilder.Entity<Count>()
                .HasKey(x => new { x.Account, x.Name });
            _ = modelBuilder.Entity<Count>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<Count>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            _ = modelBuilder.HasSequence<long>("CountEventIds");
            _ = modelBuilder.Entity<CountEvent>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.CountEventIds");
            _ = modelBuilder.Entity<CountEvent>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<CountEvent>()
                .Property(x => x.ModifiedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");
            _ = modelBuilder.Entity<CountEvent>()
                .HasOne(x => x.Count)
                .WithMany(x => x.Events)
                .HasForeignKey(x => new { x.Account, x.CountName })
                .OnDelete(DeleteBehavior.Cascade);

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
