using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoBySlug.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetVideoBySlug.V1;

/// <summary>
/// Integration tests for the PublicGetVideoBySlug endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetVideoBySlugEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<Guid> SeedCategoryAsync()
    {
        return await SeedAsync<ContentDbContext, Guid>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);

            return category.Id;
        });
    }

    [Fact]
    public async Task GetVideoBySlug_WithPublishedVideo_ReturnsVideo()
    {
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity video = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity entity = VideoFactory.CreatePublished(categoryId);
            ctx.Videos.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Videos}/{video.Slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetVideoBySlugResponse body = await response.ReadAsAsync<PublicGetVideoBySlugResponse>();
        body.Video.Id.Should().Be(video.Id);
        body.Video.Slug.Should().Be(video.Slug);
        body.Video.Title.Should().Be(video.Title);
    }

    /// <summary>
    /// Verifies that a non-published (draft) video is excluded from public reads
    /// and the endpoint reports it as not found.
    /// </summary>
    [Fact]
    public async Task GetVideoBySlug_WithDraftVideo_ReturnsNotFound()
    {
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity video = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity entity = VideoFactory.Create(categoryId);
            ctx.Videos.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Videos}/{video.Slug}");

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetVideoBySlug_WithNonExistent_ReturnsNotFound()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Videos}/non-existent-slug");

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    #region Artist Slug Resolution

    /// <summary>
    /// A video linked to an artist profile resolves the artist's slug server-side, mirroring
    /// the lyrics detail response — the client never turns a name into a slug itself.
    /// </summary>
    [Fact]
    public async Task GetVideoBySlug_WithLinkedArtist_ReturnsArtistSlug()
    {
        Guid categoryId = await SeedCategoryAsync();
        string artistSlug = $"linked-{Guid.NewGuid():N}";

        VideoEntity video = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            ArtistEntity artist = ArtistFactory.CreateWithSlug(artistSlug);
            VideoEntity entity = VideoFactory.CreatePublished(categoryId);
            entity.LinkArtist(artist.Id);

            ctx.Artists.Add(artist);
            ctx.Videos.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Videos}/{video.Slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetVideoBySlugResponse body = await response.ReadAsAsync<PublicGetVideoBySlugResponse>();
        body.ArtistSlug.Should().Be(artistSlug);
    }

    [Fact]
    public async Task GetVideoBySlug_WithNoLinkedArtist_ReturnsNullArtistSlug()
    {
        Guid categoryId = await SeedCategoryAsync();

        VideoEntity video = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity entity = VideoFactory.CreatePublished(categoryId);
            ctx.Videos.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Videos}/{video.Slug}");

        PublicGetVideoBySlugResponse body = await response.ReadAsAsync<PublicGetVideoBySlugResponse>();
        body.ArtistSlug.Should().BeNull();
    }

    #endregion
}
