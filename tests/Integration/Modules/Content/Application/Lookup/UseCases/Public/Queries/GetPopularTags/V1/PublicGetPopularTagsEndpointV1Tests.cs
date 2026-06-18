using _116.Content.Application.Lookup.UseCases.Public.Queries.GetPopularTags.V1;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Public.Queries.GetPopularTags.V1;

/// <summary>
/// Integration tests for the PublicGetPopularTags endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetPopularTagsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetPopularTags_AsAnonymous_ReturnsTagsList()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Tags}/popular");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicGetPopularTagsResponse>();
        body.Tags.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPopularTags_WithLimitParam_RespectsLimit()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Tags}/popular?limit=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicGetPopularTagsResponse>();
        body.Tags.Should().NotBeNull();
        body.Tags.Count.Should().BeLessThanOrEqualTo(5);
    }

    [Fact]
    public async Task GetPopularTags_WithContentTypeParam_ReturnsTagsList()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Tags}/popular?contentType=Article");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicGetPopularTagsResponse>();
        body.Tags.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPopularTags_WithVideoContentType_ReturnsTagsList()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Tags}/popular?contentType=Video");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicGetPopularTagsResponse>();
        body.Tags.Should().NotBeNull();
    }
}
