using _116.Content.Application.Interactions.UseCases.Public.Commands.BookmarkShortVideo.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.BookmarkShortVideo.V1;

/// <summary>
/// Integration tests for the PublicBookmarkShortVideo endpoint.
/// </summary>
[Collection("Database")]
public class PublicBookmarkShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task BookmarkShortVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync(Routes.Public.Shorts.Bookmarks(Guid.NewGuid()), null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task BookmarkShortVideo_AsVisitor_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync(Routes.Public.Shorts.Bookmarks(Guid.NewGuid()), null);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ShortVideo"))
        );
    }

    [Fact]
    public async Task BookmarkShortVideo_AsVisitor_WithValidShort_ReturnsOk()
    {
        ShortVideoEntity shortVideo = await SeedShortVideoAsync();
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync(Routes.Public.Shorts.Bookmarks(shortVideo.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicBookmarkShortVideoResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        (
            await verifyDb.ShortVideoBookmarks.AnyAsync(b =>
                b.ShortVideoId == shortVideo.Id && b.UserId == TestUser.VisitorId
            )
        )
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task BookmarkShortVideo_WhenAlreadyBookmarked_ReturnsConflict()
    {
        ShortVideoEntity shortVideo = await SeedShortVideoAsync();
        Client.AuthenticateAsVisitor();

        await Client.PostAsync(Routes.Public.Shorts.Bookmarks(shortVideo.Id), null);

        var response = await Client.PostAsync(Routes.Public.Shorts.Bookmarks(shortVideo.Id), null);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ShortVideoInteractionErrorMessage>(m => m.AlreadyBookmarked())
        );

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        (
            await verifyDb.ShortVideoBookmarks.CountAsync(b =>
                b.ShortVideoId == shortVideo.Id && b.UserId == TestUser.VisitorId
            )
        )
            .Should()
            .Be(1);
    }
}
