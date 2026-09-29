using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Messages;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArtist.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Builders.Requests;
using _116.Content.TestData.Factories;

namespace _116.Content.Integration.Tests.Application.Editorial.UseCases.Admin.Commands.UpdateArtist.V1;

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
            new AdminUpdateArtistRequestBuilder().Build()
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateArtist_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Artists}/{Guid.NewGuid()}",
            new AdminUpdateArtistRequestBuilder().Build()
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateArtist_AsSuperAdmin_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Artists}/{Guid.NewGuid()}",
            new AdminUpdateArtistRequestBuilder().Build()
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Artist"))
        );
    }

    [Fact]
    public async Task UpdateArtist_AsSuperAdmin_WithValidData_ReturnsOkAndPersists()
    {
        ArtistEntity artist = await SeedArtistAsync();
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Artists}/{artist.Id}",
            new AdminUpdateArtistRequestBuilder().WithName("Updated Name").WithBio("Updated Bio").Build()
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminUpdateArtistResponse body = await response.ReadAsAsync<AdminUpdateArtistResponse>();
        body.Artist.Name.Should().Be("Updated Name");
        body.Artist.Bio.Should().Be("Updated Bio");

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArtistEntity? persisted = await ctx.Artists.FindAsync(artist.Id);
        persisted!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateArtist_ShouldNeverChangeSlug()
    {
        ArtistEntity artist = await SeedArtistAsync();
        string originalSlug = artist.Slug;
        Client.AuthenticateAsSuperAdmin();

        await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Artists}/{artist.Id}",
            new AdminUpdateArtistRequestBuilder().WithName("New Name").Build()
        );

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArtistEntity? persisted = await ctx.Artists.FindAsync(artist.Id);
        persisted!.Slug.Value.Should().Be(originalSlug);
    }

    [Fact]
    public async Task UpdateArtist_WithEmptyName_ReturnsValidationProblem()
    {
        ArtistEntity artist = await SeedArtistAsync();
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Artists}/{artist.Id}",
            new AdminUpdateArtistRequestBuilder().WithName(string.Empty).Build()
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
