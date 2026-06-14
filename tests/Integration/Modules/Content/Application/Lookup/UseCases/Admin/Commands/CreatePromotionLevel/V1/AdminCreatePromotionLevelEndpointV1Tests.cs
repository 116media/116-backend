namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.CreatePromotionLevel.V1;

/// <summary>
/// Integration tests for the AdminCreatePromotionLevel endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreatePromotionLevelEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreatePromotionLevel_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Name = "Featured — 7 days",
            DurationDays = 7,
            PriceUsd = 50m,
            SpotPriority = (int?)null,
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PromotionLevels, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePromotionLevel_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var request = new
        {
            Name = "Featured — 7 days",
            DurationDays = 7,
            PriceUsd = 50m,
            SpotPriority = (int?)null,
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PromotionLevels, request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreatePromotionLevel_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = "Featured — 7 days",
            DurationDays = 7,
            PriceUsd = 50m,
            SpotPriority = (int?)1,
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PromotionLevels, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
