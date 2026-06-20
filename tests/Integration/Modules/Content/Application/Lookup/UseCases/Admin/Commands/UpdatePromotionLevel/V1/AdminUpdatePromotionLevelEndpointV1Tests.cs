using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePromotionLevel.V1;

/// <summary>
/// Integration tests for the AdminUpdatePromotionLevel endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdatePromotionLevelEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UpdatePromotionLevel_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Name = "Updated",
            DurationDays = 14,
            PriceUsd = 100m,
            SpotPriority = (int?)null,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PromotionLevels}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdatePromotionLevel_AsSuperAdmin_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = "Updated",
            DurationDays = 14,
            PriceUsd = 100m,
            SpotPriority = (int?)null,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PromotionLevels}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePromotionLevel_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var promotionLevel = PromotionLevelFactory.Create();
        context.PromotionLevels.Add(promotionLevel);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = "À la Une — 14 days",
            DurationDays = 14,
            PriceUsd = 100m,
            SpotPriority = (int?)2,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PromotionLevels}/{promotionLevel.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
