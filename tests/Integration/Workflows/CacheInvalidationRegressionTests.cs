using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.SetLyricsTags.V1;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles.V1;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularVideos.V1;
using _116.Content.Application.Interactions.Constants;
using _116.Content.Application.Lookup.UseCases.Public.Queries.GetPopularTags.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// Regressions for the four cache-invalidation omissions the side-effects
/// audit proved: set-lyrics-tags, admin comment deletion, article deletion
/// and video deletion previously refreshed no cache at all. Each test warms
/// the relevant popular-content cache over real HTTP, makes the underlying
/// data stale through a raw database write that bypasses the event pipeline,
/// proves the cache still serves the stale projection, then drives the
/// previously-omitted mutation over HTTP and asserts the next read reflects
/// fresh data — the invalidation now rides the domain event.
/// </summary>
[Collection("Database")]
public class CacheInvalidationRegressionTests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task SetLyricsTags_RefreshesPopularTagsCache()
    {
        Guid categoryId = await SeedCategoryAsync();

        (LyricsEntity lyrics, TagEntity usedTag, TagEntity idleTag, ArticleEntity article) = await SeedAsync<
            ContentDbContext,
            (LyricsEntity, TagEntity, TagEntity, ArticleEntity)
        >(ctx =>
        {
            TagEntity usedTag = TagFactory.Create("ZTagUsed", "z-tag-used");
            TagEntity idleTag = TagFactory.Create("ATagIdle", "a-tag-idle");
            ArticleEntity article = ArticleFactory.CreatePublished(categoryId);
            LyricsEntity lyrics = LyricsFactory.Create(categoryId);

            ctx.Tags.AddRange(usedTag, idleTag);
            ctx.Articles.Add(article);
            ctx.ArticleTags.Add(ArticleTagEntity.Create(Guid.NewGuid(), article.Id, usedTag.Id));
            ctx.Lyrics.Add(lyrics);
            return (lyrics, usedTag, idleTag, article);
        });

        // Warm the popular-tags cache: the article-linked tag ranks first.
        Client.ClearAuthentication();
        var warm = await Client.GetAsync($"{ApiRoutes.Public.Tags}/popular");
        warm.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetPopularTagsResponse warmBody = await warm.ReadAsAsync<PublicGetPopularTagsResponse>();
        warmBody.Tags.First().Id.Should().Be(usedTag.Id);

        // Raw removal of the article-tag link bypasses the event pipeline, so
        // the cached ranking is now stale (both tags actually rank by name).
        await using (ContentDbContext ctx = CreateDbContext<ContentDbContext>())
        {
            ArticleTagEntity link = await ctx.ArticleTags.FirstAsync(t => t.ArticleId == article.Id);
            ctx.ArticleTags.Remove(link);
            await ctx.SaveChangesAsync();
        }

        var stale = await Client.GetAsync($"{ApiRoutes.Public.Tags}/popular");
        PublicGetPopularTagsResponse staleBody = await stale.ReadAsAsync<PublicGetPopularTagsResponse>();
        staleBody.Tags.First().Id.Should().Be(usedTag.Id);

        // Replacing the lyrics tag set over HTTP evicts the tags cache.
        Client.AuthenticateAsSuperAdmin();
        var setTags = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Tags(EditorialRouteConstants.Lyrics, lyrics.Id),
            new AdminSetLyricsTagsRequest([idleTag.Id])
        );
        setTags.StatusCode.Should().Be(HttpStatusCode.OK);

        Client.ClearAuthentication();
        var fresh = await Client.GetAsync($"{ApiRoutes.Public.Tags}/popular");
        PublicGetPopularTagsResponse freshBody = await fresh.ReadAsAsync<PublicGetPopularTagsResponse>();
        freshBody.Tags.First().Id.Should().Be(idleTag.Id);
    }

    [Fact]
    public async Task AdminDeleteArticleComment_DecrementsCounterAndRefreshesPopularArticlesCache()
    {
        Guid categoryId = await SeedCategoryAsync();

        // Seeding is reconstitution: the comment row and the counter it would
        // have produced are both part of the arrangement and are both stated
        // outright. Only the act below goes through the event path.
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.CreatePublished(categoryId);
            entity.IncrementCommentCount();
            ctx.Articles.Add(entity);
            return entity;
        });

        ArticleCommentEntity comment = await SeedAsync<ContentDbContext, ArticleCommentEntity>(ctx =>
        {
            ArticleCommentEntity entity = ArticleCommentFactory.Create(article.Id, TestUser.VisitorId);
            ctx.ArticleComments.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();
        var warm = await Client.GetAsync($"{Routes.Public.Articles.Popular()}?limit=10");
        warm.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetPopularArticlesResponse warmBody = await warm.ReadAsAsync<PublicGetPopularArticlesResponse>();
        warmBody.Articles.Single(a => a.Id == article.Id).CommentCount.Should().Be(1);
        warmBody.Articles.Single(a => a.Id == article.Id).LikeCount.Should().Be(0);

        // Raw like-count bump bypasses the event pipeline: the cache is stale.
        await using (ContentDbContext ctx = CreateDbContext<ContentDbContext>())
        {
            ArticleEntity tracked = await ctx.Articles.FirstAsync(a => a.Id == article.Id);
            tracked.IncrementLikeCount();
            tracked.IncrementLikeCount();
            tracked.IncrementLikeCount();
            await ctx.SaveChangesAsync();
        }

        var stale = await Client.GetAsync($"{Routes.Public.Articles.Popular()}?limit=10");
        PublicGetPopularArticlesResponse staleBody = await stale.ReadAsAsync<PublicGetPopularArticlesResponse>();
        staleBody.Articles.Single(a => a.Id == article.Id).LikeCount.Should().Be(0);

        // The admin comment deletion decrements the counter and evicts the
        // cache through the engagement event — the audited omission.
        Client.AuthenticateAsSuperAdmin();
        var delete = await Client.DeleteAsync(
            $"{ApiRoutes.Admin.Articles}/{article.Id}/{InteractionsRouteConstants.Comments}/{comment.Id}"
        );
        delete.StatusCode.Should().Be(HttpStatusCode.OK);

        await using (ContentDbContext verify = CreateDbContext<ContentDbContext>())
        {
            ArticleEntity persisted = await verify.Articles.FirstAsync(a => a.Id == article.Id);
            persisted.CommentCount.Should().Be(0);
        }

        Client.ClearAuthentication();
        var fresh = await Client.GetAsync($"{Routes.Public.Articles.Popular()}?limit=10");
        PublicGetPopularArticlesResponse freshBody = await fresh.ReadAsAsync<PublicGetPopularArticlesResponse>();
        freshBody.Articles.Single(a => a.Id == article.Id).CommentCount.Should().Be(0);
        freshBody.Articles.Single(a => a.Id == article.Id).LikeCount.Should().Be(3);
    }

    [Fact]
    public async Task AdminDeleteArticle_RefreshesPopularArticlesCache()
    {
        Guid categoryId = await SeedCategoryAsync();

        (ArticleEntity published, ArticleEntity draft) = await SeedAsync<
            ContentDbContext,
            (ArticleEntity, ArticleEntity)
        >(ctx =>
        {
            ArticleEntity published = ArticleFactory.CreatePublished(categoryId);
            published.IncrementLikeCount();
            ArticleEntity draft = ArticleFactory.Create(categoryId);
            ctx.Articles.AddRange(published, draft);
            return (published, draft);
        });

        Client.ClearAuthentication();
        var warm = await Client.GetAsync($"{Routes.Public.Articles.Popular()}?limit=10");
        warm.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetPopularArticlesResponse warmBody = await warm.ReadAsAsync<PublicGetPopularArticlesResponse>();
        warmBody.Articles.Single(a => a.Id == published.Id).LikeCount.Should().Be(1);

        // Raw like-count bump bypasses the event pipeline: the cache is stale.
        await using (ContentDbContext ctx = CreateDbContext<ContentDbContext>())
        {
            ArticleEntity tracked = await ctx.Articles.FirstAsync(a => a.Id == published.Id);
            while (tracked.LikeCount < 5)
            {
                tracked.IncrementLikeCount();
            }

            await ctx.SaveChangesAsync();
        }

        var stale = await Client.GetAsync($"{Routes.Public.Articles.Popular()}?limit=10");
        PublicGetPopularArticlesResponse staleBody = await stale.ReadAsAsync<PublicGetPopularArticlesResponse>();
        staleBody.Articles.Single(a => a.Id == published.Id).LikeCount.Should().Be(1);

        // Deleting the draft article evicts the cache through the deletion
        // event — the audited omission.
        Client.AuthenticateAsSuperAdmin();
        var delete = await Client.DeleteAsync($"{ApiRoutes.Admin.Articles}/{draft.Id}");
        delete.StatusCode.Should().Be(HttpStatusCode.OK);

        Client.ClearAuthentication();
        var fresh = await Client.GetAsync($"{Routes.Public.Articles.Popular()}?limit=10");
        PublicGetPopularArticlesResponse freshBody = await fresh.ReadAsAsync<PublicGetPopularArticlesResponse>();
        freshBody.Articles.Single(a => a.Id == published.Id).LikeCount.Should().Be(5);
    }

    [Fact]
    public async Task AdminDeleteVideo_RefreshesPopularVideosCache()
    {
        Guid categoryId = await SeedCategoryAsync();

        (VideoEntity published, VideoEntity draft) = await SeedAsync<ContentDbContext, (VideoEntity, VideoEntity)>(
            ctx =>
            {
                VideoEntity published = VideoFactory.CreatePublished(categoryId);
                published.IncrementShareCount();
                VideoEntity draft = VideoFactory.Create(categoryId);
                ctx.Videos.AddRange(published, draft);
                return (published, draft);
            }
        );

        Client.ClearAuthentication();
        var warm = await Client.GetAsync($"{Routes.Public.Videos.Popular()}?limit=10");
        warm.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetPopularVideosResponse warmBody = await warm.ReadAsAsync<PublicGetPopularVideosResponse>();
        warmBody.Videos.Single(v => v.Id == published.Id).ShareCount.Should().Be(1);

        // Raw share-count bump bypasses the event pipeline: the cache is stale.
        await using (ContentDbContext ctx = CreateDbContext<ContentDbContext>())
        {
            VideoEntity tracked = await ctx.Videos.FirstAsync(v => v.Id == published.Id);
            while (tracked.ShareCount < 4)
            {
                tracked.IncrementShareCount();
            }

            await ctx.SaveChangesAsync();
        }

        var stale = await Client.GetAsync($"{Routes.Public.Videos.Popular()}?limit=10");
        PublicGetPopularVideosResponse staleBody = await stale.ReadAsAsync<PublicGetPopularVideosResponse>();
        staleBody.Videos.Single(v => v.Id == published.Id).ShareCount.Should().Be(1);

        // Deleting the draft video evicts the cache through the deletion
        // event — the audited omission.
        Client.AuthenticateAsSuperAdmin();
        var delete = await Client.DeleteAsync($"{ApiRoutes.Admin.Videos}/{draft.Id}");
        delete.StatusCode.Should().Be(HttpStatusCode.OK);

        Client.ClearAuthentication();
        var fresh = await Client.GetAsync($"{Routes.Public.Videos.Popular()}?limit=10");
        PublicGetPopularVideosResponse freshBody = await fresh.ReadAsAsync<PublicGetPopularVideosResponse>();
        freshBody.Videos.Single(v => v.Id == published.Id).ShareCount.Should().Be(4);
    }
}
