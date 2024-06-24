namespace Blocktrust.CredentialWorkflow.Core.Tests;

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class TransactionalTestDatabaseFixture
{
    private const string ConnectionString = @"Host=192.168.178.163; Database=CredentialWorkflowTests; Username=postgres; Password=password";
    // private const string ConnectionString = @"Host=10.10.20.103; Database=CredentialWorkflowTests; Username=postgres; Password=postgres";

    public DataContext CreateContext()
        => new DataContext(
            new DbContextOptionsBuilder<DataContext>()
                .UseNpgsql(ConnectionString)
                .EnableSensitiveDataLogging(true)
                .LogTo(Console.WriteLine, LogLevel.Information)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options);

    public TransactionalTestDatabaseFixture()
    {
        using var context = CreateContext();
        
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        Cleanup();
    }

    public void Cleanup()
    {
        // using var context = CreateContext();
        // context.SaveChanges();
    }
}