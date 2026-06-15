using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePromotionLevel.V1;

/// <summary>
/// Integration tests for the AdminDeactivatePromotionLevel endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeactivatePromotionLevelEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task DeactivatePromotionLevel_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.PromotionLevels}/{Guid.NewGuid()}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeactivatePromotionLevel_AsSuperAdmin_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.PromotionLevels}/{Guid.NewGuid()}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeactivatePromotionLevel_AsSuperAdmin_WithValidId_ReturnsOk()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var promotionLevel = PromotionLevelFactory.Create();
        context.PromotionLevels.Add(promotionLevel);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            $"{ApiRoutes.Admin.PromotionLevels}/{promotionLevel.Id}/deactivate",
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that deactivating a promotion level that is already inactive returns 409 Conflict.
    /// </summary>
    [Fact]
    public async Task DeactivatePromotionLevel_WhenAlreadyInactive_ReturnsConflict()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var promotionLevel = PromotionLevelFactory.CreateInactive();
        context.PromotionLevels.Add(promotionLevel);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            $"{ApiRoutes.Admin.PromotionLevels}/{promotionLevel.Id}/deactivate",
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
