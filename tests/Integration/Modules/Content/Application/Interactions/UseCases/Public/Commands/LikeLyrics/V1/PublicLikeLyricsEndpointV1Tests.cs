using _116.Content.Application.Interactions.UseCases.Public.Commands.LikeLyrics.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.LikeLyrics.V1;

/// <summary>
/// Integration tests for the PublicLikeLyrics endpoint.
/// </summary>
[Collection("Database")]
public class PublicLikeLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<LyricsEntity> SeedPublishedLyricsAsync()
    {
        return await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);

            LyricsEntity lyrics = LyricsFactory.CreatePublished(category.Id);
            ctx.Lyrics.Add(lyrics);
            return lyrics;
        });
    }

    [Fact]
    public async Task LikeLyrics_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync(Routes.Public.Lyrics.Likes(Guid.NewGuid()), null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LikeLyrics_AsVisitor_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync(Routes.Public.Lyrics.Likes(Guid.NewGuid()), null);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that liking a lyrics page creates the like row, increments the cached
    /// <c>LikeCount</c>, and returns success.
    /// </summary>
    [Fact]
    public async Task LikeLyrics_AsVisitor_WithValidLyrics_ReturnsOk()
    {
        LyricsEntity lyrics = await SeedPublishedLyricsAsync();
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync(Routes.Public.Lyrics.Likes(lyrics.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicLikeLyricsResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.LyricsLikes.AnyAsync(l => l.LyricsId == lyrics.Id && l.UserId == TestUser.VisitorId))
            .Should()
            .BeTrue();

        LyricsEntity? updated = await verifyDb.Lyrics.FindAsync(lyrics.Id);
        updated!.LikeCount.Should().Be(1);
    }

    /// <summary>
    /// Verifies that liking a lyrics page that is already liked returns 409 Conflict, and does
    /// not create a duplicate like row.
    /// </summary>
    [Fact]
    public async Task LikeLyrics_WhenAlreadyLiked_ReturnsConflict()
    {
        LyricsEntity lyrics = await SeedPublishedLyricsAsync();
        Client.AuthenticateAsVisitor();

        await Client.PostAsync(Routes.Public.Lyrics.Likes(lyrics.Id), null);

        var response = await Client.PostAsync(Routes.Public.Lyrics.Likes(lyrics.Id), null);

        await response.ShouldBeProblem(HttpStatusCode.Conflict);

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.LyricsLikes.CountAsync(l => l.LyricsId == lyrics.Id && l.UserId == TestUser.VisitorId))
            .Should()
            .Be(1);
    }
}
