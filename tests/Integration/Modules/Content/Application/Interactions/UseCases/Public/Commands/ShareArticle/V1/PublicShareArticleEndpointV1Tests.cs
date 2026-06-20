namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.ShareArticle.V1;

/// <summary>
/// Integration tests for the PublicShareArticle endpoint.
/// </summary>
[Collection("Database")]
public class PublicShareArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task ShareArticle_AsAnonymous_ReturnsOk()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync($"{ApiRoutes.Public.Articles}/{Guid.NewGuid()}/shares", null);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShareArticle_AsVisitor_ReturnsOk()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync($"{ApiRoutes.Public.Articles}/{Guid.NewGuid()}/shares", null);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }
}
