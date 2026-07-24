using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArtist.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArtist.V1;

/// <summary>
/// Integration tests for the AdminUpdateArtist endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateArtistEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<ArtistEntity> SeedArtistAsync()
    {
        return await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ArtistEntity artist = ArtistFactory.Create();
            ctx.Artists.Add(artist);
            return artist;
        });
    }

    [Fact]
    public async Task UpdateArtist_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Artists}/{Guid.NewGuid()}",
            new AdminUpdateArtistRequest("Name", null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateArtist_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Artists}/{Guid.NewGuid()}",
            new AdminUpdateArtistRequest("Name", null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateArtist_AsSuperAdmin_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Artists}/{Guid.NewGuid()}",
            new AdminUpdateArtistRequest("Name", null)
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateArtist_AsSuperAdmin_WithValidData_ReturnsOkAndPersists()
    {
        ArtistEntity artist = await SeedArtistAsync();
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Artists}/{artist.Id}",
            new AdminUpdateArtistRequest("Updated Name", "Updated Bio")
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminUpdateArtistResponse body = await response.ReadAsAsync<AdminUpdateArtistResponse>();
        body.Artist.Name.Should().Be("Updated Name");
        body.Artist.Bio.Should().Be("Updated Bio");

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArtistEntity? persisted = await ctx.Artists.FindAsync(artist.Id);
        persisted!.Name.Should().Be("Updated Name");
    }

    /// <summary>
    /// The slug is immutable after creation — updating an artist's name/bio must never
    /// change the URL-safe slug used to address its public profile page.
    /// </summary>
    [Fact]
    public async Task UpdateArtist_ShouldNeverChangeSlug()
    {
        ArtistEntity artist = await SeedArtistAsync();
        string originalSlug = artist.Slug;
        Client.AuthenticateAsSuperAdmin();

        await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Artists}/{artist.Id}",
            new AdminUpdateArtistRequest("New Name", null)
        );

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArtistEntity? persisted = await ctx.Artists.FindAsync(artist.Id);
        persisted!.Slug.Should().Be(originalSlug);
    }

    [Fact]
    public async Task UpdateArtist_WithEmptyName_ReturnsValidationProblem()
    {
        ArtistEntity artist = await SeedArtistAsync();
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Artists}/{artist.Id}",
            new AdminUpdateArtistRequest(string.Empty, null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
