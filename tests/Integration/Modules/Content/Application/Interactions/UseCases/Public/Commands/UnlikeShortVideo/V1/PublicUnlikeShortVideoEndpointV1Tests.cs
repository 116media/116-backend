using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

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

    /// <summary>
    /// Verifies that unliking a short video that was never liked returns 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task UnlikeShortVideo_WhenNotLiked_ReturnsBadRequest()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var shortVideo = ShortVideoFactory.Create();
        context.ShortVideos.Add(shortVideo);
        await context.SaveChangesAsync();

        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync($"{ApiRoutes.Public.Shorts}/{shortVideo.Id}/likes");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
