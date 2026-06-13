namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.UnlikeShortVideo.V1;

/// <summary>
/// Integration tests for the PublicUnlikeShortVideo endpoint.
/// </summary>
[Collection("Database")]
public class PublicUnlikeShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UnlikeShortVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync($"{ApiRoutes.Public.Shorts}/{Guid.NewGuid()}/likes");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UnlikeShortVideo_AsVisitor_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync($"{ApiRoutes.Public.Shorts}/{Guid.NewGuid()}/likes");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
