using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Builders;
using _116.Content.Infrastructure.Builders.Contracts;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Factories;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Content.Unit.Tests.Infrastructure.Builders;

/// <summary>
/// Unit tests for <see cref="TagsQueryBuilder"/>, the one builder serving both the alphabetical
/// vocabulary and the most-used ranking. Uses an InMemory <see cref="ContentDbContext"/> so the
/// association filters, the ordering and the per-kind counts run without a real database. The
/// ILike-based search filter is PostgreSQL-only, so search stays covered by the integration
/// repository tests.
/// </summary>
public class TagsQueryBuilderTests : IDisposable
{
    private readonly ContentDbContext _context;

    public TagsQueryBuilderTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<TagEntity> SeedTagAsync(string name)
    {
        TagEntity tag = TagFactory.Create(name, name.ToLowerInvariant().Replace(" ", "-"));
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return tag;
    }

    private async Task SeedArticleTagAsync(Guid tagId)
    {
        _context.ArticleTags.Add(ArticleTagFactory.Create(Guid.NewGuid(), tagId));
        await _context.SaveChangesAsync();
    }

    private async Task SeedVideoTagAsync(Guid tagId)
    {
        _context.VideoTags.Add(VideoTagFactory.Create(Guid.NewGuid(), tagId));
        await _context.SaveChangesAsync();
    }

    private async Task SeedLyricsTagAsync(Guid tagId)
    {
        _context.LyricsTags.Add(LyricsTagFactory.Create(Guid.NewGuid(), tagId));
        await _context.SaveChangesAsync();
    }

    #region Content-type filter

    [Fact]
    public async Task Build_WithNullContentType_ShouldReturnTheWholeVocabulary()
    {
        await SeedTagAsync("alpha");
        await SeedTagAsync("beta");

        List<TagCountRow> result = await new TagsQueryBuilder().Build(_context).ToListAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Build_WithArticleContentType_ShouldReturnOnlyArticleAssociatedTags()
    {
        TagEntity articleTag = await SeedTagAsync("article-tag");
        TagEntity videoTag = await SeedTagAsync("video-tag");
        await SeedTagAsync("unused-tag");
        await SeedArticleTagAsync(articleTag.Id);
        await SeedVideoTagAsync(videoTag.Id);

        List<TagCountRow> result = await new TagsQueryBuilder()
            .WithContentType(EnumCoreContentType.Article)
            .Build(_context)
            .ToListAsync();

        result.Should().ContainSingle();
        result[0].Tag.Id.Should().Be(articleTag.Id);
    }

    [Fact]
    public async Task Build_WithVideoContentType_ShouldReturnOnlyVideoAssociatedTags()
    {
        TagEntity articleTag = await SeedTagAsync("article-tag");
        TagEntity videoTag = await SeedTagAsync("video-tag");
        await SeedArticleTagAsync(articleTag.Id);
        await SeedVideoTagAsync(videoTag.Id);

        List<TagCountRow> result = await new TagsQueryBuilder()
            .WithContentType(EnumCoreContentType.Video)
            .Build(_context)
            .ToListAsync();

        result.Should().ContainSingle();
        result[0].Tag.Id.Should().Be(videoTag.Id);
    }

    [Fact]
    public async Task Build_WithLyricsContentType_ShouldReturnOnlyLyricsAssociatedTags()
    {
        TagEntity lyricsTag = await SeedTagAsync("lyrics-tag");
        TagEntity videoTag = await SeedTagAsync("video-tag");
        await SeedLyricsTagAsync(lyricsTag.Id);
        await SeedVideoTagAsync(videoTag.Id);

        List<TagCountRow> result = await new TagsQueryBuilder()
            .WithContentType(EnumCoreContentType.Lyrics)
            .Build(_context)
            .ToListAsync();

        result.Should().ContainSingle();
        result[0].Tag.Id.Should().Be(lyricsTag.Id);
    }

    [Theory]
    [InlineData(EnumCoreContentType.Short)]
    [InlineData(EnumCoreContentType.Custom)]
    public async Task Build_WithUntaggableContentType_ShouldReturnNothing(EnumCoreContentType contentType)
    {
        TagEntity tag = await SeedTagAsync("alpha");
        await SeedArticleTagAsync(tag.Id);

        List<TagCountRow> result = await new TagsQueryBuilder()
            .WithContentType(contentType)
            .Build(_context)
            .ToListAsync();

        result.Should().BeEmpty();
    }

    #endregion

    #region Ordering

    [Fact]
    public async Task Build_WithNameOrder_ShouldOrderAlphabetically()
    {
        TagEntity zebra = await SeedTagAsync("Zebra");
        TagEntity alpha = await SeedTagAsync("Alpha");

        List<TagCountRow> result = await new TagsQueryBuilder()
            .WithOrder(EnumTagOrder.Name)
            .Build(_context)
            .ToListAsync();

        result[0].Tag.Id.Should().Be(alpha.Id);
        result[1].Tag.Id.Should().Be(zebra.Id);
    }

    [Fact]
    public async Task Build_WithUsageOrder_ShouldRankTheMostUsedFirst()
    {
        TagEntity quiet = await SeedTagAsync("Alpha");
        TagEntity busy = await SeedTagAsync("Zebra");
        await SeedArticleTagAsync(busy.Id);
        await SeedVideoTagAsync(busy.Id);
        await SeedArticleTagAsync(quiet.Id);

        List<TagCountRow> result = await new TagsQueryBuilder()
            .WithOrder(EnumTagOrder.Usage)
            .Build(_context)
            .ToListAsync();

        result[0].Tag.Id.Should().Be(busy.Id);
        result[1].Tag.Id.Should().Be(quiet.Id);
    }

    [Fact]
    public async Task Build_WithUsageOrderAndContentType_ShouldRankByThatKindAlone()
    {
        TagEntity videoHeavy = await SeedTagAsync("Alpha");
        TagEntity articleHeavy = await SeedTagAsync("Zebra");
        await SeedVideoTagAsync(videoHeavy.Id);
        await SeedVideoTagAsync(videoHeavy.Id);
        await SeedVideoTagAsync(articleHeavy.Id);
        await SeedArticleTagAsync(articleHeavy.Id);
        await SeedArticleTagAsync(articleHeavy.Id);
        await SeedArticleTagAsync(articleHeavy.Id);

        List<TagCountRow> result = await new TagsQueryBuilder()
            .WithContentType(EnumCoreContentType.Video)
            .WithOrder(EnumTagOrder.Usage)
            .Build(_context)
            .ToListAsync();

        result[0].Tag.Id.Should().Be(videoHeavy.Id);
    }

    #endregion

    #region Counts

    [Fact]
    public async Task Build_ShouldCountEachKindSeparately()
    {
        TagEntity tag = await SeedTagAsync("alpha");
        await SeedArticleTagAsync(tag.Id);
        await SeedArticleTagAsync(tag.Id);
        await SeedVideoTagAsync(tag.Id);
        await SeedLyricsTagAsync(tag.Id);

        List<TagCountRow> result = await new TagsQueryBuilder().Build(_context).ToListAsync();

        result.Should().ContainSingle();
        result[0].Articles.Should().Be(2);
        result[0].Videos.Should().Be(1);
        result[0].Lyrics.Should().Be(1);
    }

    [Fact]
    public async Task Build_WithPublishedOnly_ShouldNotCountAssociationsWithoutPublishedContent()
    {
        TagEntity tag = await SeedTagAsync("alpha");
        await SeedArticleTagAsync(tag.Id);
        await SeedVideoTagAsync(tag.Id);

        List<TagCountRow> result = await new TagsQueryBuilder().WithPublishedOnly(true).Build(_context).ToListAsync();

        result.Should().ContainSingle();
        result[0].Articles.Should().Be(0);
        result[0].Videos.Should().Be(0);
        result[0].Lyrics.Should().Be(0);
    }

    [Fact]
    public async Task Build_WithUnusedTag_ShouldCountZeroWithoutDroppingIt()
    {
        await SeedTagAsync("alpha");

        List<TagCountRow> result = await new TagsQueryBuilder().Build(_context).ToListAsync();

        result.Should().ContainSingle();
        result[0].Articles.Should().Be(0);
        result[0].Videos.Should().Be(0);
        result[0].Lyrics.Should().Be(0);
    }

    #endregion

    #region Limit

    [Fact]
    public async Task Build_WithLimit_ShouldReturnAtMostThatMany()
    {
        await SeedTagAsync("alpha");
        await SeedTagAsync("beta");
        await SeedTagAsync("gamma");

        List<TagCountRow> result = await new TagsQueryBuilder().WithLimit(2).Build(_context).ToListAsync();

        result.Should().HaveCount(2);
    }

    #endregion
}
