using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Core.Application.Shared.Repositories;
using _116.Tests.Fixtures.Factories.Content;
using MapsterMapper;
using ContentMappingRegistration = _116.Content.Application.Shared.Mappers.MappingRegistration;

namespace _116.Integration.Tests.Modules.Content.Mappers;

/// <summary>
/// Integration tests for <see cref="ArticleMapper" />.
/// Verifies entity-to-DTO mapping with navigation properties and file resolution.
/// </summary>
[Collection("Database")]
public class ArticleMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private readonly IMapper _mapper = new Mapper(ContentMappingRegistration.CreateConfiguration());

    [Fact]
    public async Task ToArticleSummaryDtoAsync_ShouldMapAllFields()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Article");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id, "News", "news");
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var article = ArticleFactory.Create(category.Id, "Test Article", "test-article");
        seedContext.Articles.Add(article);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        ArticleEntity loaded = await readContext.Articles.Include(a => a.Category).FirstAsync(a => a.Id == article.Id);

        var fileRepository = Resolve<IFileRepository>();
        ArticleSummaryDto dto = await loaded.ToArticleSummaryDtoAsync(_mapper, fileRepository);

        dto.Id.Should().Be(loaded.Id);
        dto.CategoryId.Should().Be(category.Id);
        dto.CategoryName.Should().Be("News");
        dto.Title.Should().Be("Test Article");
        dto.Slug.Should().Be("test-article");
        dto.Status.Should().Be(loaded.Status);
    }

    [Fact]
    public async Task ToArticleSummaryDtoAsync_WithNoCoverImage_ShouldMapCoverImageUrlAsNull()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Article");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var article = ArticleFactory.Create(category.Id);
        seedContext.Articles.Add(article);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        ArticleEntity loaded = await readContext.Articles.Include(a => a.Category).FirstAsync(a => a.Id == article.Id);

        var fileRepository = Resolve<IFileRepository>();
        ArticleSummaryDto dto = await loaded.ToArticleSummaryDtoAsync(_mapper, fileRepository);

        dto.CoverImageUrl.Should().BeNull();
    }

    [Fact]
    public async Task ToArticleDetailDtoAsync_ShouldMapAllDetailFields()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Article");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id, "Culture", "culture");
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var article = ArticleFactory.Create(category.Id);
        article.Update(
            categoryId: category.Id,
            title: article.Title,
            slug: article.Slug,
            headline: article.Headline,
            body: string.Join(' ', Enumerable.Repeat("mot", 250)),
            customerId: article.CustomerId,
            orderItemId: article.OrderItemId,
            socialBoost: article.SocialBoost,
            metaTitle: article.MetaTitle,
            metaDescription: article.MetaDescription
        );
        seedContext.Articles.Add(article);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        ArticleEntity loaded = await readContext
            .Articles.Include(a => a.Category)
            .Include(a => a.Images)
            .Include(a => a.Tags)
                .ThenInclude(at => at.Tag)
            .Include(a => a.PromotionLevel)
            .Include(a => a.Customer)
            .FirstAsync(a => a.Id == article.Id);

        var fileRepository = Resolve<IFileRepository>();
        ArticleDetailDto dto = await loaded.ToArticleDetailDtoAsync(_mapper, fileRepository);

        dto.Id.Should().Be(loaded.Id);
        dto.CategoryName.Should().Be("Culture");
        dto.Title.Should().Be(loaded.Title);
        dto.Body.Should().Be(loaded.Body);
        dto.ReadTimeInMinutes.Should().Be(2);
    }

    [Fact]
    public async Task ToArticleSummaryDtosAsync_ShouldMapCollection()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Article");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var a1 = ArticleFactory.Create(category.Id);
        var a2 = ArticleFactory.Create(category.Id);
        seedContext.Articles.AddRange(a1, a2);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        List<ArticleEntity> loaded = await readContext.Articles.Include(a => a.Category).ToListAsync();

        var fileRepository = Resolve<IFileRepository>();
        IReadOnlyList<ArticleSummaryDto> dtos = await loaded.ToArticleSummaryDtosAsync(_mapper, fileRepository);

        dtos.Should().HaveCount(2);
    }
}
