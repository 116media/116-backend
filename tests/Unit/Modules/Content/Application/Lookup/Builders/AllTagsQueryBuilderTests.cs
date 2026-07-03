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
/// Unit tests for <see cref="AllTagsQueryBuilder"/>.
/// Uses an InMemory <see cref="ContentDbContext"/> so that the association filter
/// (<c>Where(... Any ...)</c>) and ordering can be executed without a real database.
/// The ILike-based search filter is only supported by PostgreSQL, so search behaviour
/// is covered by the integration repository tests rather than here.
/// </summary>
public class AllTagsQueryBuilderTests : IDisposable
{
    private readonly ContentDbContext _context;

    public AllTagsQueryBuilderTests()
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

    #region WithContentType null (all tags)

    [Fact]
    public async Task Build_WithNoContentType_ShouldReturnAllTagsOrderedByName()
    {
        await SeedTagAsync("Kinshasa");
        await SeedTagAsync("Afrobeats");

        List<TagEntity> result = await new AllTagsQueryBuilder().Build(_context).ToListAsync();

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Afrobeats");
        result[1].Name.Should().Be("Kinshasa");
    }

    [Fact]
    public async Task Build_WithNullContentType_ShouldNotFilterByAssociation()
    {
        TagEntity articleTag = await SeedTagAsync("article-tag");
        TagEntity unusedTag = await SeedTagAsync("unused-tag");
        await SeedArticleTagAsync(articleTag.Id);

        List<TagEntity> result = await new AllTagsQueryBuilder().WithContentType(null).Build(_context).ToListAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Id == articleTag.Id);
        result.Should().Contain(t => t.Id == unusedTag.Id);
    }

    #endregion

    #region WithContentType Article

    [Fact]
    public async Task Build_WithArticleContentType_ShouldReturnOnlyArticleAssociatedTags()
    {
        TagEntity articleTag = await SeedTagAsync("article-tag");
        TagEntity videoTag = await SeedTagAsync("video-tag");
        TagEntity unusedTag = await SeedTagAsync("unused-tag");
        await SeedArticleTagAsync(articleTag.Id);
        await SeedVideoTagAsync(videoTag.Id);

        List<TagEntity> result = await new AllTagsQueryBuilder()
            .WithContentType(EnumCoreContentType.Article)
            .Build(_context)
            .ToListAsync();

        result.Should().ContainSingle();
        result[0].Id.Should().Be(articleTag.Id);
    }

    [Fact]
    public async Task Build_WithArticleContentType_ShouldOrderResultsByName()
    {
        TagEntity zebra = await SeedTagAsync("Zebra");
        TagEntity alpha = await SeedTagAsync("Alpha");
        await SeedArticleTagAsync(zebra.Id);
        await SeedArticleTagAsync(alpha.Id);

        List<TagEntity> result = await new AllTagsQueryBuilder()
            .WithContentType(EnumCoreContentType.Article)
            .Build(_context)
            .ToListAsync();

        result[0].Id.Should().Be(alpha.Id);
        result[1].Id.Should().Be(zebra.Id);
    }

    #endregion

    #region WithContentType Video

    [Fact]
    public async Task Build_WithVideoContentType_ShouldReturnOnlyVideoAssociatedTags()
    {
        TagEntity videoTag = await SeedTagAsync("video-tag");
        TagEntity articleTag = await SeedTagAsync("article-tag");
        TagEntity unusedTag = await SeedTagAsync("unused-tag");
        await SeedVideoTagAsync(videoTag.Id);
        await SeedArticleTagAsync(articleTag.Id);

        List<TagEntity> result = await new AllTagsQueryBuilder()
            .WithContentType(EnumCoreContentType.Video)
            .Build(_context)
            .ToListAsync();

        result.Should().ContainSingle();
        result[0].Id.Should().Be(videoTag.Id);
    }

    #endregion

    #region WithLimit

    [Fact]
    public async Task Build_WithLimit_ShouldReturnAtMostLimitTagsOrderedByName()
    {
        await SeedTagAsync("Delta");
        await SeedTagAsync("Alpha");
        await SeedTagAsync("Charlie");
        await SeedTagAsync("Bravo");

        List<TagEntity> result = await new AllTagsQueryBuilder().WithLimit(2).Build(_context).ToListAsync();

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Alpha");
        result[1].Name.Should().Be("Bravo");
    }

    [Fact]
    public async Task Build_WithNullLimit_ShouldReturnAllTags()
    {
        await SeedTagAsync("Kinshasa");
        await SeedTagAsync("Afrobeats");
        await SeedTagAsync("Rumba");

        List<TagEntity> result = await new AllTagsQueryBuilder().WithLimit(null).Build(_context).ToListAsync();

        result.Should().HaveCount(3);
    }

    #endregion
}
