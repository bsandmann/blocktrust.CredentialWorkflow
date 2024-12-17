namespace Blocktrust.CredentialWorkflow.Core;

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
    public DbSet<OutcomeEntity> OutcomeEntities { get; set; }

    public DbSet<IssuingKeyEntity> IssuingKeys { get; set; }

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

        //////////////////////////////////////////////////////////////// Workflow
        modelBuilder.Entity<WorkflowEntity>().HasKey(p => p.WorkflowEntityId);
        modelBuilder.Entity<WorkflowEntity>().Property(p => p.WorkflowEntityId).HasValueGenerator(typeof(SequentialGuidValueGenerator));

        modelBuilder.Entity<OutcomeEntity>().HasKey(p => p.OutcomeEntityId);
        modelBuilder.Entity<OutcomeEntity>().Property(p => p.OutcomeEntityId).HasValueGenerator(typeof(SequentialGuidValueGenerator));


    }
}