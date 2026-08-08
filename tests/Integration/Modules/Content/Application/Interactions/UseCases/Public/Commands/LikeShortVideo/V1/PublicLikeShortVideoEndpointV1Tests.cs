using _116.Content.Application.Interactions.UseCases.Public.Commands.LikeShortVideo.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.LikeShortVideo.V1;

/// <summary>
/// Integration tests for the PublicLikeShortVideo endpoint.
/// </summary>
[Collection("Database")]
public class PublicLikeShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task LikeShortVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync(Routes.Public.Shorts.Likes(Guid.NewGuid()), null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LikeShortVideo_AsVisitor_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync(Routes.Public.Shorts.Likes(Guid.NewGuid()), null);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ShortVideo"))
        );
    }

    [Fact]
    public async Task LikeShortVideo_AsVisitor_WithValidShort_ReturnsOk()
    {
        ShortVideoEntity shortVideo = await SeedShortVideoAsync();
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync(Routes.Public.Shorts.Likes(shortVideo.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicLikeShortVideoResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        (
            await verifyDb.ShortVideoLikes.AnyAsync(l =>
                l.ShortVideoId == shortVideo.Id && l.UserId == TestUser.VisitorId
            )
        )
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task LikeShortVideo_WhenAlreadyLiked_ReturnsConflict()
    {
        ShortVideoEntity shortVideo = await SeedShortVideoAsync();
        Client.AuthenticateAsVisitor();

        await Client.PostAsync(Routes.Public.Shorts.Likes(shortVideo.Id), null);

        var response = await Client.PostAsync(Routes.Public.Shorts.Likes(shortVideo.Id), null);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ShortVideoInteractionErrorMessage>(m => m.AlreadyLiked())
        );

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        (
            await verifyDb.ShortVideoLikes.CountAsync(l =>
                l.ShortVideoId == shortVideo.Id && l.UserId == TestUser.VisitorId
            )
        )
            .Should()
            .Be(1);
    }
}
