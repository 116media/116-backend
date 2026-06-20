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
}
