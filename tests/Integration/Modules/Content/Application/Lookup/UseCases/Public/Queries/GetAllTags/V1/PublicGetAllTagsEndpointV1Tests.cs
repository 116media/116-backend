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

    [Fact]
    public async Task GetAllTags_RepeatedNoSearchRequests_ReturnConsistentBodies()
    {
        TagEntity tag = await SeedAsync<ContentDbContext, TagEntity>(ctx =>
        {
            TagEntity entity = TagFactory.Create("cache tag", "cache-tag");
            ctx.Tags.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var firstResponse = await Client.GetAsync(ApiRoutes.Public.Tags);
        var secondResponse = await Client.GetAsync(ApiRoutes.Public.Tags);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstBody = await firstResponse.ReadAsAsync<PublicGetAllTagsResponse>();
        var secondBody = await secondResponse.ReadAsAsync<PublicGetAllTagsResponse>();

        firstBody.Tags.Should().Contain(t => t.Id == tag.Id);
        secondBody.Tags.Select(t => t.Id).Should().Equal(firstBody.Tags.Select(t => t.Id));
    }

    [Fact]
    public async Task GetAllTags_WithLimitParam_RespectsLimit()
    {
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Tags.AddRange(
                TagFactory.Create("limit alpha", "limit-alpha"),
                TagFactory.Create("limit bravo", "limit-bravo"),
                TagFactory.Create("limit charlie", "limit-charlie")
            );
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Tags}?limit=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicGetAllTagsResponse>();
        body.Tags.Should().NotBeNull();
        body.Tags.Count.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetAllTags_WithLimitAndArticleContentType_RespectsLimit()
    {
        var (articleTag, _) = await SeedArticleAndVideoTagsAsync("limit-article-ct", "limit-video-ct");

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Tags}?limit=1&contentType=article");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicGetAllTagsResponse>();
        body.Tags.Should().NotBeNull();
        body.Tags.Count.Should().BeLessThanOrEqualTo(1);
        body.Tags.Should().OnlyContain(t => t.Id == articleTag.Id);
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
