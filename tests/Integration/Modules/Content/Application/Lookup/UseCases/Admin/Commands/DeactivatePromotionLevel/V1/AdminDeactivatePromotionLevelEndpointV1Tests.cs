using _116.Content.Application.Lookup.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePromotionLevel.V1;

/// <summary>
/// Integration tests for the AdminDeactivatePromotionLevel endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeactivatePromotionLevelEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<bool> IsPromotionLevelActiveAsync(Guid id)
    {
        await using ContentDbContext context = CreateDbContext<ContentDbContext>();
        PromotionLevelEntity? promotionLevel = await context.PromotionLevels.FindAsync(id);
        return promotionLevel!.IsActive;
    }

    [Fact]
    public async Task DeactivatePromotionLevel_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync(
            Routes.Admin.Lookup.Deactivate(LookupRouteConstants.PromotionLevels, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeactivatePromotionLevel_AsSuperAdmin_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Lookup.Deactivate(LookupRouteConstants.PromotionLevels, Guid.NewGuid()),
            null
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeactivatePromotionLevel_AsSuperAdmin_WithValidId_ReturnsOk()
    {
        PromotionLevelEntity promotionLevel = await SeedAsync<ContentDbContext, PromotionLevelEntity>(ctx =>
        {
            PromotionLevelEntity entity = PromotionLevelFactory.Create();
            ctx.PromotionLevels.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Lookup.Deactivate(LookupRouteConstants.PromotionLevels, promotionLevel.Id),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await IsPromotionLevelActiveAsync(promotionLevel.Id)).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that deactivating a promotion level that is already inactive returns 409 Conflict.
    /// </summary>
    [Fact]
    public async Task DeactivatePromotionLevel_WhenAlreadyInactive_ReturnsConflict()
    {
        PromotionLevelEntity promotionLevel = await SeedAsync<ContentDbContext, PromotionLevelEntity>(ctx =>
        {
            PromotionLevelEntity entity = PromotionLevelFactory.CreateInactive();
            ctx.PromotionLevels.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Lookup.Deactivate(LookupRouteConstants.PromotionLevels, promotionLevel.Id),
            null
        );

        await response.ShouldBeProblem(HttpStatusCode.Conflict);
        (await IsPromotionLevelActiveAsync(promotionLevel.Id)).Should().BeFalse();
    }
}
