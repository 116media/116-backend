using _116.Content.Application.Interactions.UseCases.Public.Commands.ShareLyrics.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.ShareLyrics.V1;

/// <summary>
/// Integration tests for the PublicShareLyrics endpoint.
/// </summary>
[Collection("Database")]
public class PublicShareLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task ShareLyrics_AsAnonymous_ReturnsOkAndIncrementsShareCount()
    {
        LyricsEntity lyrics = await SeedPublishedLyricsAsync();
        Client.ClearAuthentication();

        var response = await Client.PostAsync(Routes.Public.Lyrics.Shares(lyrics.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicShareLyricsResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.LyricsShares.AnyAsync(s => s.LyricsId == lyrics.Id && s.UserId == null)).Should().BeTrue();

        LyricsEntity? updated = await verifyDb.Lyrics.FindAsync(lyrics.Id);
        updated!.ShareCount.Should().Be(1);
    }

    [Fact]
    public async Task ShareLyrics_AsVisitor_ReturnsOkAndRecordsUserId()
    {
        LyricsEntity lyrics = await SeedPublishedLyricsAsync();
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync(Routes.Public.Lyrics.Shares(lyrics.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicShareLyricsResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.LyricsShares.AnyAsync(s => s.LyricsId == lyrics.Id && s.UserId == TestUser.VisitorId))
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task ShareLyrics_NonExistent_ReturnsNotFound()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync(Routes.Public.Lyrics.Shares(Guid.NewGuid()), null);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }
}
