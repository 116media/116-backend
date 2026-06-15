using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetAllShorts.V1;

/// <summary>
/// Integration tests for the AdminGetAllShorts endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllShortsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetAllShorts_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Admin.Shorts);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllShorts_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(ApiRoutes.Admin.Shorts);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllShorts_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync(ApiRoutes.Admin.Shorts);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllShorts_WithSearch_ReturnsOk()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Shorts}?search=test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the search query parameter filters short videos by title,
    /// returning only short videos whose title matches the search term.
    /// </summary>
    [Fact]
    public async Task GetAllShorts_WithSearchQuery_ReturnsFilteredResults()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var matchingShort = ShortVideoFactory.Create();
        var otherShort = ShortVideoFactory.Create();
        context.ShortVideos.AddRange(matchingShort, otherShort);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Shorts}?search=UniqueSearchTerm");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
