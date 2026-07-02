using _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags.V1;

/// <summary>
/// Integration tests for the PublicGetAllTags endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetAllTagsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetAllTags_AsAnonymous_ReturnsSeededTag()
    {
        TagEntity tag = await SeedAsync<ContentDbContext, TagEntity>(ctx =>
        {
            TagEntity entity = TagFactory.Create();
            ctx.Tags.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.Tags);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicGetAllTagsResponse>();
        body.Tags.Should().Contain(t => t.Id == tag.Id && t.Name == tag.Name && t.Slug == tag.Slug);
    }

    [Fact]
    public async Task GetAllTags_WithSearchParam_ReturnsMatchingTags()
    {
        TagEntity matching = await SeedAsync<ContentDbContext, TagEntity>(ctx =>
        {
            TagEntity entity = TagFactory.Create("test tag", "test-tag");
            ctx.Tags.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Tags}?search=test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicGetAllTagsResponse>();
        body.Tags.Should().Contain(t => t.Id == matching.Id);
        body.Tags.Should()
            .OnlyContain(t =>
                t.Name.Contains("test", StringComparison.OrdinalIgnoreCase)
                || t.Slug.Contains("test", StringComparison.OrdinalIgnoreCase)
            );
    }

    [Fact]
    public async Task GetAllTags_WithArticleContentType_ReturnsOnlyArticleAssociatedTags()
    {
        var (articleTag, videoTag) = await SeedArticleAndVideoTagsAsync("article-ct", "video-ct");

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Tags}?contentType=article");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicGetAllTagsResponse>();
        body.Tags.Should().Contain(t => t.Id == articleTag.Id);
        body.Tags.Should().NotContain(t => t.Id == videoTag.Id);
    }

    [Fact]
    public async Task GetAllTags_WithVideoContentType_ReturnsOnlyVideoAssociatedTags()
    {
        var (articleTag, videoTag) = await SeedArticleAndVideoTagsAsync("article-only-ct", "video-only-ct");

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Tags}?contentType=video");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicGetAllTagsResponse>();
        body.Tags.Should().Contain(t => t.Id == videoTag.Id);
        body.Tags.Should().NotContain(t => t.Id == articleTag.Id);
    }

    [Fact]
    public async Task GetAllTags_WithInvalidContentType_FallsBackToAllTags()
    {
        var (articleTag, videoTag) = await SeedArticleAndVideoTagsAsync("article-fallback", "video-fallback");

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Tags}?contentType=nonsense");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicGetAllTagsResponse>();
        body.Tags.Should().Contain(t => t.Id == articleTag.Id);
        body.Tags.Should().Contain(t => t.Id == videoTag.Id);
    }

    private async Task<(TagEntity ArticleTag, TagEntity VideoTag)> SeedArticleAndVideoTagsAsync(
        string articleSlug,
        string videoSlug
    )
    {
        TagEntity articleTag = TagFactory.Create(articleSlug, articleSlug);
        TagEntity videoTag = TagFactory.Create(videoSlug, videoSlug);

        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ArticleEntity article = ArticleFactory.Create(category.Id);
            VideoEntity video = VideoFactory.Create(category.Id);

            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Articles.Add(article);
            ctx.Videos.Add(video);
            ctx.Tags.AddRange(articleTag, videoTag);
            ctx.ArticleTags.Add(ArticleTagEntity.Create(Guid.NewGuid(), article.Id, articleTag.Id));
            ctx.VideoTags.Add(VideoTagEntity.Create(Guid.NewGuid(), video.Id, videoTag.Id));
        });

        return (articleTag, videoTag);
    }
}
