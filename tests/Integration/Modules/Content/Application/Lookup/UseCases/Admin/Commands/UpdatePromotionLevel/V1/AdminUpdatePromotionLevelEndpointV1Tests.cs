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

    /// <summary>
    /// Verifies that updating a promotion level with a name exceeding the maximum allowed length
    /// (40 characters) returns a 400 Bad Request or 422 Unprocessable Entity response,
    /// exercising the <c>isRequired=false</c> branch of <c>ValidPromotionLevelName</c>
    /// in PromotionLevelValidation.
    /// </summary>
    [Fact]
    public async Task UpdatePromotionLevel_WithNameTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        var request = new
        {
            Name = new string('L', 200),
            DurationDays = 7,
            PriceUsd = 50m,
            SpotPriority = (int?)null,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PromotionLevels}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Verifies that updating a promotion level with a negative price returns a 400 Bad Request
    /// or 422 Unprocessable Entity response, exercising the <c>ValidPriceUsd</c> rule
    /// in PromotionLevelValidation.
    /// </summary>
    [Fact]
    public async Task UpdatePromotionLevel_WithNegativePrice_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        var request = new
        {
            Name = "Valid Name",
            DurationDays = 7,
            PriceUsd = -10m,
            SpotPriority = (int?)null,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PromotionLevels}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Verifies that updating a promotion level with a zero duration returns a 400 Bad Request
    /// or 422 Unprocessable Entity response, exercising the <c>ValidDurationDays</c> rule
    /// in PromotionLevelValidation.
    /// </summary>
    [Fact]
    public async Task UpdatePromotionLevel_WithZeroDuration_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        var request = new
        {
            Name = "Valid Name",
            DurationDays = 0,
            PriceUsd = 50m,
            SpotPriority = (int?)null,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PromotionLevels}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Verifies that updating a promotion level with an invalid spot priority (outside 1-3)
    /// returns a 400 Bad Request or 422 Unprocessable Entity response, exercising the
    /// <c>ValidSpotPriority</c> rule in PromotionLevelValidation.
    /// </summary>
    [Fact]
    public async Task UpdatePromotionLevel_WithInvalidSpotPriority_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        var request = new
        {
            Name = "Valid Name",
            DurationDays = 7,
            PriceUsd = 50m,
            SpotPriority = (int?)5,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PromotionLevels}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
