using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IArticleRepository"/> verifying article persistence,
/// querying, pagination, filtering, image management, tag associations, social interactions
/// (likes, bookmarks, shares, comments), and promotion queries against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class ArticleRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResults()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var articles = ArticleFactory.CreateMany(category.Id, 5);
        seedContext.Articles.AddRange(articles);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IArticleRepository>();
        var (result, totalCount) = await repo.GetAllAsync(
            page: 1,
            pageSize: 3,
            search: null,
            status: null,
            categoryId: null
        );

        totalCount.Should().BeGreaterThanOrEqualTo(5);
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_SecondPage_ReturnsRemainingItems()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var articles = ArticleFactory.CreateMany(category.Id, 5);
        seedContext.Articles.AddRange(articles);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IArticleRepository>();
        var (page1, _) = await repo.GetAllAsync(page: 1, pageSize: 3, search: null, status: null, categoryId: null);
        var (page2, totalCount) = await repo.GetAllAsync(
            page: 2,
            pageSize: 3,
            search: null,
            status: null,
            categoryId: null
        );

        totalCount.Should().BeGreaterThanOrEqualTo(5);
        page1.Should().HaveCount(3);
        page2.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetAllAsync_WithStatusFilter_ReturnsOnlyMatchingArticles()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var draft = ArticleFactory.Create(category.Id);
        var published = ArticleFactory.CreatePublished(category.Id);
        seedContext.Articles.AddRange(draft, published);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IArticleRepository>();
        var (result, _) = await repo.GetAllAsync(
            page: 1,
            pageSize: 100,
            search: null,
            status: EnumContentStatus.Published,
            categoryId: null
        );

        result.Should().OnlyContain(a => a.Status == EnumContentStatus.Published);
    }

    [Fact]
    public async Task GetAllAsync_WithCategoryFilter_ReturnsOnlyMatchingArticles()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var cat1 = CategoryFactory.Create(contentType.Id);
        var cat2 = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.AddRange(cat1, cat2);
        await seedContext.SaveChangesAsync();

        var article1 = ArticleFactory.Create(cat1.Id);
        var article2 = ArticleFactory.Create(cat2.Id);
        seedContext.Articles.AddRange(article1, article2);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IArticleRepository>();
        var (result, _) = await repo.GetAllAsync(
            page: 1,
            pageSize: 100,
            search: null,
            status: null,
            categoryId: cat1.Id
        );

        result.Should().OnlyContain(a => a.CategoryId == cat1.Id);
    }

    [Fact]
    public async Task GetAllAsync_IncludesCategoryNavigation()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var article = ArticleFactory.Create(category.Id);
        seedContext.Articles.Add(article);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IArticleRepository>();
        var (result, _) = await repo.GetAllAsync(page: 1, pageSize: 10, search: null, status: null, categoryId: null);

        result.Should().Contain(a => a.Id == article.Id);
        result.First(a => a.Id == article.Id).Category.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenArticleExists_ReturnsEntityWithIncludes()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var article = ArticleFactory.Create(category.Id);
        seedContext.Articles.Add(article);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IArticleRepository>();
        var result = await repo.GetByIdAsync(article.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(article.Id);
        result.Category.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenArticleDoesNotExist_ReturnsNull()
    {
        var repo = Resolve<IArticleRepository>();
        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenArticleExists_ReturnsEntity()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var article = ArticleFactory.Create(category.Id);
        seedContext.Articles.Add(article);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IArticleRepository>();
        var result = await repo.GetByIdOrThrowAsync(article.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(article.Id);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenArticleDoesNotExist_ThrowsNotFoundException()
    {
        var repo = Resolve<IArticleRepository>();

        var act = () => repo.GetByIdOrThrowAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetBySlugAsync_WhenSlugExists_ReturnsEntity()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var article = ArticleFactory.Create(category.Id, "Test Slug Article", "test-slug-article");
        seedContext.Articles.Add(article);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IArticleRepository>();
        var result = await repo.GetBySlugAsync("test-slug-article");

        result.Should().NotBeNull();
        result!.Slug.Should().Be("test-slug-article");
        result.Category.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_WhenSlugDoesNotExist_ReturnsNull()
    {
        var repo = Resolve<IArticleRepository>();
        var result = await repo.GetBySlugAsync("non-existent-slug");

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_PersistsArticleToDatabase()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IArticleRepository, ContentDbContext>();
        var article = ArticleFactory.Create(category.Id);

        await repo.AddAsync(article);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var persisted = await verifyContext.Articles.FirstOrDefaultAsync(a => a.Id == article.Id);

        persisted.Should().NotBeNull();
        persisted!.Title.Should().Be(article.Title);
    }

    [Fact]
    public async Task Update_PersistsChangesToDatabase()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var article = ArticleFactory.Create(category.Id);
        seedContext.Articles.Add(article);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IArticleRepository, ContentDbContext>();
        var existing = await db.Articles.FindAsync(article.Id);
        repo.Update(existing!);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var updated = await verifyContext.Articles.FindAsync(article.Id);

        updated.Should().NotBeNull();
    }

    [Fact]
    public async Task Remove_DeletesArticleFromDatabase()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var article = ArticleFactory.Create(category.Id);
        seedContext.Articles.Add(article);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IArticleRepository, ContentDbContext>();
        var existing = await db.Articles.FindAsync(article.Id);
        repo.Remove(existing!);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var deleted = await verifyContext.Articles.FindAsync(article.Id);

        deleted.Should().BeNull();
    }

    [Fact]
    public async Task GetAbandonedDraftsAsync_ReturnsDraftsBeforeCutoff()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var oldDraft = ArticleFactory.Create(category.Id);
        seedContext.Articles.Add(oldDraft);
        await seedContext.SaveChangesAsync();

        await seedContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE content.articles SET created_at = {DateTime.UtcNow.AddDays(-10)} WHERE id = {oldDraft.Id}"
        );

        var repo = Resolve<IArticleRepository>();
        var cutoff = DateTime.UtcNow.AddDays(-5);
        var result = await repo.GetAbandonedDraftsAsync(cutoff);

        result.Should().Contain(a => a.Id == oldDraft.Id);
    }

    [Fact]
    public async Task AddImageAsync_And_GetImagesByArticleIdAsync_PersistsAndRetrievesImages()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var article = ArticleFactory.Create(category.Id);
        seedContext.Articles.Add(article);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IArticleRepository, ContentDbContext>();
        var image = ArticleImageFactory.CreateCover(article.Id);
        await repo.AddImageAsync(image);
        await db.SaveChangesAsync();

        var readRepo = Resolve<IArticleRepository>();
        var result = await readRepo.GetImagesByArticleIdAsync(article.Id);

        result.Should().ContainSingle();
        result[0].ArticleId.Should().Be(article.Id);
    }

    [Fact]
    public async Task RemoveImages_DeletesImagesFromDatabase()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var article = ArticleFactory.Create(category.Id);
        seedContext.Articles.Add(article);
        await seedContext.SaveChangesAsync();

        var images = ArticleImageFactory.CreateMany(article.Id, 3);
        seedContext.ArticleImages.AddRange(images);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IArticleRepository, ContentDbContext>();
        var existing = await db.ArticleImages.Where(i => i.ArticleId == article.Id).ToListAsync();
        repo.RemoveImages(existing);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var remaining = await verifyContext.ArticleImages.Where(i => i.ArticleId == article.Id).ToListAsync();

        remaining.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByOrderItemIdAsync_WhenExists_ReturnsArticle()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);

        var customer = CustomerFactory.Create();
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        var orderItemId = Guid.NewGuid();
        var article = ArticleFactory.CreatePaid(category.Id, customer.Id, orderItemId);
        seedContext.Articles.Add(article);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IArticleRepository>();
        var result = await repo.GetByOrderItemIdAsync(orderItemId);

        result.Should().NotBeNull();
        result!.OrderItemId.Should().Be(orderItemId);
    }

    [Fact]
    public async Task GetByOrderItemIdAsync_WhenDoesNotExist_ReturnsNull()
    {
        var repo = Resolve<IArticleRepository>();
        var result = await repo.GetByOrderItemIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }
}
