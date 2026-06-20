namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetArticleComments.V1;

/// <summary>
/// Integration tests for the PublicGetArticleComments endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetArticleCommentsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetArticleComments_AsAnonymous_ReturnsOk()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/{Guid.NewGuid()}/comments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetArticleComments_WithPagination_ReturnsOk()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Public.Articles}/{Guid.NewGuid()}/comments?pageIndex=0&pageSize=5"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
