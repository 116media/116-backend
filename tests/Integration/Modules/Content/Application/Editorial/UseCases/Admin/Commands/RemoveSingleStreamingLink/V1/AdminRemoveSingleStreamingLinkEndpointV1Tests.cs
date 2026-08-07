using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.RemoveSingleStreamingLink.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RemoveSingleStreamingLink.V1;

/// <summary>
/// Integration tests for the AdminRemoveSingleStreamingLink endpoint.
/// </summary>
[Collection("Database")]
public class AdminRemoveSingleStreamingLinkEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// Builds the single streaming link route for the given lyrics page and platform.
    /// </summary>
    /// <param name="lyricsId">The standalone single the link belongs to.</param>
    /// <param name="platform">The streaming platform slot.</param>
    /// <returns>The fully qualified endpoint URL.</returns>
    private static string Url(Guid lyricsId, EnumStreamingPlatform platform) =>
        Routes.Admin.Editorial.StreamingLink(EditorialRouteConstants.Lyrics, lyricsId, platform.ToString());

    /// <summary>
    /// Seeds a standalone single carrying curated links on two distinct platform slots.
    /// </summary>
    /// <returns>The seeded lyrics page.</returns>
    private async Task<LyricsEntity> SeedSingleWithLinksAsync()
    {
        Guid categoryId = await SeedAsync<ContentDbContext, Guid>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);

            return category.Id;
        });

        return await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity lyrics = LyricsFactory.Create(categoryId);
            ctx.Lyrics.Add(lyrics);

            ctx.StreamingLinks.Add(StreamingLinkFactory.CreateForLyrics(lyrics.Id, EnumStreamingPlatform.Spotify));
            ctx.StreamingLinks.Add(StreamingLinkFactory.CreateForLyrics(lyrics.Id, EnumStreamingPlatform.Tidal));

            return lyrics;
        });
    }

    [Fact]
    public async Task RemoveSingleStreamingLink_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync(Url(Guid.NewGuid(), EnumStreamingPlatform.Spotify));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveSingleStreamingLink_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(Url(Guid.NewGuid(), EnumStreamingPlatform.Spotify));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveSingleStreamingLink_WhenLinkExists_DeletesOnlyThatPlatformRow()
    {
        LyricsEntity lyrics = await SeedSingleWithLinksAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.DeleteAsync(Url(lyrics.Id, EnumStreamingPlatform.Spotify));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminRemoveSingleStreamingLinkResponse body =
            await response.ReadAsAsync<AdminRemoveSingleStreamingLinkResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        List<EnumStreamingPlatform> remaining = await ctx
            .StreamingLinks.Where(link => link.LyricsId == lyrics.Id)
            .Select(link => link.Platform)
            .ToListAsync();

        remaining.Should().ContainSingle().Which.Should().Be(EnumStreamingPlatform.Tidal);
    }

    [Fact]
    public async Task RemoveSingleStreamingLink_WhenLinkAbsent_ReturnsOkAndChangesNothing()
    {
        LyricsEntity lyrics = await SeedSingleWithLinksAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.DeleteAsync(Url(lyrics.Id, EnumStreamingPlatform.AppleMusic));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminRemoveSingleStreamingLinkResponse body =
            await response.ReadAsAsync<AdminRemoveSingleStreamingLinkResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        int remaining = await ctx.StreamingLinks.CountAsync(link => link.LyricsId == lyrics.Id);

        remaining.Should().Be(2);
    }

    [Fact]
    public async Task RemoveSingleStreamingLink_WithNonExistentLyrics_ReturnsOk()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.DeleteAsync(Url(Guid.NewGuid(), EnumStreamingPlatform.Spotify));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminRemoveSingleStreamingLinkResponse body =
            await response.ReadAsAsync<AdminRemoveSingleStreamingLinkResponse>();
        body.IsSuccess.Should().BeTrue();
    }
}
