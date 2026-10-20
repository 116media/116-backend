using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedArticles.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;

namespace _116.Content.Integration.Tests.Application.Editorial.UseCases.Public.Queries.GetPublishedArticles.V1;

/// <summary>
/// Integration tests for the PublicGetPublishedArticles endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetPublishedArticlesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<Guid> SeedCategoryAsync() => await SeedAsync<ContentDbContext, Guid>(ContentSeeder.AddCategory);

    /// <summary>
    /// Seeds a category the slug filter can name, alongside the article filed under it and one
    /// filed elsewhere.
    /// </summary>
    private async Task<(
        ArticleEntity InCategory,
        ArticleEntity Elsewhere,
        CategoryEntity Category
    )> SeedCategorisedArticlesAsync()
    {
        Guid otherCategoryId = await SeedCategoryAsync();

        return await SeedAsync<ContentDbContext, (ArticleEntity, ArticleEntity, CategoryEntity)>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);

            ArticleEntity inCategory = ArticleFactory.CreatePublished(category.Id);
            ArticleEntity elsewhere = ArticleFactory.CreatePublished(otherCategoryId);
            ctx.Articles.AddRange(inCategory, elsewhere);

            return (inCategory, elsewhere, category);
        });
    }

    /// <summary>
    /// Seeds a tag carried by one published article and not by another.
    /// </summary>
    private async Task<(
        ArticleEntity TaggedArticle,
        ArticleEntity UntaggedArticle,
        TagEntity Tag
    )> SeedTaggedArticlesAsync()
    {
        Guid categoryId = await SeedCategoryAsync();

        return await SeedAsync<ContentDbContext, (ArticleEntity, ArticleEntity, TagEntity)>(ctx =>
        {
            ArticleEntity taggedArticle = ArticleFactory.CreatePublished(categoryId);
            ArticleEntity untaggedArticle = ArticleFactory.CreatePublished(categoryId);
            ctx.Articles.AddRange(taggedArticle, untaggedArticle);

            TagEntity tag = TagFactory.Create();
            ctx.Tags.Add(tag);
            taggedArticle.ReplaceTags([.. taggedArticle.Tags.Select(t => t.TagId), tag.Id]);

            return (taggedArticle, untaggedArticle, tag);
        });
    }

    [Fact]
    public async Task GetPublishedArticles_WithCategorySlug_ReturnsOnlyArticlesInThatCategory()
    {
        (ArticleEntity inCategory, ArticleEntity elsewhere, CategoryEntity category) =
            await SeedCategorisedArticlesAsync();

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}?categorySlug={category.Slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedArticlesResponse body = await response.ReadAsAsync<PublicGetPublishedArticlesResponse>();
        body.Articles.Items.Should().Contain(item => item.Id == inCategory.Id);
        body.Articles.Items.Should().NotContain(item => item.Id == elsewhere.Id);
    }

    [Fact]
    public async Task GetPublishedArticles_WithCategorySlugInDifferentCase_ReturnsArticlesInThatCategory()
    {
        (ArticleEntity inCategory, _, CategoryEntity category) = await SeedCategorisedArticlesAsync();

        Client.ClearAuthentication();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Public.Articles}?categorySlug={category.Slug.Value.ToUpperInvariant()}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedArticlesResponse body = await response.ReadAsAsync<PublicGetPublishedArticlesResponse>();
        body.Articles.Items.Should().Contain(item => item.Id == inCategory.Id);
    }

    [Fact]
    public async Task GetPublishedArticles_WithUnknownCategorySlug_ReturnsEmptyResult()
    {
        await SeedCategorisedArticlesAsync();

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}?categorySlug=slug-with-no-articles");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedArticlesResponse body = await response.ReadAsAsync<PublicGetPublishedArticlesResponse>();
        body.Articles.Items.Should().BeEmpty();
        body.Articles.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetPublishedArticles_WithTagSlug_ReturnsOnlyTaggedArticles()
    {
        (ArticleEntity taggedArticle, ArticleEntity untaggedArticle, TagEntity tag) = await SeedTaggedArticlesAsync();

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}?tagSlug={tag.Slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedArticlesResponse body = await response.ReadAsAsync<PublicGetPublishedArticlesResponse>();
        body.Articles.Items.Should().Contain(item => item.Id == taggedArticle.Id);
        body.Articles.Items.Should().NotContain(item => item.Id == untaggedArticle.Id);
    }

    [Fact]
    public async Task GetPublishedArticles_WithTagSlugInDifferentCase_ReturnsTaggedArticles()
    {
        (ArticleEntity taggedArticle, _, TagEntity tag) = await SeedTaggedArticlesAsync();

        Client.ClearAuthentication();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Public.Articles}?tagSlug={tag.Slug.Value.ToUpperInvariant()}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedArticlesResponse body = await response.ReadAsAsync<PublicGetPublishedArticlesResponse>();
        body.Articles.Items.Should().Contain(item => item.Id == taggedArticle.Id);
    }

    [Fact]
    public async Task GetPublishedArticles_WithUnknownTagSlug_ReturnsEmptyResult()
    {
        await SeedTaggedArticlesAsync();

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}?tagSlug=slug-with-no-articles");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedArticlesResponse body = await response.ReadAsAsync<PublicGetPublishedArticlesResponse>();
        body.Articles.Items.Should().BeEmpty();
        body.Articles.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetPublishedArticles_AsAnonymous_ReturnsOk()
    {
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity publishedArticle = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.CreatePublished(categoryId);
            ctx.Articles.Add(entity);
            return entity;
        });
        ArticleEntity draftArticle = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.Create(categoryId);
            ctx.Articles.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.Articles);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedArticlesResponse body = await response.ReadAsAsync<PublicGetPublishedArticlesResponse>();
        body.Articles.Items.Should().Contain(item => item.Id == publishedArticle.Id);
        body.Articles.Items.Should().NotContain(item => item.Id == draftArticle.Id);
        body.Articles.PageIndex.Should().Be(0);
        body.Articles.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetPublishedArticles_WhenAuthenticated_StampsOnlyTheUsersInteractions()
    {
        Guid categoryId = await SeedCategoryAsync();
        Guid userId = await SeedAuthenticatedUserAsync();
        List<ArticleEntity> articles = await SeedAsync<ContentDbContext, List<ArticleEntity>>(ctx =>
        {
            List<ArticleEntity> entities = ArticleFactory.CreateManyPublished(categoryId, 3);
            ctx.Articles.AddRange(entities);
            ctx.ArticleLikes.Add(ArticleLikeFactory.Create(userId, entities[0].Id));
            ctx.ArticleBookmarks.Add(ArticleBookmarkFactory.Create(userId, entities[1].Id));
            return entities;
        });

        Client.AuthenticateAs(userId, "Visitor");

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}?categoryId={categoryId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedArticlesResponse body = await response.ReadAsAsync<PublicGetPublishedArticlesResponse>();
        body.Articles.Items.Single(item => item.Id == articles[0].Id).IsLiked.Should().BeTrue();
        body.Articles.Items.Single(item => item.Id == articles[0].Id).IsBookmarked.Should().BeFalse();
        body.Articles.Items.Single(item => item.Id == articles[1].Id).IsBookmarked.Should().BeTrue();
        body.Articles.Items.Single(item => item.Id == articles[1].Id).IsLiked.Should().BeFalse();
        body.Articles.Items.Single(item => item.Id == articles[2].Id).IsLiked.Should().BeFalse();
        body.Articles.Items.Single(item => item.Id == articles[2].Id).IsBookmarked.Should().BeFalse();
    }

    [Fact]
    public async Task GetPublishedArticles_DoesNotLeakInteractionStateAcrossUsers()
    {
        Guid categoryId = await SeedCategoryAsync();
        Guid userAId = await SeedAuthenticatedUserAsync();
        Guid userBId = await SeedAuthenticatedUserAsync();
        ArticleEntity likedArticle = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.CreatePublished(categoryId);
            ctx.Articles.Add(entity);
            ctx.ArticleLikes.Add(ArticleLikeFactory.Create(userAId, entity.Id));
            return entity;
        });

        Client.AuthenticateAs(userAId, "Visitor");
        var responseA = await Client.GetAsync($"{ApiRoutes.Public.Articles}?categoryId={categoryId}");
        PublicGetPublishedArticlesResponse bodyA = await responseA.ReadAsAsync<PublicGetPublishedArticlesResponse>();
        bodyA.Articles.Items.Single(item => item.Id == likedArticle.Id).IsLiked.Should().BeTrue();

        Client.AuthenticateAs(userBId, "Visitor");
        var responseB = await Client.GetAsync($"{ApiRoutes.Public.Articles}?categoryId={categoryId}");
        PublicGetPublishedArticlesResponse bodyB = await responseB.ReadAsAsync<PublicGetPublishedArticlesResponse>();
        bodyB.Articles.Items.Single(item => item.Id == likedArticle.Id).IsLiked.Should().BeFalse();
    }
}
