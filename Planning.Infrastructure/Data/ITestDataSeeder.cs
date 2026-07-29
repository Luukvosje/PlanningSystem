namespace Planning.Infrastructure.Data;

public interface ITestDataSeeder
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
