using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedVideos.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;

namespace _116.Content.Integration.Tests.Application.Editorial.UseCases.Public.Queries.GetPublishedVideos.V1;

/// <summary>
/// Integration tests for the PublicGetPublishedVideos endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetPublishedVideosEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<Guid> SeedCategoryAsync() => await SeedAsync<ContentDbContext, Guid>(ContentSeeder.AddCategory);

    [Fact]
    public async Task GetPublishedVideos_AsAnonymous_ReturnsOk()
    {
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity publishedVideo = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity entity = VideoFactory.CreatePublished(categoryId);
            ctx.Videos.Add(entity);
            return entity;
        });
        VideoEntity draftVideo = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity entity = VideoFactory.Create(categoryId);
            ctx.Videos.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.Videos);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedVideosResponse body = await response.ReadAsAsync<PublicGetPublishedVideosResponse>();
        body.Videos.Items.Should().Contain(item => item.Id == publishedVideo.Id);
        body.Videos.Items.Should().NotContain(item => item.Id == draftVideo.Id);
        body.Videos.PageIndex.Should().Be(0);
        body.Videos.PageSize.Should().Be(10);
    }

    /// <summary>
    /// Seeds a category the slug filter can name, alongside the video filed under it and one
    /// filed elsewhere.
    /// </summary>
    private async Task<(
        VideoEntity InCategory,
        VideoEntity Elsewhere,
        CategoryEntity Category
    )> SeedCategorisedVideosAsync()
    {
        Guid otherCategoryId = await SeedCategoryAsync();

        return await SeedAsync<ContentDbContext, (VideoEntity, VideoEntity, CategoryEntity)>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);

            VideoEntity inCategory = VideoFactory.CreatePublished(category.Id);
            VideoEntity elsewhere = VideoFactory.CreatePublished(otherCategoryId);
            ctx.Videos.AddRange(inCategory, elsewhere);

            return (inCategory, elsewhere, category);
        });
    }

    [Fact]
    public async Task GetPublishedVideos_WithCategorySlug_ReturnsOnlyVideosInThatCategory()
    {
        (VideoEntity inCategory, VideoEntity elsewhere, CategoryEntity category) = await SeedCategorisedVideosAsync();

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Videos}?categorySlug={category.Slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedVideosResponse body = await response.ReadAsAsync<PublicGetPublishedVideosResponse>();
        body.Videos.Items.Should().Contain(item => item.Id == inCategory.Id);
        body.Videos.Items.Should().NotContain(item => item.Id == elsewhere.Id);
    }

    [Fact]
    public async Task GetPublishedVideos_WithCategorySlugInDifferentCase_ReturnsVideosInThatCategory()
    {
        (VideoEntity inCategory, _, CategoryEntity category) = await SeedCategorisedVideosAsync();

        Client.ClearAuthentication();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Public.Videos}?categorySlug={category.Slug.Value.ToUpperInvariant()}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedVideosResponse body = await response.ReadAsAsync<PublicGetPublishedVideosResponse>();
        body.Videos.Items.Should().Contain(item => item.Id == inCategory.Id);
    }

    [Fact]
    public async Task GetPublishedVideos_WithUnknownCategorySlug_ReturnsEmptyResult()
    {
        await SeedCategorisedVideosAsync();

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Videos}?categorySlug=slug-with-no-videos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedVideosResponse body = await response.ReadAsAsync<PublicGetPublishedVideosResponse>();
        body.Videos.Items.Should().BeEmpty();
        body.Videos.Count.Should().Be(0);
    }

    private async Task<(VideoEntity TaggedVideo, VideoEntity UntaggedVideo, TagEntity Tag)> SeedTaggedVideosAsync()
    {
        Guid categoryId = await SeedCategoryAsync();

        return await SeedAsync<ContentDbContext, (VideoEntity, VideoEntity, TagEntity)>(ctx =>
        {
            VideoEntity taggedVideo = VideoFactory.CreatePublished(categoryId);
            VideoEntity untaggedVideo = VideoFactory.CreatePublished(categoryId);
            ctx.Videos.AddRange(taggedVideo, untaggedVideo);

            TagEntity tag = TagFactory.Create();
            ctx.Tags.Add(tag);
            taggedVideo.ReplaceTags([.. taggedVideo.Tags.Select(t => t.TagId), tag.Id]);

            return (taggedVideo, untaggedVideo, tag);
        });
    }

    [Fact]
    public async Task GetPublishedVideos_WithTagSlug_ReturnsOnlyTaggedVideos()
    {
        (VideoEntity taggedVideo, VideoEntity untaggedVideo, TagEntity tag) = await SeedTaggedVideosAsync();

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Videos}?tagSlug={tag.Slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedVideosResponse body = await response.ReadAsAsync<PublicGetPublishedVideosResponse>();
        body.Videos.Items.Should().Contain(item => item.Id == taggedVideo.Id);
        body.Videos.Items.Should().NotContain(item => item.Id == untaggedVideo.Id);
    }

    [Fact]
    public async Task GetPublishedVideos_WithTagSlugInDifferentCase_ReturnsTaggedVideos()
    {
        (VideoEntity taggedVideo, _, TagEntity tag) = await SeedTaggedVideosAsync();

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Videos}?tagSlug={tag.Slug.Value.ToUpperInvariant()}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedVideosResponse body = await response.ReadAsAsync<PublicGetPublishedVideosResponse>();
        body.Videos.Items.Should().Contain(item => item.Id == taggedVideo.Id);
    }

    [Fact]
    public async Task GetPublishedVideos_WithUnknownTagSlug_ReturnsEmptyResult()
    {
        await SeedTaggedVideosAsync();

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Videos}?tagSlug=slug-with-no-videos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedVideosResponse body = await response.ReadAsAsync<PublicGetPublishedVideosResponse>();
        body.Videos.Items.Should().BeEmpty();
        body.Videos.Count.Should().Be(0);
    }
}
