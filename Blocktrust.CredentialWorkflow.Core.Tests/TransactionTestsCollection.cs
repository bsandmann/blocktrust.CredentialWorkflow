namespace Blocktrust.CredentialWorkflow.Core.Tests;

using Xunit;

[CollectionDefinition("TransactionalTests", DisableParallelization = true)]
public class TransactionalTestsCollection : ICollectionFixture<TransactionalTestDatabaseFixture>
{
}