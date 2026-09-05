using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Covers the set-based engagement counters against a real database. The point of these is the
/// column mapping: a kind wired to the wrong column still satisfies every mocked unit test, so
/// each case asserts the intended counter moved <em>and</em> that its siblings did not.
/// </summary>
[Collection("Database")]
public class EngagementCounterTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private async Task<CategoryEntity> SeedCategoryAsync()
    {
        await using ContentDbContext seed = CreateDbContext<ContentDbContext>();
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        seed.ContentTypes.Add(contentType);
        seed.Categories.Add(category);
        await seed.SaveChangesAsync();

        return category;
    }

    private async Task<TEntity> ReadAsync<TEntity>(Guid id)
        where TEntity : class
    {
        await using ContentDbContext verify = CreateDbContext<ContentDbContext>();

        return (await verify.FindAsync<TEntity>(id))!;
    }

    #region Article

    private async Task<Guid> SeedArticleAsync()
    {
        CategoryEntity category = await SeedCategoryAsync();
        await using ContentDbContext seed = CreateDbContext<ContentDbContext>();
        ArticleEntity article = ArticleFactory.CreatePublished(category.Id);
        seed.Articles.Add(article);
        await seed.SaveChangesAsync();

        return article.Id;
    }

    [Theory]
    [InlineData(EnumEngagementKind.Like)]
    [InlineData(EnumEngagementKind.Bookmark)]
    [InlineData(EnumEngagementKind.Comment)]
    [InlineData(EnumEngagementKind.Share)]
    public async Task Article_ShouldMoveOnlyTheMappedCounter(EnumEngagementKind kind)
    {
        Guid articleId = await SeedArticleAsync();

        await Resolve<IArticleRepository>().ApplyEngagementDeltaAsync(articleId, kind, 1, CancellationToken.None);

        ArticleEntity a = await ReadAsync<ArticleEntity>(articleId);
        (
            kind switch
            {
                EnumEngagementKind.Like => a.LikeCount,
                EnumEngagementKind.Bookmark => a.BookmarkCount,
                EnumEngagementKind.Comment => a.CommentCount,
                _ => a.ShareCount,
            }
        )
            .Should()
            .Be(1);
        (a.LikeCount + a.BookmarkCount + a.CommentCount + a.ShareCount).Should().Be(1, "no sibling counter moves");
    }

    [Fact]
    public async Task Article_WhenDecrementedBelowZero_ShouldClampInSql()
    {
        Guid articleId = await SeedArticleAsync();
        IArticleRepository repository = Resolve<IArticleRepository>();
        await repository.ApplyEngagementDeltaAsync(articleId, EnumEngagementKind.Like, 1, CancellationToken.None);

        await repository.ApplyEngagementDeltaAsync(articleId, EnumEngagementKind.Like, -1, CancellationToken.None);
        await repository.ApplyEngagementDeltaAsync(articleId, EnumEngagementKind.Like, -1, CancellationToken.None);

        (await ReadAsync<ArticleEntity>(articleId)).LikeCount.Should().Be(0);
    }

    [Fact]
    public async Task Article_ForAKindItDoesNotCount_ShouldReportNoCounter()
    {
        Guid articleId = await SeedArticleAsync();

        int? updated = await Resolve<IArticleRepository>()
            .ApplyEngagementDeltaAsync(articleId, EnumEngagementKind.View, 1, CancellationToken.None);

        updated.Should().BeNull("articles carry no view counter, which is not a missing row");
    }

    [Fact]
    public async Task Article_WhenTheRowIsGone_ShouldReportZeroRows()
    {
        int? updated = await Resolve<IArticleRepository>()
            .ApplyEngagementDeltaAsync(Guid.NewGuid(), EnumEngagementKind.Like, 1, CancellationToken.None);

        updated.Should().Be(0);
    }

    [Fact]
    public async Task ArticleComment_ShouldMoveTheLikeCounterAndClamp()
    {
        Guid articleId = await SeedArticleAsync();
        ArticleCommentEntity comment = ArticleCommentFactory.Create(articleId, Guid.NewGuid());
        await using (ContentDbContext seed = CreateDbContext<ContentDbContext>())
        {
            seed.ArticleComments.Add(comment);
            await seed.SaveChangesAsync();
        }

        IArticleRepository repository = Resolve<IArticleRepository>();
        await repository.ApplyCommentLikeDeltaAsync(comment.Id, 1, CancellationToken.None);
        (await ReadAsync<ArticleCommentEntity>(comment.Id)).LikeCount.Should().Be(1);

        await repository.ApplyCommentLikeDeltaAsync(comment.Id, -1, CancellationToken.None);
        await repository.ApplyCommentLikeDeltaAsync(comment.Id, -1, CancellationToken.None);
        (await ReadAsync<ArticleCommentEntity>(comment.Id)).LikeCount.Should().Be(0);
    }

    #endregion

    #region Lyrics

    [Theory]
    [InlineData(EnumEngagementKind.Like)]
    [InlineData(EnumEngagementKind.Share)]
    [InlineData(EnumEngagementKind.View)]
    public async Task Lyrics_ShouldMoveOnlyTheMappedCounter(EnumEngagementKind kind)
    {
        CategoryEntity category = await SeedCategoryAsync();
        LyricsEntity lyrics = LyricsFactory.Create(category.Id);
        await using (ContentDbContext seed = CreateDbContext<ContentDbContext>())
        {
            seed.Lyrics.Add(lyrics);
            await seed.SaveChangesAsync();
        }

        await Resolve<ILyricsRepository>().ApplyEngagementDeltaAsync(lyrics.Id, kind, 1, CancellationToken.None);

        LyricsEntity l = await ReadAsync<LyricsEntity>(lyrics.Id);
        (
            kind switch
            {
                EnumEngagementKind.Like => l.LikeCount,
                EnumEngagementKind.Share => l.ShareCount,
                _ => l.ViewCount,
            }
        )
            .Should()
            .Be(1);
        (l.LikeCount + l.ShareCount + l.ViewCount).Should().Be(1, "no sibling counter moves");
    }

    [Fact]
    public async Task Lyrics_ForAKindItDoesNotCount_ShouldReportNoCounter()
    {
        CategoryEntity category = await SeedCategoryAsync();
        LyricsEntity lyrics = LyricsFactory.Create(category.Id);
        await using (ContentDbContext seed = CreateDbContext<ContentDbContext>())
        {
            seed.Lyrics.Add(lyrics);
            await seed.SaveChangesAsync();
        }

        int? updated = await Resolve<ILyricsRepository>()
            .ApplyEngagementDeltaAsync(lyrics.Id, EnumEngagementKind.Bookmark, 1, CancellationToken.None);

        updated.Should().BeNull("lyrics pages carry no bookmark counter");
    }

    #endregion

    #region Short video

    [Theory]
    [InlineData(EnumEngagementKind.Like)]
    [InlineData(EnumEngagementKind.Bookmark)]
    [InlineData(EnumEngagementKind.Share)]
    [InlineData(EnumEngagementKind.View)]
    public async Task ShortVideo_ShouldMoveOnlyTheMappedCounter(EnumEngagementKind kind)
    {
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        await using (ContentDbContext seed = CreateDbContext<ContentDbContext>())
        {
            seed.ShortVideos.Add(shortVideo);
            await seed.SaveChangesAsync();
        }

        await Resolve<IShortVideoRepository>()
            .ApplyEngagementDeltaAsync(shortVideo.Id, kind, 1, CancellationToken.None);

        ShortVideoEntity v = await ReadAsync<ShortVideoEntity>(shortVideo.Id);
        (
            kind switch
            {
                EnumEngagementKind.Like => v.LikeCount,
                EnumEngagementKind.Bookmark => v.BookmarkCount,
                EnumEngagementKind.Share => v.ShareCount,
                _ => v.ViewCount,
            }
        )
            .Should()
            .Be(1);
        (v.LikeCount + v.BookmarkCount + v.ShareCount + v.ViewCount).Should().Be(1, "no sibling counter moves");
    }

    [Theory]
    [InlineData(EnumEngagementKind.Comment)]
    [InlineData(EnumEngagementKind.Rating)]
    public async Task ShortVideo_ForAKindItDoesNotCount_ShouldReportNoCounter(EnumEngagementKind kind)
    {
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        await using (ContentDbContext seed = CreateDbContext<ContentDbContext>())
        {
            seed.ShortVideos.Add(shortVideo);
            await seed.SaveChangesAsync();
        }

        int? updated = await Resolve<IShortVideoRepository>()
            .ApplyEngagementDeltaAsync(shortVideo.Id, kind, 1, CancellationToken.None);

        updated.Should().BeNull("short videos are neither commented on nor rated");
    }

    #endregion

    #region Video

    private async Task<Guid> SeedVideoAsync()
    {
        CategoryEntity category = await SeedCategoryAsync();
        await using ContentDbContext seed = CreateDbContext<ContentDbContext>();
        VideoEntity video = VideoFactory.Create(category.Id);
        seed.Videos.Add(video);
        await seed.SaveChangesAsync();

        return video.Id;
    }

    [Fact]
    public async Task Video_ShouldMoveTheShareCounter()
    {
        Guid videoId = await SeedVideoAsync();

        await Resolve<IVideoRepository>()
            .ApplyEngagementDeltaAsync(videoId, EnumEngagementKind.Share, 1, CancellationToken.None);

        (await ReadAsync<VideoEntity>(videoId)).ShareCount.Should().Be(1);
    }

    [Theory]
    [InlineData(EnumEngagementKind.Like)]
    [InlineData(EnumEngagementKind.Bookmark)]
    [InlineData(EnumEngagementKind.View)]
    public async Task Video_ForAKindItDoesNotCount_ShouldReportNoCounter(EnumEngagementKind kind)
    {
        Guid videoId = await SeedVideoAsync();

        int? updated = await Resolve<IVideoRepository>()
            .ApplyEngagementDeltaAsync(videoId, kind, 1, CancellationToken.None);

        updated.Should().BeNull("videos count shares only");
    }

    [Fact]
    public async Task Video_SetRating_ShouldWriteBothColumns()
    {
        Guid videoId = await SeedVideoAsync();

        await Resolve<IVideoRepository>().SetRatingAsync(videoId, 4.25m, 8, CancellationToken.None);

        VideoEntity video = await ReadAsync<VideoEntity>(videoId);
        video.RatingAverage.Should().Be(4.25m);
        video.RatingCount.Should().Be(8);
    }

    [Fact]
    public async Task Video_SetRating_WhenTheRowIsGone_ShouldReportZeroRows()
    {
        int updated = await Resolve<IVideoRepository>().SetRatingAsync(Guid.NewGuid(), 1m, 1, CancellationToken.None);

        updated.Should().Be(0);
    }

    #endregion
}
