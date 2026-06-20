namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.LikeShortVideo.V1;

/// <summary>
/// Integration tests for the PublicLikeShortVideo endpoint.
/// </summary>
[Collection("Database")]
public class PublicLikeShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task LikeShortVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync($"{ApiRoutes.Public.Shorts}/{Guid.NewGuid()}/likes", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LikeShortVideo_AsVisitor_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync($"{ApiRoutes.Public.Shorts}/{Guid.NewGuid()}/likes", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
