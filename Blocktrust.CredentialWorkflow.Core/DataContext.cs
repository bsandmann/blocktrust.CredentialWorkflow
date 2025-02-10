namespace Blocktrust.CredentialWorkflow.Core;

using Entities.DIDComm;
using Entities.Identity;
using Entities.Outcome;
using Entities.Tenant;
using Entities.Workflow;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.Configuration;

public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Blocktrust.CredentialWorkflow.Web"))
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        optionsBuilder
            .UseNpgsql(connectionString)
            .EnableSensitiveDataLogging(false)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        return new DataContext(optionsBuilder.Options);
    }
}

public class DataContext : IdentityDbContext<ApplicationUser>
{
    /// <summary>
    /// Represents the data context for the application's storage.
    /// </summary
    /// 
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    public DbSet<TenantEntity> TenantEntities { get; set; }
    public DbSet<WorkflowEntity> WorkflowEntities { get; set; }
    public DbSet<WorkflowOutcomeEntity> WorkflowOutcomeEntities { get; set; }
    public DbSet<IssuingKeyEntity> IssuingKeys { get; set; }
    public DbSet<PeerDIDSecretEntity> PeerDIDSecrets { get; set; }
    public DbSet<PeerDIDEntity> PeerDIDEntities { get; set; }

    /// <summary>
    /// Setup
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Setup the the relationship for the inherited Identity
        base.OnModelCreating(modelBuilder);

        ///////////////////////////////////////////////////////////////// Tenants
        modelBuilder.Entity<TenantEntity>().HasKey(p => p.TenantEntityId);
        modelBuilder.Entity<TenantEntity>().Property(p => p.TenantEntityId).HasValueGenerator(typeof(SequentialGuidValueGenerator));

        // Connection between an ASP.net Identity ApplicationUser and a Tenant
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(p => p.TenantEntity)
            .WithMany(b => b.ApplicationUsers)
            .HasForeignKey(p => p.TenantEntityId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<IssuingKeyEntity>().HasKey(p => p.IssuingKeyId);
        modelBuilder.Entity<IssuingKeyEntity>().Property(p => p.IssuingKeyId).HasValueGenerator(typeof(SequentialGuidValueGenerator));

        modelBuilder.Entity<IssuingKeyEntity>()
            .HasOne<TenantEntity>()
            .WithMany(t => t.IssuingKeys) // You need a navigation property on TenantEntity
            .HasForeignKey(p => p.TenantEntityId)
            .OnDelete(DeleteBehavior.NoAction);

        //////////////////////////////////////////////////////////////// Workflow
        modelBuilder.Entity<WorkflowEntity>().HasKey(p => p.WorkflowEntityId);
        modelBuilder.Entity<WorkflowEntity>().Property(p => p.WorkflowEntityId).HasValueGenerator(typeof(SequentialGuidValueGenerator));
        modelBuilder.Entity<WorkflowEntity>().Property(b => b.WorkflowState).HasConversion<string>();

        modelBuilder.Entity<WorkflowOutcomeEntity>().HasKey(p => p.WorkflowOutcomeEntityId);
        modelBuilder.Entity<WorkflowOutcomeEntity>().Property(p => p.WorkflowOutcomeEntityId).HasValueGenerator(typeof(SequentialGuidValueGenerator));
        modelBuilder.Entity<WorkflowOutcomeEntity>().Property(b => b.WorkflowOutcomeState).HasConversion<string>();

        /////////////////////////////////////////////////////////////// DIDComm 
        modelBuilder.Entity<PeerDIDSecretEntity>().HasKey(p => p.PeerDIDSecretId);
        modelBuilder.Entity<PeerDIDSecretEntity>().Property(p => p.PeerDIDSecretId).HasValueGenerator(typeof(SequentialGuidValueGenerator));

        modelBuilder.Entity<PeerDIDEntity>().HasKey(p => p.PeerDIDEntityId);
        modelBuilder.Entity<PeerDIDEntity>().Property(p => p.PeerDIDEntityId)
            .HasValueGenerator(typeof(SequentialGuidValueGenerator));

        modelBuilder.Entity<PeerDIDEntity>()
            .HasOne<TenantEntity>()
            .WithMany(t => t.PeerDIDEntities) // You need a navigation property on TenantEntity
            .HasForeignKey(p => p.TenantEntityId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}