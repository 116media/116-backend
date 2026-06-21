using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePricingTier.V1;

/// <summary>
/// Integration tests for the AdminUpdatePricingTier endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdatePricingTierEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UpdatePricingTier_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Name = "Updated", Description = "Updated desc" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PricingTiers}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdatePricingTier_AsSuperAdmin_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "Updated", Description = "Updated desc" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PricingTiers}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePricingTier_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var pricingTier = PricingTierFactory.Create();
        context.PricingTiers.Add(pricingTier);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "Updated Tier", Description = "Updated description" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PricingTiers}/{pricingTier.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that updating a pricing tier with a name exceeding the maximum allowed length
    /// (40 characters) returns a 400 Bad Request or 422 Unprocessable Entity response,
    /// exercising the <c>isRequired=false</c> branch of <c>ValidPricingTierName</c> in PricingTierValidation.
    /// </summary>
    [Fact]
    public async Task UpdatePricingTier_WithNameTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        var request = new { Name = new string('P', 200), Description = "Valid description" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PricingTiers}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Verifies that updating a pricing tier with a description exceeding the maximum allowed
    /// length (200 characters) returns a 400 Bad Request or 422 Unprocessable Entity response.
    /// </summary>
    [Fact]
    public async Task UpdatePricingTier_WithDescriptionTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        var request = new { Name = "Valid Name", Description = new string('D', 500) };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PricingTiers}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
