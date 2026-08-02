using _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeLyrics.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.UnlikeLyrics.V1;

/// <summary>
/// Integration tests for the PublicUnlikeLyrics endpoint.
/// </summary>
[Collection("Database")]
public class PublicUnlikeLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task UnlikeLyrics_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync(Routes.Public.Lyrics.Likes(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UnlikeLyrics_AsVisitor_NonExistentLyrics_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(Routes.Public.Lyrics.Likes(Guid.NewGuid()));

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Unliking a lyrics page with no prior like row must reject rather than silently succeed.
    /// </summary>
    [Fact]
    public async Task UnlikeLyrics_WithoutPriorLike_ReturnsBadRequest()
    {
        LyricsEntity lyrics = await SeedPublishedLyricsAsync();
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(Routes.Public.Lyrics.Likes(lyrics.Id));

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that unliking a previously liked lyrics page removes the like row, decrements
    /// the cached <c>LikeCount</c>, and returns success.
    /// </summary>
    [Fact]
    public async Task UnlikeLyrics_WithPriorLike_ReturnsOkAndRemovesLike()
    {
        LyricsEntity lyrics = await SeedPublishedLyricsAsync();
        Client.AuthenticateAsVisitor();

        await Client.PostAsync(Routes.Public.Lyrics.Likes(lyrics.Id), null);

        var response = await Client.DeleteAsync(Routes.Public.Lyrics.Likes(lyrics.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicUnlikeLyricsResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.LyricsLikes.AnyAsync(l => l.LyricsId == lyrics.Id && l.UserId == TestUser.VisitorId))
            .Should()
            .BeFalse();

        LyricsEntity? updated = await verifyDb.Lyrics.FindAsync(lyrics.Id);
        updated!.LikeCount.Should().Be(0);
    }
}
