namespace Blocktrust.CredentialWorkflow.Core;

using System.Text.Json;
using Entities.Identity;
using Entities.Tenants;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;

public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(),"../Blocktrust.CredentialWorkflow.Web"))
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
        : base(options){}

    // Tenants
    public DbSet<TenantEntity> TenantEntities { get; set; }

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
    }
}