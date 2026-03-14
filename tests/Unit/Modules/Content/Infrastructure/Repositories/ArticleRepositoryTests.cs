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

    /// <summary>Seeds a ContentType and Category, returning the Category ID.</summary>
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

    #region GetFeaturedAsync Tests

    [Fact]
    public async Task GetFeaturedAsync_ShouldReturnFeaturedPublishedArticles()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity featured = ArticleFactory.CreateFeatured(categoryId);
        ArticleEntity draft = ArticleFactory.Create(categoryId);
        _context.Articles.AddRange(featured, draft);
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<ArticleEntity> result = await _repository.GetFeaturedAsync();

        // Assert
        result.Should().ContainSingle();
        result.First().IsFeatured.Should().BeTrue();
        result.First().Status.Should().Be(EnumContentStatus.Published);
    }

    [Fact]
    public async Task GetFeaturedAsync_WhenNoFeaturedArticles_ShouldReturnEmptyList()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        _context.Articles.Add(ArticleFactory.Create(categoryId));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<ArticleEntity> result = await _repository.GetFeaturedAsync();

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
        // Arrange — published article should not be returned
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

    #region AddImageAsync / GetImagesByArticleIdAsync / RemoveImages Tests

    [Fact]
    public async Task AddImageAsync_ShouldAddImageToContext()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        ArticleImageEntity image = ArticleImageFactory.CreateCover(article.Id);

        // Act
        await _repository.AddImageAsync(image);
        await _context.SaveChangesAsync();

        // Assert
        ArticleImageEntity? saved = await _context.ArticleImages.FirstOrDefaultAsync(i => i.Id == image.Id);
        saved.Should().NotBeNull();
        saved.ArticleId.Should().Be(article.Id);
    }

    [Fact]
    public async Task GetImagesByArticleIdAsync_ShouldReturnImagesForArticle()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        ArticleImageEntity image1 = ArticleImageFactory.CreateCover(article.Id);
        ArticleImageEntity image2 = ArticleImageFactory.CreateBody(article.Id);
        _context.ArticleImages.AddRange(image1, image2);
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<ArticleImageEntity> result = await _repository.GetImagesByArticleIdAsync(article.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(i => i.ArticleId.Should().Be(article.Id));
    }

    [Fact]
    public async Task RemoveImages_ShouldRemoveImagesFromContext()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        ArticleImageEntity image = ArticleImageFactory.CreateCover(article.Id);
        _context.ArticleImages.Add(image);
        await _context.SaveChangesAsync();

        // Act
        _repository.RemoveImages([image]);
        await _context.SaveChangesAsync();

        // Assert
        ArticleImageEntity? deleted = await _context.ArticleImages.FirstOrDefaultAsync(i => i.Id == image.Id);
        deleted.Should().BeNull();
    }

    #endregion

    #region AddTagAsync / GetTagsByArticleIdAsync / RemoveTag Tests

    [Fact]
    public async Task AddTagAsync_ShouldAddTagJunctionToContext()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);

        TagEntity tag = TagFactory.CreateDefault();
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        var articleTag = new ArticleTagEntity { ArticleId = article.Id, TagId = tag.Id };

        // Act
        await _repository.AddTagAsync(articleTag);
        await _context.SaveChangesAsync();

        // Assert
        IReadOnlyList<ArticleTagEntity> result = await _repository.GetTagsByArticleIdAsync(article.Id);
        result.Should().ContainSingle();
        result.First().TagId.Should().Be(tag.Id);
    }

    [Fact]
    public async Task GetTagsByArticleIdAsync_WhenNoTags_ShouldReturnEmptyList()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<ArticleTagEntity> result = await _repository.GetTagsByArticleIdAsync(article.Id);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveTag_ShouldRemoveTagJunctionFromContext()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);

        TagEntity tag = TagFactory.CreateDefault();
        _context.Tags.Add(tag);

        var articleTag = new ArticleTagEntity { ArticleId = article.Id, TagId = tag.Id };
        _context.ArticleTags.Add(articleTag);
        await _context.SaveChangesAsync();

        // Act
        _repository.RemoveTag(articleTag);
        await _context.SaveChangesAsync();

        // Assert
        IReadOnlyList<ArticleTagEntity> result = await _repository.GetTagsByArticleIdAsync(article.Id);
        result.Should().BeEmpty();
    }

    #endregion
}
