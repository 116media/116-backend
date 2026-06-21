using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

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

    /// <summary>
    /// Verifies that creating a promotion level with a name that already exists
    /// returns a 409 Conflict response.
    /// </summary>
    [Fact]
    public async Task CreatePromotionLevel_WithDuplicateName_ReturnsConflict()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var existing = PromotionLevelFactory.Create("Featured — 7 days", 7, 50m);
        seedContext.PromotionLevels.Add(existing);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = "Featured — 7 days",
            DurationDays = 14,
            PriceUsd = 99m,
            SpotPriority = (int?)null,
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PromotionLevels, request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    /// <summary>
    /// Verifies that creating a promotion level with a zero or negative duration
    /// returns a 400 Bad Request or 422 Unprocessable Entity response, exercising
    /// the <c>ValidDurationDays</c> rule in PromotionLevelValidation.
    /// </summary>
    [Fact]
    public async Task CreatePromotionLevel_WithZeroDuration_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = "Zero Duration",
            DurationDays = 0,
            PriceUsd = 50m,
            SpotPriority = (int?)null,
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PromotionLevels, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Verifies that creating a promotion level with a negative price
    /// returns a 400 Bad Request or 422 Unprocessable Entity response, exercising
    /// the <c>ValidPriceUsd</c> rule in PromotionLevelValidation.
    /// </summary>
    [Fact]
    public async Task CreatePromotionLevel_WithNegativePrice_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = "Negative Price",
            DurationDays = 7,
            PriceUsd = -10m,
            SpotPriority = (int?)null,
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PromotionLevels, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Verifies that creating a promotion level with a spot priority outside the
    /// valid range (1-3) returns a 400 Bad Request or 422 Unprocessable Entity
    /// response, exercising the <c>ValidSpotPriority</c> rule in PromotionLevelValidation.
    /// </summary>
    [Fact]
    public async Task CreatePromotionLevel_WithInvalidSpotPriority_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = "Invalid Spot",
            DurationDays = 7,
            PriceUsd = 50m,
            SpotPriority = (int?)4,
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PromotionLevels, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Verifies that creating a promotion level with an empty name
    /// returns a 400 Bad Request or 422 Unprocessable Entity response, exercising
    /// the <c>ValidPromotionLevelName</c> rule in PromotionLevelValidation.
    /// </summary>
    [Fact]
    public async Task CreatePromotionLevel_WithEmptyName_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = "",
            DurationDays = 7,
            PriceUsd = 50m,
            SpotPriority = (int?)null,
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PromotionLevels, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
