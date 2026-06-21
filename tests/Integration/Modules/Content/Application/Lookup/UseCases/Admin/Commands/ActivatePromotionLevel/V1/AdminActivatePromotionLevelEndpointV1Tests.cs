using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePromotionLevel.V1;

/// <summary>
/// Integration tests for the AdminActivatePromotionLevel endpoint.
/// </summary>
[Collection("Database")]
public class AdminActivatePromotionLevelEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task ActivatePromotionLevel_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.PromotionLevels}/{Guid.NewGuid()}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ActivatePromotionLevel_AsSuperAdmin_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.PromotionLevels}/{Guid.NewGuid()}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActivatePromotionLevel_AsSuperAdmin_WithValidId_ReturnsOk()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var promotionLevel = PromotionLevelFactory.Create();
        promotionLevel.Deactivate();
        context.PromotionLevels.Add(promotionLevel);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.PromotionLevels}/{promotionLevel.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that activating a promotion level that is already active returns 409 Conflict.
    /// </summary>
    [Fact]
    public async Task ActivatePromotionLevel_WhenAlreadyActive_ReturnsConflict()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var promotionLevel = PromotionLevelFactory.Create();
        context.PromotionLevels.Add(promotionLevel);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.PromotionLevels}/{promotionLevel.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
