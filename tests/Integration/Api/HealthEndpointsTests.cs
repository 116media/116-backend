namespace _116.Integration.Tests.Api;

/// <summary>
/// Integration tests for the health endpoints: liveness reports process-up without probing
/// dependencies, readiness probes the backing stores.
/// </summary>
public class HealthEndpointsTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task HealthLive_WhenProcessIsUp_Returns200()
    {
        // Act
        using HttpResponseMessage response = await Client.GetAsync("/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("Healthy");
    }

    [Fact]
    public async Task HealthReady_WithReachableDatabase_Returns200()
    {
        // Act
        using HttpResponseMessage response = await Client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("Healthy");
    }
}
