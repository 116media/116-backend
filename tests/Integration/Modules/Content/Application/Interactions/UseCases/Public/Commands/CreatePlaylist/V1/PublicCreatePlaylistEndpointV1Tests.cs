using _116.Content.Application.Interactions.UseCases.Public.Commands.CreatePlaylist.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.CreatePlaylist.V1;

/// <summary>
/// Integration tests for the PublicCreatePlaylist endpoint.
/// </summary>
[Collection("Database")]
public class PublicCreatePlaylistEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task CreatePlaylist_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        PublicCreatePlaylistRequest request = new PublicCreatePlaylistRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Public.Playlists, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePlaylist_AsVisitor_WithEmptyName_ReturnsValidationError()
    {
        Client.AuthenticateAsVisitor();
        PublicCreatePlaylistRequest request = new PublicCreatePlaylistRequestBuilder().WithName(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Public.Playlists, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Name", Localized<PlaylistErrorMessage>(m => m.NameRequired()))
        );
    }

    [Fact]
    public async Task CreatePlaylist_AsVisitor_WithValidData_ReturnsCreated()
    {
        Client.AuthenticateAsVisitor();
        PublicCreatePlaylistRequest request = new PublicCreatePlaylistRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Public.Playlists, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        PublicCreatePlaylistResponse body = await response.ReadAsAsync<PublicCreatePlaylistResponse>();
        body.Playlist.Id.Should().NotBeEmpty();
        body.Playlist.Name.Should().Be(request.Name);
        body.Playlist.VideoCount.Should().Be(0);

        await using ContentDbContext verifyDb = CreateDbContext<ContentDbContext>();
        PlaylistEntity? stored = await verifyDb.Playlists.FindAsync(body.Playlist.Id);
        stored.Should().NotBeNull();
        stored!.Name.Should().Be(request.Name);
        stored.UserId.Should().Be(TestUser.VisitorId);
    }
}
