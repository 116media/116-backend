namespace _116.Integration.Tests.Api;

/// <summary>
/// Verifies that readiness degrades to 503 when the database is unreachable while liveness
/// keeps reporting the process as up.
/// </summary>
/// <param name="db">The dedicated Testcontainer database and misdirected application host.</param>
[Collection("UnreachableDatabase")]
public class HealthReadyDegradedTests(UnreachableDatabasePostgresFixture db) : IDisposable
{
    private readonly HttpClient _client = db.Api.CreateClient();

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task HealthReady_WithUnreachableDatabase_Returns503()
    {
        // Act
        using HttpResponseMessage response = await _client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        (await response.Content.ReadAsStringAsync()).Should().Be("Unhealthy");
    }

    [Fact]
    public async Task HealthLive_WithUnreachableDatabase_StillReturns200()
    {
        // Act
        using HttpResponseMessage response = await _client.GetAsync("/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
