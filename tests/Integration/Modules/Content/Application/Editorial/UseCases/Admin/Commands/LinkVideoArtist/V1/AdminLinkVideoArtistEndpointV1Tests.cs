using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.LinkVideoArtist.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.LinkVideoArtist.V1;

/// <summary>
/// Integration tests for the AdminLinkVideoArtist endpoint.
/// </summary>
[Collection("Database")]
public class AdminLinkVideoArtistEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<(VideoEntity Video, ArtistEntity Artist)> SeedVideoAndArtistAsync()
    {
        return await SeedAsync<ContentDbContext, (VideoEntity, ArtistEntity)>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            VideoEntity video = VideoFactory.Create(category.Id);
            ArtistEntity artist = ArtistFactory.Create();
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Videos.Add(video);
            ctx.Artists.Add(artist);
            return (video, artist);
        });
    }

    [Fact]
    public async Task LinkVideoArtist_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(
                EditorialRouteConstants.Videos,
                Guid.NewGuid(),
                EditorialRouteConstants.Artist
            ),
            new AdminLinkVideoArtistRequest(null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LinkVideoArtist_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(
                EditorialRouteConstants.Videos,
                Guid.NewGuid(),
                EditorialRouteConstants.Artist
            ),
            new AdminLinkVideoArtistRequest(null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task LinkVideoArtist_WithExistingArtist_LinksAndPersists()
    {
        (VideoEntity video, ArtistEntity artist) = await SeedVideoAndArtistAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(EditorialRouteConstants.Videos, video.Id, EditorialRouteConstants.Artist),
            new AdminLinkVideoArtistRequest(artist.Id)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminLinkVideoArtistResponse body = await response.ReadAsAsync<AdminLinkVideoArtistResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        VideoEntity? persisted = await ctx.Videos.FindAsync(video.Id);
        persisted!.ArtistId.Should().Be(artist.Id);
    }

    [Fact]
    public async Task LinkVideoArtist_WithNonExistentArtistId_ReturnsNotFound()
    {
        (VideoEntity video, _) = await SeedVideoAndArtistAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(EditorialRouteConstants.Videos, video.Id, EditorialRouteConstants.Artist),
            new AdminLinkVideoArtistRequest(Guid.NewGuid())
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task LinkVideoArtist_WithNullArtistId_UnlinksExistingArtist()
    {
        (VideoEntity video, ArtistEntity artist) = await SeedVideoAndArtistAsync();
        Client.AuthenticateAsAdmin();

        await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(EditorialRouteConstants.Videos, video.Id, EditorialRouteConstants.Artist),
            new AdminLinkVideoArtistRequest(artist.Id)
        );

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(EditorialRouteConstants.Videos, video.Id, EditorialRouteConstants.Artist),
            new AdminLinkVideoArtistRequest(null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        VideoEntity? persisted = await ctx.Videos.FindAsync(video.Id);
        persisted!.ArtistId.Should().BeNull();
    }

    [Fact]
    public async Task LinkVideoArtist_ReLinkToDifferentArtist_UpdatesToNewArtist()
    {
        (VideoEntity video, ArtistEntity firstArtist) = await SeedVideoAndArtistAsync();
        ArtistEntity secondArtist = await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ArtistEntity a = ArtistFactory.Create();
            ctx.Artists.Add(a);
            return a;
        });

        Client.AuthenticateAsAdmin();

        await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(EditorialRouteConstants.Videos, video.Id, EditorialRouteConstants.Artist),
            new AdminLinkVideoArtistRequest(firstArtist.Id)
        );

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(EditorialRouteConstants.Videos, video.Id, EditorialRouteConstants.Artist),
            new AdminLinkVideoArtistRequest(secondArtist.Id)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        VideoEntity? persisted = await ctx.Videos.FindAsync(video.Id);
        persisted!.ArtistId.Should().Be(secondArtist.Id);
    }
}
