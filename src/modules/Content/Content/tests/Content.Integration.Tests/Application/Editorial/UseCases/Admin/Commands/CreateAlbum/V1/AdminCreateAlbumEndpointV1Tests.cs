using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Messages;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateAlbum.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Builders.Requests;
using _116.Content.TestData.Factories;

namespace _116.Content.Integration.Tests.Application.Editorial.UseCases.Admin.Commands.CreateAlbum.V1;

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
            new AdminCreateAlbumRequestBuilder().Build()
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAlbum_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Albums,
            new AdminCreateAlbumRequestBuilder().Build()
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateAlbum_AsSuperAdmin_WithValidData_ReturnsCreatedAndPersists()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Albums,
            new AdminCreateAlbumRequestBuilder()
                .WithName("Le Grand Kalle")
                .WithReleaseYear(1960)
                .WithLabel("Fiesta")
                .Build()
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
            new AdminCreateAlbumRequestBuilder().WithArtistId(artist.Id).Build()
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
            new AdminCreateAlbumRequestBuilder().WithArtistId(Guid.NewGuid()).Build()
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Artist"))
        );
    }

    [Fact]
    public async Task CreateAlbum_WithEmptyName_ReturnsValidationProblem()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Albums,
            new AdminCreateAlbumRequestBuilder().WithName(string.Empty).Build()
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAlbum_WithAReleaseTypeOutsideTheEnum_ReturnsTheLocalizedTypeError()
    {
        Client.AuthenticateAsSuperAdmin();

        // An out-of-range integer binds to the enum, so the rule guards what the binder allows.
        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Albums,
            new
            {
                Name = "Album Name",
                ArtistId = (Guid?)null,
                ReleaseYear = (short?)null,
                Label = (string?)null,
                ReleaseType = 999,
            }
        );

        await response.ShouldBeValidationProblem(
            "ReleaseType",
            Localized<AlbumErrorMessage>(m => m.InvalidReleaseType())
        );
    }
}
