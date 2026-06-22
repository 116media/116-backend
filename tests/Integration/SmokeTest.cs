using _116.Identity.Infrastructure.Persistence;

namespace _116.Integration.Tests;

/// <summary>
/// Validates that the integration test infrastructure boots correctly.
/// </summary>
[Collection("Database")]
public class SmokeTest(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task Api_ShouldReturnResponse_WhenHealthEndpointHit()
    {
        var response = await Client.GetAsync(Routes.Public.Auth.Login());

        response.Should().NotBeNull();
    }

    [Fact]
    public async Task DbContext_ShouldBeAccessible()
    {
        await using var context = CreateDbContext<IdentityDbContext>();

        bool canConnect = await context.Database.CanConnectAsync();

        canConnect.Should().BeTrue();
    }
}
