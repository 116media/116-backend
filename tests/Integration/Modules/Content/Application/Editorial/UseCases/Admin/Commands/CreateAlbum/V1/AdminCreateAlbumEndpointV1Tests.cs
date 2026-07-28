using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateAlbum.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateAlbum.V1;

/// <summary>
/// Integration tests for the AdminCreateAlbum endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateAlbumEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreateAlbum_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Albums,
            new AdminCreateAlbumRequest("Album Name", null, null, null, EnumReleaseType.Album)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAlbum_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Albums,
            new AdminCreateAlbumRequest("Album Name", null, null, null, EnumReleaseType.Album)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateAlbum_AsSuperAdmin_WithValidData_ReturnsCreatedAndPersists()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Albums,
            new AdminCreateAlbumRequest("Le Grand Kalle", null, 1960, "Fiesta", EnumReleaseType.Album)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        AdminCreateAlbumResponse body = await response.ReadAsAsync<AdminCreateAlbumResponse>();
        body.Album.Name.Should().Be("Le Grand Kalle");
        body.Album.ReleaseYear.Should().Be(1960);
        body.Album.Label.Should().Be("Fiesta");

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        AlbumEntity? persisted = await ctx.Albums.FindAsync(body.Album.Id);
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAlbum_WithExistingArtistId_LinksArtistAndPersists()
    {
        ArtistEntity artist = await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ArtistEntity a = ArtistFactory.Create();
            ctx.Artists.Add(a);
            return a;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Albums,
            new AdminCreateAlbumRequest("Album Name", artist.Id, null, null, EnumReleaseType.Album)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        AdminCreateAlbumResponse body = await response.ReadAsAsync<AdminCreateAlbumResponse>();
        body.Album.ArtistId.Should().Be(artist.Id);
    }

    [Fact]
    public async Task CreateAlbum_WithNonExistentArtistId_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Albums,
            new AdminCreateAlbumRequest("Album Name", Guid.NewGuid(), null, null, EnumReleaseType.Album)
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAlbum_WithEmptyName_ReturnsValidationProblem()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Albums,
            new AdminCreateAlbumRequest(string.Empty, null, null, null, EnumReleaseType.Album)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
