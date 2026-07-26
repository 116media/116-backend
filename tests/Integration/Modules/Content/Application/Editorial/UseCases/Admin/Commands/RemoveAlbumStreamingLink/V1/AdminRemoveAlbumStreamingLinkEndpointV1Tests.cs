using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.RemoveAlbumStreamingLink.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RemoveAlbumStreamingLink.V1;

/// <summary>
/// Integration tests for the AdminRemoveAlbumStreamingLink endpoint.
/// </summary>
[Collection("Database")]
public class AdminRemoveAlbumStreamingLinkEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// Builds the album streaming link route for the given album and platform.
    /// </summary>
    /// <param name="albumId">The album the link belongs to.</param>
    /// <param name="platform">The streaming platform slot.</param>
    /// <returns>The fully qualified endpoint URL.</returns>
    private static string Url(Guid albumId, EnumStreamingPlatform platform) =>
        Routes.Admin.Editorial.StreamingLink(EditorialRouteConstants.Albums, albumId, platform.ToString());

    /// <summary>
    /// Seeds a standalone album carrying curated links on two distinct platform slots.
    /// </summary>
    /// <returns>The seeded album.</returns>
    private async Task<AlbumEntity> SeedAlbumWithLinksAsync()
    {
        return await SeedAsync<ContentDbContext, AlbumEntity>(ctx =>
        {
            AlbumEntity album = AlbumFactory.Create();
            ctx.Albums.Add(album);

            ctx.StreamingLinks.Add(StreamingLinkFactory.CreateForAlbum(album.Id, EnumStreamingPlatform.Spotify));
            ctx.StreamingLinks.Add(StreamingLinkFactory.CreateForAlbum(album.Id, EnumStreamingPlatform.Tidal));

            return album;
        });
    }

    [Fact]
    public async Task RemoveAlbumStreamingLink_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync(Url(Guid.NewGuid(), EnumStreamingPlatform.Spotify));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveAlbumStreamingLink_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(Url(Guid.NewGuid(), EnumStreamingPlatform.Spotify));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Removing an existing curated link deletes only that platform's row, leaving the album's
    /// other platform slots intact.
    /// </summary>
    [Fact]
    public async Task RemoveAlbumStreamingLink_WhenLinkExists_DeletesOnlyThatPlatformRow()
    {
        AlbumEntity album = await SeedAlbumWithLinksAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.DeleteAsync(Url(album.Id, EnumStreamingPlatform.Spotify));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminRemoveAlbumStreamingLinkResponse body =
            await response.ReadAsAsync<AdminRemoveAlbumStreamingLinkResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        List<EnumStreamingPlatform> remaining = await ctx
            .StreamingLinks.Where(link => link.AlbumId == album.Id)
            .Select(link => link.Platform)
            .ToListAsync();

        remaining.Should().ContainSingle().Which.Should().Be(EnumStreamingPlatform.Tidal);
    }

    /// <summary>
    /// Removing a platform slot that holds no curated link is a no-op success — the absence of
    /// a row is a valid state, since the public endpoint falls back to a generated search URL.
    /// </summary>
    [Fact]
    public async Task RemoveAlbumStreamingLink_WhenLinkAbsent_ReturnsOkAndChangesNothing()
    {
        AlbumEntity album = await SeedAlbumWithLinksAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.DeleteAsync(Url(album.Id, EnumStreamingPlatform.AppleMusic));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminRemoveAlbumStreamingLinkResponse body =
            await response.ReadAsAsync<AdminRemoveAlbumStreamingLinkResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        int remaining = await ctx.StreamingLinks.CountAsync(link => link.AlbumId == album.Id);

        remaining.Should().Be(2);
    }

    /// <summary>
    /// The endpoint does not require the album itself to exist: an unknown album resolves to no
    /// curated link and therefore takes the same no-op success path.
    /// </summary>
    [Fact]
    public async Task RemoveAlbumStreamingLink_WithNonExistentAlbum_ReturnsOk()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.DeleteAsync(Url(Guid.NewGuid(), EnumStreamingPlatform.Spotify));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminRemoveAlbumStreamingLinkResponse body =
            await response.ReadAsAsync<AdminRemoveAlbumStreamingLinkResponse>();
        body.IsSuccess.Should().BeTrue();
    }
}
