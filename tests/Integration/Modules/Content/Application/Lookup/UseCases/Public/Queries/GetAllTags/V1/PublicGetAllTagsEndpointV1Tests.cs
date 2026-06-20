using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags.V1;

/// <summary>
/// Integration tests for the PublicGetAllTags endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetAllTagsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetAllTags_AsAnonymous_ReturnsOk()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.Tags);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllTags_WithSearchParam_ReturnsOk()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Tags}?search=test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
