using _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeShortVideo.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.UnlikeShortVideo.V1;

/// <summary>
/// Integration tests for the PublicUnlikeShortVideo endpoint.
/// </summary>
[Collection("Database")]
public class PublicUnlikeShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<ShortVideoEntity> SeedShortVideoAsync()
    {
        return await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity shortVideo = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(shortVideo);
            return shortVideo;
        });
    }

    [Fact]
    public async Task UnlikeShortVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync(Routes.Public.Shorts.Likes(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UnlikeShortVideo_AsVisitor_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(Routes.Public.Shorts.Likes(Guid.NewGuid()));

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ShortVideo"))
        );
    }

    [Fact]
    public async Task UnlikeShortVideo_WhenLiked_RemovesLikeAndPersists()
    {
        ShortVideoEntity shortVideo = await SeedShortVideoAsync();
        Client.AuthenticateAsVisitor();
        await Client.PostAsync(Routes.Public.Shorts.Likes(shortVideo.Id), null);

        var response = await Client.DeleteAsync(Routes.Public.Shorts.Likes(shortVideo.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicUnlikeShortVideoResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        (
            await verifyDb.ShortVideoLikes.AnyAsync(l =>
                l.ShortVideoId == shortVideo.Id && l.UserId == TestUser.VisitorId
            )
        )
            .Should()
            .BeFalse();
    }

    [Fact]
    public async Task UnlikeShortVideo_WhenNotLiked_ReturnsBadRequest()
    {
        ShortVideoEntity shortVideo = await SeedShortVideoAsync();
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(Routes.Public.Shorts.Likes(shortVideo.Id));

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ShortVideoInteractionErrorMessage>(m => m.LikeNotFound())
        );
    }
}
