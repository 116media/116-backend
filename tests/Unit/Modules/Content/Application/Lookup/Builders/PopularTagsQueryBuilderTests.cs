using _116.Content.Application.Lookup.Builders;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.Builders;

/// <summary>
/// Unit tests for <see cref="PopularTagsQueryBuilder"/>.
/// Uses an InMemory <see cref="ContentDbContext"/> so that the JOIN/GROUP-BY
/// queries can be executed without a real database, while still exercising
/// the full LINQ pipeline produced by the builder.
/// </summary>
public class PopularTagsQueryBuilderTests : IDisposable
{
    private readonly ContentDbContext _context;

    public PopularTagsQueryBuilderTests()
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

    private async Task<TagEntity> SeedTagAsync(string name = "tag")
    {
        TagEntity tag = TagFactory.Create(name, name.ToLowerInvariant().Replace(" ", "-"));
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return tag;
    }

    private async Task SeedArticleTagAsync(Guid tagId)
    {
        ArticleTagEntity at = ArticleTagEntity.Create(Guid.NewGuid(), Guid.NewGuid(), tagId);
        _context.ArticleTags.Add(at);
        await _context.SaveChangesAsync();
    }

    private async Task SeedVideoTagAsync(Guid tagId)
    {
        VideoTagEntity vt = VideoTagEntity.Create(Guid.NewGuid(), Guid.NewGuid(), tagId);
        _context.VideoTags.Add(vt);
        await _context.SaveChangesAsync();
    }

    #region WithContentType null (combined)

    [Fact]
    public async Task Build_WithNoContentType_ShouldReturnAllTags()
    {
        TagEntity articleTag = await SeedTagAsync("article-tag");
        TagEntity videoTag = await SeedTagAsync("video-tag");
        await SeedArticleTagAsync(articleTag.Id);
        await SeedVideoTagAsync(videoTag.Id);

        List<TagEntity> result = await new PopularTagsQueryBuilder().Build(_context).ToListAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Id == articleTag.Id);
        result.Should().Contain(t => t.Id == videoTag.Id);
    }

    [Fact]
    public async Task Build_WithNoContentType_ShouldOrderByTotalUsageDescending()
    {
        // tagA: 1 article + 1 video = 2 usages; tagB: 1 article = 1 usage
        TagEntity tagA = await SeedTagAsync("tag-a");
        TagEntity tagB = await SeedTagAsync("tag-b");
        await SeedArticleTagAsync(tagA.Id);
        await SeedVideoTagAsync(tagA.Id);
        await SeedArticleTagAsync(tagB.Id);

        List<TagEntity> result = await new PopularTagsQueryBuilder().Build(_context).ToListAsync();

        result[0].Id.Should().Be(tagA.Id);
        result[1].Id.Should().Be(tagB.Id);
    }

    [Fact]
    public async Task Build_WithNoContentType_WhenNoUsages_ShouldStillReturnTags()
    {
        TagEntity tag = await SeedTagAsync("unused-tag");

        List<TagEntity> result = await new PopularTagsQueryBuilder().Build(_context).ToListAsync();

        result.Should().ContainSingle();
        result[0].Id.Should().Be(tag.Id);
    }

    #endregion

    #region WithContentType Article

    [Fact]
    public async Task Build_WithArticleContentType_ShouldReturnTagsOrderedByArticleUsage()
    {
        // tagA: 2 article uses; tagB: 1 article use
        TagEntity tagA = await SeedTagAsync("alpha");
        TagEntity tagB = await SeedTagAsync("beta");
        await SeedArticleTagAsync(tagA.Id);
        await SeedArticleTagAsync(tagA.Id);
        await SeedArticleTagAsync(tagB.Id);

        List<TagEntity> result = await new PopularTagsQueryBuilder()
            .WithContentType(EnumCoreContentType.Article)
            .Build(_context)
            .ToListAsync();

        result[0].Id.Should().Be(tagA.Id);
        result[1].Id.Should().Be(tagB.Id);
    }

    [Fact]
    public async Task Build_WithArticleContentType_ShouldNotFilterOutTagsWithZeroArticleUsage()
    {
        TagEntity articleTag = await SeedTagAsync("article-tag");
        TagEntity videoOnlyTag = await SeedTagAsync("video-only-tag");
        await SeedArticleTagAsync(articleTag.Id);
        await SeedVideoTagAsync(videoOnlyTag.Id);

        List<TagEntity> result = await new PopularTagsQueryBuilder()
            .WithContentType(EnumCoreContentType.Article)
            .Build(_context)
            .ToListAsync();

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(articleTag.Id);
        result[1].Id.Should().Be(videoOnlyTag.Id);
    }

    #endregion

    #region WithContentType Video

    [Fact]
    public async Task Build_WithVideoContentType_ShouldReturnTagsOrderedByVideoUsage()
    {
        // tagA: 2 video uses; tagB: 1 video use
        TagEntity tagA = await SeedTagAsync("alpha");
        TagEntity tagB = await SeedTagAsync("beta");
        await SeedVideoTagAsync(tagA.Id);
        await SeedVideoTagAsync(tagA.Id);
        await SeedVideoTagAsync(tagB.Id);

        List<TagEntity> result = await new PopularTagsQueryBuilder()
            .WithContentType(EnumCoreContentType.Video)
            .Build(_context)
            .ToListAsync();

        result[0].Id.Should().Be(tagA.Id);
        result[1].Id.Should().Be(tagB.Id);
    }

    [Fact]
    public async Task Build_WithVideoContentType_ShouldNotFilterOutTagsWithZeroVideoUsage()
    {
        TagEntity videoTag = await SeedTagAsync("video-tag");
        TagEntity articleOnlyTag = await SeedTagAsync("article-only-tag");
        await SeedVideoTagAsync(videoTag.Id);
        await SeedArticleTagAsync(articleOnlyTag.Id);

        List<TagEntity> result = await new PopularTagsQueryBuilder()
            .WithContentType(EnumCoreContentType.Video)
            .Build(_context)
            .ToListAsync();

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(videoTag.Id);
        result[1].Id.Should().Be(articleOnlyTag.Id);
    }

    #endregion

    #region WithLimit

    [Fact]
    public async Task Build_WithLimit_ShouldReturnOnlyThatManyTags()
    {
        for (int i = 1; i <= 5; i++)
        {
            await SeedTagAsync($"tag-{i}");
        }

        List<TagEntity> result = await new PopularTagsQueryBuilder().WithLimit(3).Build(_context).ToListAsync();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Build_WithNullLimit_ShouldReturnAllTags()
    {
        for (int i = 1; i <= 5; i++)
        {
            await SeedTagAsync($"tag-{i}");
        }

        List<TagEntity> result = await new PopularTagsQueryBuilder().WithLimit(null).Build(_context).ToListAsync();

        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task Build_WithLimitLargerThanAvailable_ShouldReturnAllTags()
    {
        for (int i = 1; i <= 3; i++)
        {
            await SeedTagAsync($"tag-{i}");
        }

        List<TagEntity> result = await new PopularTagsQueryBuilder().WithLimit(10).Build(_context).ToListAsync();

        result.Should().HaveCount(3);
    }

    #endregion

    #region Chaining

    [Fact]
    public async Task Build_WithArticleContentTypeAndLimit_ShouldApplyBoth()
    {
        for (int i = 1; i <= 4; i++)
        {
            TagEntity t = await SeedTagAsync($"tag-{i}");
            await SeedArticleTagAsync(t.Id);
        }

        List<TagEntity> result = await new PopularTagsQueryBuilder()
            .WithContentType(EnumCoreContentType.Article)
            .WithLimit(2)
            .Build(_context)
            .ToListAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Build_WithVideoContentTypeAndLimit_ShouldApplyBoth()
    {
        for (int i = 1; i <= 4; i++)
        {
            TagEntity t = await SeedTagAsync($"tag-{i}");
            await SeedVideoTagAsync(t.Id);
        }

        List<TagEntity> result = await new PopularTagsQueryBuilder()
            .WithContentType(EnumCoreContentType.Video)
            .WithLimit(2)
            .Build(_context)
            .ToListAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Build_WithNullContentTypeAndLimit_ShouldApplyBoth()
    {
        for (int i = 1; i <= 5; i++)
        {
            await SeedTagAsync($"tag-{i}");
        }

        List<TagEntity> result = await new PopularTagsQueryBuilder()
            .WithContentType(null)
            .WithLimit(3)
            .Build(_context)
            .ToListAsync();

        result.Should().HaveCount(3);
    }

    #endregion
}
