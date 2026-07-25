using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedLyrics.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedLyrics.V1;

/// <summary>
/// Integration tests for the PublicGetPublishedLyrics endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetPublishedLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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

    /// <summary>
    /// Verifies that the published lyrics feed only returns Published lyrics, excluding
    /// Draft, PendingReview, and Rejected lyrics pages seeded alongside them.
    /// </summary>
    [Fact]
    public async Task GetPublishedLyrics_AsAnonymous_ReturnsOnlyPublishedLyrics()
    {
        Guid categoryId = await SeedCategoryAsync();

        LyricsEntity publishedLyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreatePublished(categoryId);
            ctx.Lyrics.Add(entity);
            return entity;
        });
        LyricsEntity draftLyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.Create(categoryId);
            ctx.Lyrics.Add(entity);
            return entity;
        });
        LyricsEntity pendingReviewLyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreatePendingReview(categoryId);
            ctx.Lyrics.Add(entity);
            return entity;
        });
        LyricsEntity rejectedLyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreateRejected(categoryId);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.Lyrics);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedLyricsResponse body = await response.ReadAsAsync<PublicGetPublishedLyricsResponse>();
        body.Lyrics.Items.Should().Contain(item => item.Id == publishedLyrics.Id);
        body.Lyrics.Items.Should().NotContain(item => item.Id == draftLyrics.Id);
        body.Lyrics.Items.Should().NotContain(item => item.Id == pendingReviewLyrics.Id);
        body.Lyrics.Items.Should().NotContain(item => item.Id == rejectedLyrics.Id);
        body.Lyrics.Items.Should().OnlyContain(item => item.Status == EnumContentStatus.Published);
        body.Lyrics.PageIndex.Should().Be(0);
        body.Lyrics.PageSize.Should().Be(10);
    }

    /// <summary>
    /// Verifies that the page index and page size query parameters correctly paginate
    /// the published lyrics feed.
    /// </summary>
    [Fact]
    public async Task GetPublishedLyrics_WithPagination_ReturnsRequestedPage()
    {
        Guid categoryId = await SeedCategoryAsync();
        await SeedAsync<ContentDbContext, List<LyricsEntity>>(ctx =>
        {
            List<LyricsEntity> entities = LyricsFactory.CreateManyPublished(categoryId, 5);
            ctx.Lyrics.AddRange(entities);
            return entities;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}?pageIndex=0&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedLyricsResponse body = await response.ReadAsAsync<PublicGetPublishedLyricsResponse>();
        body.Lyrics.Items.Should().HaveCount(2);
        body.Lyrics.PageSize.Should().Be(2);
        body.Lyrics.Count.Should().BeGreaterThanOrEqualTo(5);
    }

    /// <summary>
    /// Verifies that the search query parameter filters the published lyrics feed by
    /// song title, returning only lyrics whose song title matches the search term.
    /// </summary>
    [Fact]
    public async Task GetPublishedLyrics_WithSearchQuery_ReturnsFilteredResults()
    {
        Guid categoryId = await SeedCategoryAsync();

        LyricsEntity matchingLyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.Create(categoryId, "UniquePublicSearchTerm Song", "Test Artist");
            entity.MarkPendingReview();
            entity.Approve();
            entity.Publish();
            ctx.Lyrics.Add(entity);
            return entity;
        });
        LyricsEntity otherLyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreatePublished(categoryId);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}?search=UniquePublicSearchTerm");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedLyricsResponse body = await response.ReadAsAsync<PublicGetPublishedLyricsResponse>();
        body.Lyrics.Items.Should().Contain(item => item.Id == matchingLyrics.Id);
        body.Lyrics.Items.Should().NotContain(item => item.Id == otherLyrics.Id);
    }

    /// <summary>
    /// Verifies that the categoryId query parameter filters the published lyrics feed
    /// down to the given category.
    /// </summary>
    [Fact]
    public async Task GetPublishedLyrics_WithCategoryFilter_ReturnsOnlyMatchingLyrics()
    {
        Guid category1Id = await SeedCategoryAsync();
        Guid category2Id = await SeedCategoryAsync();

        LyricsEntity lyricsInCategory1 = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreatePublished(category1Id);
            ctx.Lyrics.Add(entity);
            return entity;
        });
        LyricsEntity lyricsInCategory2 = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreatePublished(category2Id);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}?categoryId={category1Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedLyricsResponse body = await response.ReadAsAsync<PublicGetPublishedLyricsResponse>();
        body.Lyrics.Items.Should().Contain(item => item.Id == lyricsInCategory1.Id);
        body.Lyrics.Items.Should().NotContain(item => item.Id == lyricsInCategory2.Id);
        body.Lyrics.Items.Should().OnlyContain(item => item.CategoryId == category1Id);
    }

    /// <summary>
    /// Verifies that the language query parameter filters the published lyrics feed by
    /// ISO 639-1 language code.
    /// </summary>
    [Fact]
    public async Task GetPublishedLyrics_WithLanguageFilter_ReturnsOnlyMatchingLyrics()
    {
        Guid categoryId = await SeedCategoryAsync();

        LyricsEntity frenchLyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreatePublished(categoryId);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}?language={frenchLyrics.Language}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedLyricsResponse body = await response.ReadAsAsync<PublicGetPublishedLyricsResponse>();
        body.Lyrics.Items.Should().Contain(item => item.Id == frenchLyrics.Id);
        body.Lyrics.Items.Should().OnlyContain(item => item.Language == frenchLyrics.Language);
    }

    /// <summary>
    /// The lyrics page liked by the current user resolves <c>IsLiked: true</c> on its summary,
    /// and the cached view/like/share counters pass through end to end through real Postgres.
    /// </summary>
    [Fact]
    public async Task GetPublishedLyrics_WhenCurrentUserHasLiked_ReturnsIsLikedTrueAndCounts()
    {
        Guid categoryId = await SeedCategoryAsync();

        LyricsEntity likedLyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreatePublished(categoryId);
            entity.IncrementViewCount();
            entity.IncrementViewCount();
            entity.IncrementViewCount();
            ctx.Lyrics.Add(entity);
            return entity;
        });
        LyricsEntity notLikedLyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreatePublished(categoryId);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.AuthenticateAsVisitor();
        await Client.PostAsync(Routes.Public.Lyrics.Likes(likedLyrics.Id), null);

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}?categoryId={categoryId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetPublishedLyricsResponse body = await response.ReadAsAsync<PublicGetPublishedLyricsResponse>();

        body.Lyrics.Items.Single(item => item.Id == likedLyrics.Id).IsLiked.Should().BeTrue();
        body.Lyrics.Items.Single(item => item.Id == likedLyrics.Id).ViewCount.Should().Be(3);
        body.Lyrics.Items.Single(item => item.Id == likedLyrics.Id).LikeCount.Should().Be(1);
        body.Lyrics.Items.Single(item => item.Id == notLikedLyrics.Id).IsLiked.Should().BeFalse();
    }

    /// <summary>
    /// An anonymous caller sees <c>IsLiked: false</c> on every summary in the feed.
    /// </summary>
    [Fact]
    public async Task GetPublishedLyrics_WhenAnonymous_ReturnsIsLikedFalseForAllItems()
    {
        Guid categoryId = await SeedCategoryAsync();

        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreatePublished(categoryId);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.AuthenticateAsVisitor();
        await Client.PostAsync(Routes.Public.Lyrics.Likes(lyrics.Id), null);
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}?categoryId={categoryId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetPublishedLyricsResponse body = await response.ReadAsAsync<PublicGetPublishedLyricsResponse>();
        body.Lyrics.Items.Should().OnlyContain(item => item.IsLiked == false);
    }
}
