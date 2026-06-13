namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.UnbookmarkShortVideo.V1;

/// <summary>
/// Integration tests for the PublicUnbookmarkShortVideo endpoint.
/// </summary>
[Collection("Database")]
public class PublicUnbookmarkShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UnbookmarkShortVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync($"{ApiRoutes.Public.Shorts}/{Guid.NewGuid()}/bookmarks");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UnbookmarkShortVideo_AsVisitor_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync($"{ApiRoutes.Public.Shorts}/{Guid.NewGuid()}/bookmarks");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
