using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.CreatePricingTier.V1;

/// <summary>
/// Integration tests for the AdminCreatePricingTier endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreatePricingTierEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreatePricingTier_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Name = "base_upload", Description = "Base upload fee for content." };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PricingTiers, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePricingTier_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var request = new { Name = "base_upload", Description = "Base upload fee for content." };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PricingTiers, request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreatePricingTier_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "base_upload", Description = "Base upload fee for content." };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PricingTiers, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    /// <summary>
    /// Verifies that creating a pricing tier with a name that already exists
    /// returns a 409 Conflict response.
    /// </summary>
    [Fact]
    public async Task CreatePricingTier_WithDuplicateName_ReturnsConflict()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var existing = PricingTierFactory.Create("base_upload");
        seedContext.PricingTiers.Add(existing);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "base_upload", Description = "Duplicate tier." };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PricingTiers, request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    /// <summary>
    /// Verifies that creating a pricing tier with an empty name returns a
    /// 400 Bad Request or 422 Unprocessable Entity response from the validator.
    /// </summary>
    [Fact]
    public async Task CreatePricingTier_WithEmptyName_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "", Description = "A tier with no name." };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PricingTiers, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
