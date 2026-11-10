namespace PatientManagement.IntegrationTests.Fixtures;

// Every test class that uses the test database joins this collection. Tests in the collection run one at a time,
// so one class cannot clear or read rows while another class is writing them.
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class TestDatabaseCollection : ICollectionFixture<TestDatabaseFixture>
{
    public const string Name = "TestDatabase";
}
