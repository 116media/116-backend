using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="ArticleRepository"/>.
/// </summary>
public class ArticleRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly ArticleRepository _repository;

    public ArticleRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(new CreatedAtStampingInterceptor())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new ArticleRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Seeds a ContentType and Category, returning the Category ID.
    /// </summary>
    private async Task<Guid> SeedCategoryAsync()
    {
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        _context.ContentTypes.Add(contentType);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category.Id;
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithNoFilters_ShouldReturnAllArticles()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        _context.Articles.AddRange(ArticleFactory.CreateMany(categoryId, 3));
        await _context.SaveChangesAsync();

        // Act
        var (articles, totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            search: null,
            status: null,
            categoryId: null
        );

        // Assert
        articles.Should().HaveCount(3);
        totalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        _context.Articles.AddRange(ArticleFactory.CreateMany(categoryId, 5));
        await _context.SaveChangesAsync();

        // Act
        var (articles, totalCount) = await _repository.GetAllAsync(
            page: 2,
            pageSize: 2,
            search: null,
            status: null,
            categoryId: null
        );

        // Assert
        articles.Should().HaveCount(2);
        totalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetAllAsync_WithStatusFilter_ShouldFilterByStatus()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        _context.Articles.Add(ArticleFactory.Create(categoryId)); // Draft
        _context.Articles.Add(ArticleFactory.CreatePublished(categoryId)); // Published
        await _context.SaveChangesAsync();

        // Act
        var (articles, totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            search: null,
            status: EnumContentStatus.Draft,
            categoryId: null
        );

        // Assert
        articles.Should().ContainSingle();
        totalCount.Should().Be(1);
        articles.First().Status.Should().Be(EnumContentStatus.Draft);
    }

    [Fact]
    public async Task GetAllAsync_WithCategoryFilter_ShouldFilterByCategory()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        Guid otherCategoryId = await SeedCategoryAsync();

        _context.Articles.Add(ArticleFactory.Create(categoryId));
        _context.Articles.Add(ArticleFactory.Create(otherCategoryId));
        await _context.SaveChangesAsync();

        // Act
        var (articles, totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            search: null,
            status: null,
            categoryId: categoryId
        );

        // Assert
        articles.Should().ContainSingle();
        totalCount.Should().Be(1);
        articles.First().CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Act
        var (articles, totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            search: null,
            status: null,
            categoryId: null
        );

        // Assert
        articles.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    // Note: GetAllAsync with search uses PostgreSQL ILike which is not supported by InMemoryDatabase.
    // Search filtering is covered in integration tests.

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenArticleExists_ShouldReturnArticle()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        // Act
        ArticleEntity? result = await _repository.GetByIdAsync(article.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(article.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenArticleDoesNotExist_ShouldReturnNull()
    {
        // Act
        ArticleEntity? result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdOrThrowAsync Tests

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenArticleExists_ShouldReturnArticle()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        // Act
        ArticleEntity result = await _repository.GetByIdOrThrowAsync(article.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(article.Id);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenArticleDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.GetByIdOrThrowAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetBySlugAsync Tests

    // Note: GetBySlugAsync uses PostgreSQL ILike which is not supported by InMemoryDatabase.
    // This method is covered in integration tests.

    #endregion

    #region GetPromotedAsync Tests

    [Fact]
    public async Task GetPromotedAsync_ShouldReturnPromotedPublishedArticles()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity promoted = ArticleFactory.CreatePromoted(categoryId);
        ArticleEntity draft = ArticleFactory.Create(categoryId);
        _context.Articles.AddRange(promoted, draft);
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<ArticleEntity> result = await _repository.GetPromotedAsync();

        // Assert
        result.Should().ContainSingle();
        result.First().IsPromoted.Should().BeTrue();
        result.First().Status.Should().Be(EnumContentStatus.Published);
    }

    [Fact]
    public async Task GetPromotedAsync_WhenNoPromotedArticles_ShouldReturnEmptyList()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        _context.Articles.Add(ArticleFactory.Create(categoryId));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<ArticleEntity> result = await _repository.GetPromotedAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetAbandonedDraftsAsync Tests

    [Fact]
    public async Task GetAbandonedDraftsAsync_ShouldReturnDraftsOlderThanCutoff()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity abandonedDraft = ArticleFactory.Create(categoryId);
        abandonedDraft.CreatedAt = DateTime.UtcNow.AddDays(-10); // older than 7-day cutoff

        ArticleEntity recentDraft = ArticleFactory.Create(categoryId);
        recentDraft.CreatedAt = DateTime.UtcNow.AddDays(-1); // recent

        _context.Articles.AddRange(abandonedDraft, recentDraft);
        await _context.SaveChangesAsync();

        DateTime cutoff = DateTime.UtcNow.AddDays(-7);

        // Act
        IReadOnlyList<ArticleEntity> result = await _repository.GetAbandonedDraftsAsync(cutoff);

        // Assert
        result.Should().ContainSingle();
        result.First().Id.Should().Be(abandonedDraft.Id);
    }

    [Fact]
    public async Task GetAbandonedDraftsAsync_WhenNoneMatchCriteria_ShouldReturnEmptyList()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity published = ArticleFactory.CreatePublished(categoryId);
        published.CreatedAt = DateTime.UtcNow.AddDays(-10);
        _context.Articles.Add(published);
        await _context.SaveChangesAsync();

        DateTime cutoff = DateTime.UtcNow.AddDays(-7);

        // Act
        IReadOnlyList<ArticleEntity> result = await _repository.GetAbandonedDraftsAsync(cutoff);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddArticleToContext()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);

        // Act
        await _repository.AddAsync(article);
        await _context.SaveChangesAsync();

        // Assert
        ArticleEntity? saved = await _context.Articles.FirstOrDefaultAsync(a => a.Id == article.Id);
        saved.Should().NotBeNull();
        saved.Title.Should().Be(article.Title);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ShouldMarkArticleAsModified()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        // Act
        _repository.Update(article);

        // Assert
        _context.Entry(article).State.Should().Be(EntityState.Modified);
    }

    #endregion

    #region Remove Tests

    [Fact]
    public async Task Remove_ShouldRemoveArticleFromContext()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        // Act
        _repository.Remove(article);
        await _context.SaveChangesAsync();

        // Assert
        ArticleEntity? deleted = await _context.Articles.FirstOrDefaultAsync(a => a.Id == article.Id);
        deleted.Should().BeNull();
    }

    #endregion

    #region Image Tests

    [Fact]
    public async Task AddImage_ThroughTheRoot_ShouldPersistTheRow()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        // Act
        ArticleImageEntity image = ArticleImageFactory.CreateCover(article);
        await _context.SaveChangesAsync();

        // Assert
        ArticleImageEntity? saved = await _context.ArticleImages.FirstOrDefaultAsync(i => i.Id == image.Id);
        saved.Should().NotBeNull();
        saved!.ArticleId.Should().Be(article.Id);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_ShouldHydrateTheImages()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        ArticleImageFactory.CreateCover(article);
        ArticleImageFactory.CreateBody(article);
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        ArticleEntity loaded = await _repository.GetByIdOrThrowAsync(article.Id);

        // Assert
        loaded.Images.Should().HaveCount(2);
    }

    [Fact]
    public async Task RemoveCoverImage_ThroughTheRoot_ShouldDeleteOnlyTheCoverRow()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        ArticleImageFactory.CreateCover(article);
        ArticleImageFactory.CreateBody(article);
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        // Act
        ArticleImageEntity? removed = article.RemoveCoverImage();
        await _context.SaveChangesAsync();

        // Assert
        removed.Should().NotBeNull();
        (await _context.ArticleImages.CountAsync(i => i.ArticleId == article.Id)).Should().Be(1);
    }

    [Fact]
    public async Task RemoveBodyImages_ThroughTheRoot_ShouldDeleteOnlyTheMatchingRows()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        ArticleImageEntity keep = ArticleImageFactory.CreateBody(article, "keep-key", "https://cdn.example/keep.jpg");
        ArticleImageEntity drop = ArticleImageFactory.CreateBody(article, "drop-key", "https://cdn.example/drop.jpg");
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<ArticleImageEntity> removed = article.RemoveBodyImages(["drop-key"]);
        await _context.SaveChangesAsync();

        // Assert
        removed.Should().ContainSingle().Which.Id.Should().Be(drop.Id);
        (await _context.ArticleImages.SingleAsync(i => i.ArticleId == article.Id)).Id.Should().Be(keep.Id);
    }

    #endregion

    #region Tag Tests

    [Fact]
    public async Task ReplaceTags_ThroughTheRoot_ShouldPersistTheRows()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        var tagId = Guid.NewGuid();

        // Act
        article.ReplaceTags([tagId]);
        await _context.SaveChangesAsync();

        // Assert
        (await _context.ArticleTags.CountAsync(t => t.ArticleId == article.Id))
            .Should()
            .Be(1);
    }

    [Fact]
    public async Task ReplaceTags_ThroughTheRoot_ShouldDropRowsThatLeftTheSet()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        TagEntity keptTag = TagFactory.Create("kept-article-tag", "kept-article-tag");
        TagEntity removedTag = TagFactory.Create("removed-article-tag", "removed-article-tag");
        _context.Tags.AddRange(keptTag, removedTag);
        Guid keptTagId = keptTag.Id;
        article.ReplaceTags([keptTagId, removedTag.Id]);
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        // Act
        article.ReplaceTags([keptTagId]);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        ArticleEntity loaded = await _repository.GetByIdOrThrowAsync(article.Id);
        loaded.Tags.Should().ContainSingle().Which.TagId.Should().Be(keptTagId);
    }

    #endregion
}
