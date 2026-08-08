using _116.Content.Application.Interactions.UseCases.Public.Commands.RenamePlaylist.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.RenamePlaylist.V1;

/// <summary>
/// Integration tests for the PublicRenamePlaylist endpoint.
/// </summary>
[Collection("Database")]
public class PublicRenamePlaylistEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task RenamePlaylist_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        PublicRenamePlaylistRequest request = new PublicRenamePlaylistRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync(Routes.Public.Playlists.ById(Guid.NewGuid()), request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RenamePlaylist_AsVisitor_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();
        PublicRenamePlaylistRequest request = new PublicRenamePlaylistRequestBuilder().Build();
        var missingId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync(Routes.Public.Playlists.ById(missingId), request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<PlaylistErrorMessage>(m => m.NotFound(missingId))
        );
    }

    [Fact]
    public async Task RenamePlaylist_AsVisitor_NotOwner_ReturnsBadRequest()
    {
        PlaylistEntity playlist = await SeedPlaylistAsync(TestUser.AdminId);
        Client.AuthenticateAsVisitor();
        PublicRenamePlaylistRequest request = new PublicRenamePlaylistRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync(Routes.Public.Playlists.ById(playlist.Id), request);

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<PlaylistErrorMessage>(m => m.NotOwner())
        );
    }

    [Fact]
    public async Task RenamePlaylist_AsOwner_UpdatesName()
    {
        PlaylistEntity playlist = await SeedPlaylistAsync(TestUser.VisitorId);
        Client.AuthenticateAsVisitor();
        PublicRenamePlaylistRequest request = new PublicRenamePlaylistRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync(Routes.Public.Playlists.ById(playlist.Id), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicRenamePlaylistResponse body = await response.ReadAsAsync<PublicRenamePlaylistResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext verifyDb = CreateDbContext<ContentDbContext>();
        PlaylistEntity? stored = await verifyDb.Playlists.FindAsync(playlist.Id);
        stored!.Name.Should().Be(request.Name);
    }
}
