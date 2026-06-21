using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

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

    /// <summary>
    /// Verifies that liking a short video that is already liked returns 409 Conflict.
    /// </summary>
    [Fact]
    public async Task LikeShortVideo_WhenAlreadyLiked_ReturnsConflict()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var shortVideo = ShortVideoFactory.Create();
        context.ShortVideos.Add(shortVideo);
        await context.SaveChangesAsync();

        Client.AuthenticateAsVisitor();

        await Client.PostAsync($"{ApiRoutes.Public.Shorts}/{shortVideo.Id}/likes", null);

        var response = await Client.PostAsync($"{ApiRoutes.Public.Shorts}/{shortVideo.Id}/likes", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
