using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Search;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Builders.Entities;
using _116.Content.TestData.Factories;
using _116.Tests.Fixtures.Stubs;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Integration.Tests.Infrastructure.Search;

/// <summary>
/// Integration tests for <see cref="ISearchCatalog" /> and the registered providers behind
/// it, covering ranked matching, accent folding, visibility filtering, paging, the merged
/// All scope and the cached per-scope counts against real PostgreSQL.
/// </summary>
[Collection("Database")]
public class SearchCatalogTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    /// <summary>
    /// Seeds a content type and a category, returning the category every seeded item belongs to.
    /// </summary>
    private async Task<CategoryEntity> SeedCategoryAsync()
    {
        await using var context = CreateDbContext<ContentDbContext>();

        ContentTypeEntity contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        return category;
    }

    private async Task SeedArticlesAsync(params ArticleEntity[] articles)
    {
        await using (var context = CreateDbContext<ContentDbContext>())
        {
            context.Articles.AddRange(articles);
            await context.SaveChangesAsync();
        }

        await RebuildSearchAsync();
    }

    private async Task SeedVideosAsync(params VideoEntity[] videos)
    {
        await using (var context = CreateDbContext<ContentDbContext>())
        {
            context.Videos.AddRange(videos);
            await context.SaveChangesAsync();
        }

        await RebuildSearchAsync();
    }

    private async Task SeedShortsAsync(params ShortVideoEntity[] shorts)
    {
        await using (var context = CreateDbContext<ContentDbContext>())
        {
            context.ShortVideos.AddRange(shorts);
            await context.SaveChangesAsync();
        }

        await RebuildSearchAsync();
    }

    /// <summary>
    /// Projects the seeded rows into the search documents. Seeding writes straight to the
    /// context, so the domain events that normally drive the projection never fire.
    /// </summary>
    private Task RebuildSearchAsync() => Resolve<ISearchProjection>().RebuildAllAsync();

    /// <summary>
    /// Sets the engagement figures on a seeded row. They are raised by interactions rather than by a
    /// factory, so a read-side test arranges them directly.
    /// </summary>
    private async Task SetArticleEngagementAsync(Guid id, int likes, int comments, int shares, int bookmarks)
    {
        await using var context = CreateDbContext<ContentDbContext>();

        await context
            .Articles.Where(article => article.Id == id)
            .ExecuteUpdateAsync(setters =>
                setters
                    .SetProperty(article => article.LikeCount, likes)
                    .SetProperty(article => article.CommentCount, comments)
                    .SetProperty(article => article.ShareCount, shares)
                    .SetProperty(article => article.BookmarkCount, bookmarks)
            );
    }

    [Fact]
    public async Task SearchArticlesAsync_WithATermInTheTitle_ReturnsTheArticleWithAPositiveScore()
    {
        CategoryEntity category = await SeedCategoryAsync();
        await SeedArticlesAsync(
            new ArticleBuilder(category.Id)
                .WithTitle("La rumba congolaise et ses racines")
                .WithSlug("rumba-congolaise-racines")
                .AsPublished()
                .Build()
        );

        var catalog = Resolve<ISearchCatalog>();
        IReadOnlyList<SearchHitDto> hits = await catalog.SearchAsync(
            EnumSearchScope.Articles,
            "rumba",
            skip: 0,
            take: 20
        );

        hits.Should().ContainSingle();
        hits[0].Slug.Should().Be("rumba-congolaise-racines");
        hits[0].Score.Should().BeGreaterThan(0);
        hits[0].CategoryId.Should().Be(category.Id);
    }

    [Fact]
    public async Task SearchArticlesAsync_WithAnUnaccentedTerm_StillMatchesTheAccentedTitle()
    {
        CategoryEntity category = await SeedCategoryAsync();
        await SeedArticlesAsync(
            new ArticleBuilder(category.Id)
                .WithTitle("Une melodie inoubliable")
                .WithSlug("melodie-inoubliable")
                .AsPublished()
                .Build()
        );

        var catalog = Resolve<ISearchCatalog>();
        IReadOnlyList<SearchHitDto> hits = await catalog.SearchAsync(
            EnumSearchScope.Articles,
            "melodie",
            skip: 0,
            take: 20
        );

        hits.Should().ContainSingle(hit => hit.Slug == "melodie-inoubliable");
    }

    [Fact]
    public async Task SearchArticlesAsync_WithADraftArticle_DoesNotReturnIt()
    {
        CategoryEntity category = await SeedCategoryAsync();
        await SeedArticlesAsync(
            new ArticleBuilder(category.Id).WithTitle("Brouillon sur le sebene").WithSlug("brouillon-sebene").Build()
        );

        var catalog = Resolve<ISearchCatalog>();
        IReadOnlyList<SearchHitDto> hits = await catalog.SearchAsync(
            EnumSearchScope.Articles,
            "sebene",
            skip: 0,
            take: 20
        );

        hits.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchArticlesAsync_WithATitleAndABodyMatch_RanksTheTitleMatchFirst()
    {
        CategoryEntity category = await SeedCategoryAsync();
        await SeedArticlesAsync(
            new ArticleBuilder(category.Id)
                .WithTitle("Sebene toute la nuit")
                .WithSlug("sebene-toute-la-nuit")
                .AsPublished()
                .Build(),
            new ArticleBuilder(category.Id)
                .WithTitle("Chronique de la semaine")
                .WithSlug("chronique-semaine")
                .WithBody("Le resume de la semaine.", "Un long texte qui mentionne le sebene une seule fois.")
                .AsPublished()
                .Build()
        );

        var catalog = Resolve<ISearchCatalog>();
        IReadOnlyList<SearchHitDto> hits = await catalog.SearchAsync(
            EnumSearchScope.Articles,
            "sebene",
            skip: 0,
            take: 20
        );

        hits.Should().HaveCount(2);
        hits[0].Slug.Should().Be("sebene-toute-la-nuit");
        hits[0].Score.Should().BeGreaterThan(hits[1].Score);
    }

    [Fact]
    public async Task SearchArticlesAsync_WithASkipAndTake_PagesThroughTheRankedHits()
    {
        CategoryEntity category = await SeedCategoryAsync();
        await SeedArticlesAsync(
            new ArticleBuilder(category.Id).WithTitle("Ndombolo un").WithSlug("ndombolo-un").AsPublished().Build(),
            new ArticleBuilder(category.Id).WithTitle("Ndombolo deux").WithSlug("ndombolo-deux").AsPublished().Build(),
            new ArticleBuilder(category.Id).WithTitle("Ndombolo trois").WithSlug("ndombolo-trois").AsPublished().Build()
        );

        var catalog = Resolve<ISearchCatalog>();
        IReadOnlyList<SearchHitDto> firstPage = await catalog.SearchAsync(
            EnumSearchScope.Articles,
            "ndombolo",
            skip: 0,
            take: 2
        );
        IReadOnlyList<SearchHitDto> secondPage = await catalog.SearchAsync(
            EnumSearchScope.Articles,
            "ndombolo",
            skip: 2,
            take: 2
        );

        firstPage.Should().HaveCount(2);
        secondPage.Should().ContainSingle();
        secondPage.Select(hit => hit.Slug).Should().NotIntersectWith(firstPage.Select(hit => hit.Slug));
    }

    [Fact]
    public async Task SearchVideosAsync_WithATermInTheDescription_ReturnsThePublishedVideo()
    {
        CategoryEntity category = await SeedCategoryAsync();
        await SeedVideosAsync(
            new VideoBuilder(category.Id)
                .WithTitle("Session live")
                .WithDescription("Une captation du groupe en pleine repetition soukous.")
                .WithSlug("session-live-soukous")
                .AsPublished()
                .Build(),
            new VideoBuilder(category.Id).WithTitle("Teaser soukous").WithSlug("teaser-soukous-brouillon").Build()
        );

        var catalog = Resolve<ISearchCatalog>();
        IReadOnlyList<SearchHitDto> hits = await catalog.SearchAsync(
            EnumSearchScope.Videos,
            "soukous",
            skip: 0,
            take: 20
        );

        hits.Should().ContainSingle(hit => hit.Slug == "session-live-soukous");
    }

    [Fact]
    public async Task SearchShortsAsync_WithATermInTheTitle_ReturnsTheActiveShort()
    {
        await SeedShortsAsync(new ShortVideoBuilder().WithTitle("Teaser mutuashi").WithSlug("teaser-mutuashi").Build());

        var catalog = Resolve<ISearchCatalog>();
        IReadOnlyList<SearchHitDto> hits = await catalog.SearchAsync(
            EnumSearchScope.Shorts,
            "mutuashi",
            skip: 0,
            take: 20
        );

        hits.Should().ContainSingle();
        hits[0].Slug.Should().Be("teaser-mutuashi");
        hits[0].Type.Should().Be(EnumCoreContentType.Short);
        hits[0].Score.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SearchShortsAsync_ForAHit_CarriesNoCategoryExcerptOrRating()
    {
        await SeedShortsAsync(new ShortVideoBuilder().WithTitle("Coulisses mutuashi").WithSlug("coulisses").Build());

        var catalog = Resolve<ISearchCatalog>();
        IReadOnlyList<SearchHitDto> hits = await catalog.SearchAsync(
            EnumSearchScope.Shorts,
            "mutuashi",
            skip: 0,
            take: 20
        );

        hits[0].CategoryId.Should().BeNull();
        hits[0].CategoryName.Should().BeNull();
        hits[0].Excerpt.Should().BeEmpty();
        hits[0].Engagement.RatingAverage.Should().BeNull();
        hits[0].PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchShortsAsync_WithAnInactiveShort_DoesNotReturnIt()
    {
        await SeedShortsAsync(
            new ShortVideoBuilder().WithTitle("Archive mutuashi").WithSlug("archive-mutuashi").AsInactive().Build()
        );

        var catalog = Resolve<ISearchCatalog>();
        IReadOnlyList<SearchHitDto> hits = await catalog.SearchAsync(
            EnumSearchScope.Shorts,
            "mutuashi",
            skip: 0,
            take: 20
        );

        hits.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchShortsAsync_WithAnUnaccentedTerm_StillMatchesTheAccentedTitle()
    {
        await SeedShortsAsync(
            new ShortVideoBuilder().WithTitle("Repetition generale").WithSlug("repetition-generale").Build()
        );

        var catalog = Resolve<ISearchCatalog>();
        IReadOnlyList<SearchHitDto> hits = await catalog.SearchAsync(
            EnumSearchScope.Shorts,
            "repetition",
            skip: 0,
            take: 20
        );

        hits.Should().ContainSingle(hit => hit.Slug == "repetition-generale");
    }

    [Fact]
    public async Task CountAsync_WithMatchesInBothScopes_CountsEachScopeAndTheirSum()
    {
        CategoryEntity category = await SeedCategoryAsync();
        await SeedArticlesAsync(
            new ArticleBuilder(category.Id)
                .WithTitle("Kwassa en studio")
                .WithSlug("kwassa-studio")
                .AsPublished()
                .Build()
        );
        await SeedVideosAsync(
            new VideoBuilder(category.Id).WithTitle("Kwassa live").WithSlug("kwassa-live").AsPublished().Build(),
            new VideoBuilder(category.Id).WithTitle("Kwassa backstage").WithSlug("kwassa-backstage").Build()
        );
        await SeedShortsAsync(
            new ShortVideoBuilder().WithTitle("Kwassa teaser").WithSlug("kwassa-teaser").Build(),
            new ShortVideoBuilder().WithTitle("Kwassa retire").WithSlug("kwassa-retire").AsInactive().Build()
        );

        var catalog = Resolve<ISearchCatalog>();
        SearchCountsDto counts = await catalog.CountAsync("kwassa");

        counts.For(EnumSearchScope.Articles).Should().Be(1);
        counts.For(EnumSearchScope.Videos).Should().Be(1);
        counts.For(EnumSearchScope.Shorts).Should().Be(1);
        counts.All.Should().Be(3);
    }

    [Fact]
    public async Task CountAsync_CalledTwiceForATerm_ServesTheSecondCallFromCache()
    {
        CategoryEntity category = await SeedCategoryAsync();
        await SeedArticlesAsync(
            new ArticleBuilder(category.Id).WithTitle("Soukous un").WithSlug("soukous-un").AsPublished().Build()
        );

        var catalog = Resolve<ISearchCatalog>();
        SearchCountsDto first = await catalog.CountAsync("soukous");

        // Raw SQL, so the removal raises no domain event and evicts no cache tag.
        await using (var context = CreateDbContext<ContentDbContext>())
        {
            await context.Database.ExecuteSqlRawAsync("DELETE FROM content.articles WHERE slug = 'soukous-un';");
        }

        SearchCountsDto second = await catalog.CountAsync("soukous");

        first.For(EnumSearchScope.Articles).Should().Be(1);
        second
            .For(EnumSearchScope.Articles)
            .Should()
            .Be(1, "the second call is answered from the cached count, not the emptied table");
    }

    [Fact]
    public async Task CountAsync_AfterAnArticleIsPublished_ReportsTheNewTotal()
    {
        CategoryEntity category = await SeedCategoryAsync();
        await SeedArticlesAsync(
            new ArticleBuilder(category.Id).WithTitle("Lingala un").WithSlug("lingala-un").AsPublished().Build()
        );

        var catalog = Resolve<ISearchCatalog>();
        SearchCountsDto before = await catalog.CountAsync("lingala");

        await SeedArticlesAsync(
            new ArticleBuilder(category.Id).WithTitle("Lingala deux").WithSlug("lingala-deux").AsPublished().Build()
        );

        SearchCountsDto after = await catalog.CountAsync("lingala");

        before.For(EnumSearchScope.Articles).Should().Be(1);
        after.For(EnumSearchScope.Articles).Should().Be(2, "publishing evicts the cached counts by tag");
    }

    [Fact]
    public async Task CountAsync_ForTheSameTermInAnotherCasing_SharesTheCachedCount()
    {
        CategoryEntity category = await SeedCategoryAsync();
        await SeedArticlesAsync(
            new ArticleBuilder(category.Id).WithTitle("Zouk un").WithSlug("zouk-un").AsPublished().Build()
        );

        var catalog = Resolve<ISearchCatalog>();

        SearchCountsDto lower = await catalog.CountAsync("zouk");
        SearchCountsDto upper = await catalog.CountAsync("ZOUK");

        upper.Should().BeEquivalentTo(lower);
    }

    [Fact]
    public async Task SearchAsync_WithAnEnglishStemOfAnEnglishTitle_StillMatches()
    {
        CategoryEntity category = await SeedCategoryAsync();
        await SeedArticlesAsync(
            new ArticleBuilder(category.Id)
                .WithTitle("Recording sessions at the studio")
                .WithSlug("recording-sessions")
                .AsPublished()
                .Build()
        );

        var catalog = Resolve<ISearchCatalog>();
        IReadOnlyList<SearchHitDto> hits = await catalog.SearchAsync(
            EnumSearchScope.Articles,
            "record",
            skip: 0,
            take: 20
        );

        hits.Should().ContainSingle(hit => hit.Slug == "recording-sessions");
    }

    [Fact]
    public async Task SearchAsync_WithAFrenchStemOfAFrenchTitle_StillMatches()
    {
        CategoryEntity category = await SeedCategoryAsync();
        await SeedArticlesAsync(
            new ArticleBuilder(category.Id)
                .WithTitle("Les chanteurs congolais")
                .WithSlug("chanteurs-congolais")
                .AsPublished()
                .Build()
        );

        var catalog = Resolve<ISearchCatalog>();
        IReadOnlyList<SearchHitDto> hits = await catalog.SearchAsync(
            EnumSearchScope.Articles,
            "chanteur",
            skip: 0,
            take: 20
        );

        hits.Should().ContainSingle(hit => hit.Slug == "chanteurs-congolais");
    }

    [Fact]
    public async Task SearchArticlesAsync_WithATermThatMatchesNothing_ReturnsEmpty()
    {
        CategoryEntity category = await SeedCategoryAsync();
        await SeedArticlesAsync(
            new ArticleBuilder(category.Id).WithTitle("Rumba odyssee").WithSlug("rumba-odyssee").AsPublished().Build()
        );

        var catalog = Resolve<ISearchCatalog>();
        IReadOnlyList<SearchHitDto> hits = await catalog.SearchAsync(
            EnumSearchScope.Articles,
            "zzzznotaterm",
            skip: 0,
            take: 20
        );

        hits.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_ForAnArticle_CarriesItsEngagementFigures()
    {
        CategoryEntity category = await SeedCategoryAsync();
        ArticleEntity article = new ArticleBuilder(category.Id)
            .WithTitle("Soukous et engagement")
            .WithSlug("soukous-engagement")
            .AsPublished()
            .Build();
        await SeedArticlesAsync(article);
        await SetArticleEngagementAsync(article.Id, likes: 12, comments: 5, shares: 3, bookmarks: 7);

        var catalog = Resolve<ISearchCatalog>();
        IReadOnlyList<SearchHitDto> hits = await catalog.SearchAsync(
            EnumSearchScope.Articles,
            "soukous",
            skip: 0,
            take: 20
        );

        SearchHitEngagementDto engagement = hits.Should().ContainSingle().Subject.Engagement;

        engagement.LikeCount.Should().Be(12);
        engagement.CommentCount.Should().Be(5);
        engagement.ShareCount.Should().Be(3);
        engagement.BookmarkCount.Should().Be(7);
        engagement.ViewCount.Should().BeNull("an article keeps no view count");
        engagement.RatingCount.Should().BeNull("an article keeps no ratings");
        engagement.RatingAverage.Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_ForAVideo_CarriesOnlyTheFiguresAVideoKeeps()
    {
        CategoryEntity category = await SeedCategoryAsync();
        await SeedVideosAsync(
            new VideoBuilder(category.Id).WithTitle("Ndombolo live").WithSlug("ndombolo-live").AsPublished().Build()
        );

        var catalog = Resolve<ISearchCatalog>();
        IReadOnlyList<SearchHitDto> hits = await catalog.SearchAsync(
            EnumSearchScope.Videos,
            "ndombolo",
            skip: 0,
            take: 20
        );

        SearchHitEngagementDto engagement = hits.Should().ContainSingle().Subject.Engagement;

        engagement.ShareCount.Should().NotBeNull();
        engagement.RatingCount.Should().NotBeNull();
        engagement.RatingAverage.Should().NotBeNull("the average and its count belong together");
        engagement.CommentCount.Should().BeNull("a video keeps no comment count");
        engagement.LikeCount.Should().BeNull("a video is rated rather than liked");
        engagement.ViewCount.Should().BeNull();
        engagement.BookmarkCount.Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_ForAShort_CarriesOnlyTheFiguresAShortKeeps()
    {
        await SeedShortsAsync(new ShortVideoBuilder().WithTitle("Kwassa teaser").WithSlug("kwassa-teaser").Build());

        var catalog = Resolve<ISearchCatalog>();
        IReadOnlyList<SearchHitDto> hits = await catalog.SearchAsync(
            EnumSearchScope.Shorts,
            "kwassa",
            skip: 0,
            take: 20
        );

        SearchHitEngagementDto engagement = hits.Should().ContainSingle().Subject.Engagement;

        engagement.ViewCount.Should().NotBeNull();
        engagement.LikeCount.Should().NotBeNull();
        engagement.ShareCount.Should().NotBeNull();
        engagement.BookmarkCount.Should().NotBeNull();
        engagement.CommentCount.Should().BeNull("a short keeps no comment count");
        engagement.RatingCount.Should().BeNull("a short is not rated");
        engagement.RatingAverage.Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_AcrossEveryType_ReadsThePageAndItsEngagementInOneStatement()
    {
        CategoryEntity category = await SeedCategoryAsync();
        await SeedArticlesAsync(
            new ArticleBuilder(category.Id).WithTitle("Mutuashi roots").WithSlug("mutuashi-roots").AsPublished().Build()
        );
        await SeedVideosAsync(
            new VideoBuilder(category.Id).WithTitle("Mutuashi live").WithSlug("mutuashi-live").AsPublished().Build()
        );
        await SeedShortsAsync(new ShortVideoBuilder().WithTitle("Mutuashi clip").WithSlug("mutuashi-clip").Build());

        var recorder = Api.Services.GetRequiredService<RecordingCommandInterceptor>();
        recorder.Reset();

        var catalog = Resolve<ISearchCatalog>();
        IReadOnlyList<SearchHitDto> hits = await catalog.SearchAsync(
            EnumSearchScope.All,
            "mutuashi",
            skip: 0,
            take: 20
        );

        hits.Should().HaveCount(3);
        hits.Should().AllSatisfy(hit => hit.Engagement.ShareCount.Should().NotBeNull());

        // The guard against a per-row lookup: three types, three joins, one statement.
        recorder.Commands.Should().ContainSingle("the page and its figures are one round trip");
        recorder.Commands[0].Should().Contain("LEFT JOIN", "the figures are joined, not fetched per row");
    }
}
