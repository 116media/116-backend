using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist.V1;

/// <summary>
/// Integration tests for the AdminCreateArtist endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateArtistEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreateArtist_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequest("Fally Ipupa", "fally-ipupa", null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateArtist_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequest("Fally Ipupa", "fally-ipupa", null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateArtist_AsSuperAdmin_WithValidData_ReturnsCreatedAndPersists()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequest("Fally Ipupa", "fally-ipupa", "Congolese singer.")
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        AdminCreateArtistResponse body = await response.ReadAsAsync<AdminCreateArtistResponse>();
        body.Artist.Name.Should().Be("Fally Ipupa");
        body.Artist.Slug.Should().Be("fally-ipupa");
        body.Artist.Bio.Should().Be("Congolese singer.");

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArtistEntity? persisted = await ctx.Artists.FindAsync(body.Artist.Id);
        persisted.Should().NotBeNull();
        persisted!.UserId.Should().BeNull();
    }

    /// <summary>
    /// Creating an artist profile with a slug that already exists returns a conflict rather
    /// than silently creating a duplicate profile.
    /// </summary>
    [Fact]
    public async Task CreateArtist_WithDuplicateSlug_ReturnsConflict()
    {
        await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ArtistEntity artist = ArtistFactory.CreateWithSlug("fally-ipupa");
            ctx.Artists.Add(artist);
            return artist;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequest("Fally Ipupa Copy", "fally-ipupa", null)
        );

        await response.ShouldBeProblem(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateArtist_WithEmptyName_ReturnsValidationProblem()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequest(string.Empty, "some-slug", null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
