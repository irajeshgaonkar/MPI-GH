using HCA.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HCA.Data;

public class HcaDbContext : DbContext
{
    private readonly string _conntectionString;

    public HcaDbContext()
    {
        _conntectionString = "Host=aurora-postgres-database.cluster-ce211rmnisgi.us-east-1.rds.amazonaws.com;Port=5432;Database=testdb;Username=postgres;Password=admin1234;SearchPath='mpicoalation';";
    }

    public HcaDbContext(ConnectionDetails connectionDetails)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        _conntectionString = connectionDetails.ConnectionString;
    }

    public HcaDbContext(DbContextOptions<HcaDbContext> options, ConnectionDetails connectionDetails)
       : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        _conntectionString = connectionDetails.ConnectionString;
    }

    public DbSet<ClientIdentityEntity> ClientIdentities { get; set; }

    public DbSet<ClientIdentityAddressEntity> ClientIdentityAddresses { get; set; }

    public DbSet<ClientIdentityCommunicationEntity> ClientIdentityCommunications { get; set; }

    public DbSet<ClientIdentityAddressCommunicationEntity> ClientIdentityAddressCommunication { get; set; }

    public DbSet<FileRequestEntity> FileRequests { get; set; }

    public DbSet<UserRequestEntity> UserRequests { get; set; }

    public DbSet<ClientIdentityRequestEntity> ClientIdentityRequests { get; set; }

    public DbSet<MpiLinkIdHistoryEntity> MpiLinkIdHistory { get; set; }

    public DbSet<RequestProcessLogEntity> RequestProcessLogs { get; set; }

    public DbSet<UserModifyRecordsEntity> UserModifyRecords { get; set; }

    public DbSet<SftpFileTransferEntity> SftpFileTransfers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            optionsBuilder.UseNpgsql(_conntectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientIdentityEntity>()
            .HasKey(c => new { c.SourceSystemName, c.SourceSystemId });

        modelBuilder.Entity<ClientIdentityEntity>()
            .Property(f => f.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<ClientIdentityEntity>()
            .HasMany(c => c.Addresses)
            .WithOne(a => a.ClientIdentity)
            .HasForeignKey(a => new { a.SourceSystemName, a.SourceSystemId })
            .IsRequired();

        modelBuilder.Entity<ClientIdentityEntity>()
            .HasMany(c => c.Communications)
            .WithOne(c => c.ClientIdentity)
            .HasForeignKey(c => new { c.SourceSystemName, c.SourceSystemId })
            .IsRequired();

        modelBuilder.Entity<ClientIdentityAddressCommunicationEntity>()
            .HasKey(c => new { c.ClientIdentityAddressId, c.ClientIdentityCommunicationId });

        modelBuilder.Entity<ClientIdentityAddressCommunicationEntity>()
            .HasOne(ac => ac.Address)
            .WithMany(a => a.AddressCommunications)
            .HasForeignKey(ac => ac.ClientIdentityAddressId);

        modelBuilder.Entity<ClientIdentityAddressCommunicationEntity>()
           .HasOne(ac => ac.Communication)
           .WithMany(c => c.AddressCommunications)
           .HasForeignKey(ac => ac.ClientIdentityCommunicationId);

        base.OnModelCreating(modelBuilder);
    }
}