using _116.Content.Application.Interactions.UseCases.Public.Commands.DeletePlaylist.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.DeletePlaylist.V1;

/// <summary>
/// Integration tests for the PublicDeletePlaylist endpoint.
/// </summary>
[Collection("Database")]
public class PublicDeletePlaylistEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<PlaylistEntity> SeedPlaylistAsync(Guid userId)
    {
        return await SeedAsync<ContentDbContext, PlaylistEntity>(ctx =>
        {
            PlaylistEntity playlist = PlaylistFactory.Create(userId);
            ctx.Playlists.Add(playlist);
            return playlist;
        });
    }

    [Fact]
    public async Task DeletePlaylist_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync(Routes.Public.Playlists.ById(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeletePlaylist_AsVisitor_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();
        var missingId = Guid.NewGuid();

        var response = await Client.DeleteAsync(Routes.Public.Playlists.ById(missingId));

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<PlaylistErrorMessage>(m => m.NotFound(missingId))
        );
    }

    [Fact]
    public async Task DeletePlaylist_AsVisitor_WhenNotOwner_ReturnsBadRequest()
    {
        PlaylistEntity playlist = await SeedPlaylistAsync(TestUser.AdminId);
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(Routes.Public.Playlists.ById(playlist.Id));

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<PlaylistErrorMessage>(m => m.NotOwner())
        );
    }

    [Fact]
    public async Task DeletePlaylist_AsOwner_RemovesPlaylist()
    {
        PlaylistEntity playlist = await SeedPlaylistAsync(TestUser.VisitorId);
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(Routes.Public.Playlists.ById(playlist.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicDeletePlaylistResponse body = await response.ReadAsAsync<PublicDeletePlaylistResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.Playlists.AnyAsync(p => p.Id == playlist.Id)).Should().BeFalse();
    }
}
