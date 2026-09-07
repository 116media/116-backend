using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="TagRepository"/> using InMemory database.
/// </summary>
public class TagRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly TagRepository _repository;

    public TagRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new TagRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistEntity()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();

        // Act
        await _repository.AddAsync(tag);
        await _context.SaveChangesAsync();

        // Assert
        TagEntity? retrieved = await _context.Tags.FindAsync(tag.Id);
        retrieved.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_WhenFound_ShouldReturnEntity()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        // Act
        TagEntity? result = await _repository.GetBySlugAsync(tag.Slug);

        // Assert
        result.Should().NotBeNull();
        result.Slug.Should().Be(tag.Slug);
    }

    [Fact]
    public async Task GetBySlugAsync_WhenNotFound_ShouldReturnNull()
    {
        // Act
        TagEntity? result = await _repository.GetBySlugAsync("non-existent-slug");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithNoSearch_ShouldReturnAllTagsOrderedByName()
    {
        // Arrange
        _context.Tags.Add(TagFactory.Create("Kinshasa", "kinshasa"));
        _context.Tags.Add(TagFactory.Create("Fally Ipupa", "fally-ipupa"));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<TagEntity> result = await _repository.GetAllAsync(search: null);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Fally Ipupa");
        result[1].Name.Should().Be("Kinshasa");
    }

    [Fact]
    public async Task GetAllAsync_WithArticleContentType_ShouldReturnOnlyArticleAssociatedTags()
    {
        // Arrange
        TagEntity articleTag = TagFactory.Create("Alpha", "alpha");
        TagEntity videoTag = TagFactory.Create("Beta", "beta");
        TagEntity unusedTag = TagFactory.Create("Gamma", "gamma");
        _context.Tags.AddRange(articleTag, videoTag, unusedTag);
        _context.ArticleTags.Add(ArticleTagEntity.Create(Guid.NewGuid(), Guid.NewGuid(), articleTag.Id));
        _context.VideoTags.Add(VideoTagEntity.Create(Guid.NewGuid(), Guid.NewGuid(), videoTag.Id));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<TagEntity> result = await _repository.GetAllAsync(
            search: null,
            contentType: EnumCoreContentType.Article
        );

        // Assert
        result.Should().ContainSingle();
        result[0].Id.Should().Be(articleTag.Id);
    }

    [Fact]
    public async Task GetAllAsync_WithVideoContentType_ShouldReturnOnlyVideoAssociatedTags()
    {
        // Arrange
        TagEntity articleTag = TagFactory.Create("Alpha", "alpha");
        TagEntity videoTag = TagFactory.Create("Beta", "beta");
        TagEntity unusedTag = TagFactory.Create("Gamma", "gamma");
        _context.Tags.AddRange(articleTag, videoTag, unusedTag);
        _context.ArticleTags.Add(ArticleTagEntity.Create(Guid.NewGuid(), Guid.NewGuid(), articleTag.Id));
        _context.VideoTags.Add(VideoTagEntity.Create(Guid.NewGuid(), Guid.NewGuid(), videoTag.Id));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<TagEntity> result = await _repository.GetAllAsync(
            search: null,
            contentType: EnumCoreContentType.Video
        );

        // Assert
        result.Should().ContainSingle();
        result[0].Id.Should().Be(videoTag.Id);
    }

    [Fact]
    public async Task GetAllAsync_WithNullContentType_ShouldReturnAllTagsOrderedByName()
    {
        // Arrange
        TagEntity articleTag = TagFactory.Create("Kinshasa", "kinshasa");
        TagEntity unusedTag = TagFactory.Create("Afrobeats", "afrobeats");
        _context.Tags.AddRange(articleTag, unusedTag);
        _context.ArticleTags.Add(ArticleTagEntity.Create(Guid.NewGuid(), Guid.NewGuid(), articleTag.Id));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<TagEntity> result = await _repository.GetAllAsync(search: null, contentType: null);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Afrobeats");
        result[1].Name.Should().Be("Kinshasa");
    }

    [Fact]
    public async Task GetAllAsync_WithArticleContentType_ShouldOrderResultsByName()
    {
        // Arrange
        TagEntity zebra = TagFactory.Create("Zebra", "zebra");
        TagEntity alpha = TagFactory.Create("Alpha", "alpha");
        _context.Tags.AddRange(zebra, alpha);
        _context.ArticleTags.Add(ArticleTagEntity.Create(Guid.NewGuid(), Guid.NewGuid(), zebra.Id));
        _context.ArticleTags.Add(ArticleTagEntity.Create(Guid.NewGuid(), Guid.NewGuid(), alpha.Id));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<TagEntity> result = await _repository.GetAllAsync(
            search: null,
            contentType: EnumCoreContentType.Article
        );

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(alpha.Id);
        result[1].Id.Should().Be(zebra.Id);
    }

    [Fact]
    public async Task GetAllAsync_WithLimit_ShouldCapResultCountAndPreserveNameOrder()
    {
        // Arrange
        _context.Tags.Add(TagFactory.Create("Delta", "delta"));
        _context.Tags.Add(TagFactory.Create("Alpha", "alpha"));
        _context.Tags.Add(TagFactory.Create("Charlie", "charlie"));
        _context.Tags.Add(TagFactory.Create("Bravo", "bravo"));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<TagEntity> result = await _repository.GetAllAsync(search: null, contentType: null, limit: 2);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Alpha");
        result[1].Name.Should().Be("Bravo");
    }

    [Fact]
    public async Task Remove_ShouldDeleteTagFromDatabase()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        // Act
        _repository.Remove(tag);
        await _context.SaveChangesAsync();

        // Assert
        TagEntity? retrieved = await _context.Tags.FindAsync(tag.Id);
        retrieved.Should().BeNull();
    }
}
