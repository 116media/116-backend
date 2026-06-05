using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Shared.Application.DTOs;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Mappers;

/// <summary>
/// Unit tests for <see cref="ArticleMapper"/>.
/// </summary>
public class ArticleMapperTests : BaseContentHandlerTest
{
    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly Guid ContentTypeId = Guid.NewGuid();

    #region Register

    [Fact]
    public void Register_ShouldNotThrowException()
    {
        Action act = () => MappingRegistration.CreateConfiguration();

        act.Should().NotThrow();
    }

    #endregion

    #region ToArticleSummaryDto — category name

    [Fact]
    public void ToArticleSummaryDto_WhenCategoryIsNull_ShouldMapCategoryNameAsEmptyString()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        // Category nav property is null by default (not loaded)

        // Act
        ArticleSummaryDto dto = article.ToArticleSummaryDto(Mapper);

        // Assert
        dto.CategoryName.Should().BeEmpty();
    }

    [Fact]
    public void ToArticleSummaryDto_WhenCategoryIsLoaded_ShouldMapCategoryName()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        article.GetType().GetProperty("Category")!.SetValue(article, category);

        // Act
        ArticleSummaryDto dto = article.ToArticleSummaryDto(Mapper);

        // Assert
        dto.CategoryName.Should().Be(category.Name);
    }

    #endregion

    #region ToArticleSummaryDto — core fields

    [Fact]
    public void ToArticleSummaryDto_ShouldMapCoreFields()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);

        // Act
        ArticleSummaryDto dto = article.ToArticleSummaryDto(Mapper);

        // Assert
        dto.Id.Should().Be(article.Id);
        dto.CategoryId.Should().Be(article.CategoryId);
        dto.Title.Should().Be(article.Title);
        dto.Slug.Should().Be(article.Slug);
        dto.Headline.Should().Be(article.Headline);
        dto.CoverImageUrl.Should().Be(article.CoverImageUrl);
        dto.AuthorId.Should().Be(article.AuthorId.ToString());
        dto.Status.Should().Be(article.Status);
        dto.IsPromoted.Should().Be(article.IsPromoted);
        dto.PublishedAt.Should().Be(article.PublishedAt);
    }

    #endregion

    #region ToArticleSummaryDto — AuditableDto fields

    [Fact]
    public void ToArticleSummaryDto_ShouldInheritAuditableDto()
    {
        typeof(ArticleSummaryDto).Should().BeAssignableTo<AuditableDto>();
    }

    [Fact]
    public void ToArticleSummaryDto_ShouldMapAuditFields()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.CreatedAt = new DateTime(2025, 3, 1, 8, 0, 0, DateTimeKind.Utc);
        article.CreatedBy = "creator";
        article.UpdatedAt = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        article.UpdatedBy = "updater";

        // Act
        ArticleSummaryDto dto = article.ToArticleSummaryDto(Mapper);

        // Assert
        dto.CreatedAt.Should().Be(article.CreatedAt);
        dto.CreatedBy.Should().Be("creator");
        dto.UpdatedAt.Should().Be(article.UpdatedAt);
        dto.UpdatedBy.Should().Be("updater");
    }

    #endregion

    #region ToArticleSummaryDto — promoted fields

    [Fact]
    public void ToArticleSummaryDto_WhenPromoted_ShouldMapIsPromotedTrue()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePromoted(CategoryId);

        // Act
        ArticleSummaryDto dto = article.ToArticleSummaryDto(Mapper);

        // Assert
        dto.IsPromoted.Should().BeTrue();
    }

    [Fact]
    public void ToArticleSummaryDto_WhenNotPromoted_ShouldMapIsPromotedFalse()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);

        // Act
        ArticleSummaryDto dto = article.ToArticleSummaryDto(Mapper);

        // Assert
        dto.IsPromoted.Should().BeFalse();
    }

    #endregion

    #region ToArticleSummaryDto — PromotionLevel nav property null safety

    [Fact]
    public void ToArticleSummaryDto_WhenPromotionLevelIsNull_ShouldNotThrow()
    {
        // Arrange — article with IsPromoted=true but PromotionLevel nav not loaded (null)
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7));
        // PromotionLevel nav property remains null (EF not loaded)

        // Act
        Action act = () => article.ToArticleSummaryDto(Mapper);

        // Assert — must not NPE
        act.Should().NotThrow();
    }

    #endregion

    #region ToArticleSummaryDtos — list mapping

    [Fact]
    public void ToArticleSummaryDtos_ShouldMapAllEntities()
    {
        // Arrange
        IReadOnlyList<ArticleEntity> articles = ArticleFactory.CreateMany(CategoryId, 3);

        // Act
        IReadOnlyList<ArticleSummaryDto> dtos = articles.ToArticleSummaryDtos(Mapper);

        // Assert
        dtos.Should().HaveCount(3);
    }

    [Fact]
    public void ToArticleSummaryDtos_WhenEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        IReadOnlyList<ArticleEntity> articles = [];

        // Act
        IReadOnlyList<ArticleSummaryDto> dtos = articles.ToArticleSummaryDtos(Mapper);

        // Assert
        dtos.Should().BeEmpty();
    }

    [Fact]
    public void ToArticleSummaryDtos_ShouldPreserveOrder()
    {
        // Arrange
        ArticleEntity first = ArticleFactory.Create(CategoryId);
        ArticleEntity second = ArticleFactory.Create(CategoryId);
        IReadOnlyList<ArticleEntity> articles = [first, second];

        // Act
        IReadOnlyList<ArticleSummaryDto> dtos = articles.ToArticleSummaryDtos(Mapper);

        // Assert
        dtos[0].Id.Should().Be(first.Id);
        dtos[1].Id.Should().Be(second.Id);
    }

    #endregion

    #region ToArticleDetailDto — AuditableDto inheritance

    [Fact]
    public void ToArticleDetailDto_ShouldInheritAuditableDto()
    {
        typeof(ArticleDetailDto).Should().BeAssignableTo<AuditableDto>();
    }

    [Fact]
    public void ToArticleDetailDto_ShouldMapCreatedAtFromEntity()
    {
        // Arrange
        DateTime createdAt = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.CreatedAt = createdAt;

        // Act
        ArticleDetailDto dto = article.ToArticleDetailDto(Mapper);

        // Assert
        dto.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void ToArticleDetailDto_ShouldMapCreatedByFromEntity()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.CreatedBy = "system";

        // Act
        ArticleDetailDto dto = article.ToArticleDetailDto(Mapper);

        // Assert
        dto.CreatedBy.Should().Be("system");
    }

    [Fact]
    public void ToArticleDetailDto_ShouldMapUpdatedAtFromEntity()
    {
        // Arrange
        DateTime updatedAt = new DateTime(2025, 6, 20, 14, 0, 0, DateTimeKind.Utc);
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.UpdatedAt = updatedAt;

        // Act
        ArticleDetailDto dto = article.ToArticleDetailDto(Mapper);

        // Assert
        dto.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void ToArticleDetailDto_ShouldMapUpdatedByFromEntity()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.UpdatedBy = "admin-user";

        // Act
        ArticleDetailDto dto = article.ToArticleDetailDto(Mapper);

        // Assert
        dto.UpdatedBy.Should().Be("admin-user");
    }

    #endregion

    #region ToArticleDetailDto — core field mapping

    [Fact]
    public void ToArticleDetailDto_ShouldMapCoreFields()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);

        // Act
        ArticleDetailDto dto = article.ToArticleDetailDto(Mapper);

        // Assert
        dto.Id.Should().Be(article.Id);
        dto.CategoryId.Should().Be(article.CategoryId);
        dto.AuthorId.Should().Be(article.AuthorId.ToString());
        dto.Status.Should().Be(article.Status);
        dto.Title.Should().Be(article.Title);
        dto.Slug.Should().Be(article.Slug);
        dto.Headline.Should().Be(article.Headline);
        dto.Body.Should().Be(article.Body);
        dto.CoverImageUrl.Should().Be(article.CoverImageUrl);
        dto.PublishedAt.Should().Be(article.PublishedAt);
    }

    #endregion

    #region ToArticleDetailDto — category name

    [Fact]
    public void ToArticleDetailDto_WhenCategoryIsNull_ShouldMapCategoryNameAsEmptyString()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);

        // Act
        ArticleDetailDto dto = article.ToArticleDetailDto(Mapper);

        // Assert
        dto.CategoryName.Should().BeEmpty();
    }

    [Fact]
    public void ToArticleDetailDto_WhenCategoryIsLoaded_ShouldMapCategoryName()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        article.GetType().GetProperty("Category")!.SetValue(article, category);

        // Act
        ArticleDetailDto dto = article.ToArticleDetailDto(Mapper);

        // Assert
        dto.CategoryName.Should().Be(category.Name);
    }

    #endregion

    #region ToArticleDetailDto — promotion fields

    [Fact]
    public void ToArticleDetailDto_WhenPromoted_ShouldMapPromotionFields()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePromoted(CategoryId);

        // Act
        ArticleDetailDto dto = article.ToArticleDetailDto(Mapper);

        // Assert
        dto.IsPromoted.Should().BeTrue();
        dto.PromotedUntil.Should().NotBeNull();
    }

    [Fact]
    public void ToArticleDetailDto_WhenNotPromoted_ShouldMapPromotionFieldsAsDefault()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);

        // Act
        ArticleDetailDto dto = article.ToArticleDetailDto(Mapper);

        // Assert
        dto.IsPromoted.Should().BeFalse();
        dto.PromotedUntil.Should().BeNull();
    }

    [Fact]
    public void ToArticleDetailDto_WhenPromotionLevelIsNull_ShouldNotThrow()
    {
        // Arrange — article has IsPromoted=true but PromotionLevel nav not loaded
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7));

        // Act
        Action act = () => article.ToArticleDetailDto(Mapper);

        // Assert — must not NPE
        act.Should().NotThrow();
    }

    #endregion

    #region ToArticleDetailDto — read time computation

    [Fact]
    public void ToArticleDetailDto_WhenBodyIsEmpty_ShouldReturnReadTimeOfOne()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        // Body is empty string by default on new articles

        // Act
        ArticleDetailDto dto = article.ToArticleDetailDto(Mapper);

        // Assert — Math.Max(1, ceil(0/200)) = 1
        dto.ReadTimeInMinutes.Should().Be(1);
    }

    [Theory]
    [InlineData(200, 1)]
    [InlineData(201, 2)]
    [InlineData(400, 2)]
    [InlineData(401, 3)]
    public void ToArticleDetailDto_ShouldComputeReadTimeInMinutes(int wordCount, int expectedMinutes)
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.Update(
            categoryId: CategoryId,
            title: article.Title,
            slug: article.Slug,
            headline: "headline",
            body: string.Join(" ", Enumerable.Repeat("word", wordCount)),
            coverImageUrl: null,
            customerId: null,
            orderItemId: null,
            socialBoost: false,
            metaTitle: null,
            metaDescription: null
        );

        // Act
        ArticleDetailDto dto = article.ToArticleDetailDto(Mapper);

        // Assert
        dto.ReadTimeInMinutes.Should().Be(expectedMinutes);
    }

    #endregion

    #region ToArticleDetailDto — SEO and optional fields

    [Fact]
    public void ToArticleDetailDto_ShouldMapRejectionReasonAndSocialBoost()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.Update(
            categoryId: CategoryId,
            title: article.Title,
            slug: article.Slug,
            headline: "headline",
            body: "body",
            coverImageUrl: null,
            customerId: null,
            orderItemId: null,
            socialBoost: true,
            metaTitle: "SEO Title",
            metaDescription: "SEO Description"
        );

        // Act
        ArticleDetailDto dto = article.ToArticleDetailDto(Mapper);

        // Assert
        dto.SocialBoost.Should().BeTrue();
        dto.MetaTitle.Should().Be("SEO Title");
        dto.MetaDescription.Should().Be("SEO Description");
    }

    #endregion

    #region ToArticleDetailDto — customer and order item mapping

    [Fact]
    public void ToArticleDetailDto_WhenFreeArticle_ShouldMapCustomerFieldsAsNull()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        article.GetType().GetProperty("Category")!.SetValue(article, category);

        // Act
        ArticleDetailDto dto = article.ToArticleDetailDto(Mapper);

        // Assert
        dto.CustomerId.Should().BeNull();
        dto.CustomerName.Should().BeNull();
        dto.OrderItemId.Should().BeNull();
    }

    [Fact]
    public void ToArticleDetailDto_WhenPaidArticle_ShouldMapCustomerAndOrderItemId()
    {
        // Arrange
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();

        ArticleEntity article = ArticleFactory.CreatePaid(CategoryId, customerId, orderItemId);
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        CustomerEntity customer = CustomerFactory.Create();

        article.GetType().GetProperty("Category")!.SetValue(article, category);
        article.GetType().GetProperty("Customer")!.SetValue(article, customer);

        // Act
        ArticleDetailDto dto = article.ToArticleDetailDto(Mapper);

        // Assert
        dto.CustomerId.Should().Be(customerId);
        dto.CustomerName.Should().Be(customer.FullName);
        dto.OrderItemId.Should().Be(orderItemId);
    }

    [Fact]
    public void ToArticleDetailDto_WhenCustomerNavIsNull_ShouldMapCustomerNameAsNull()
    {
        // Arrange — CustomerId is set but Customer nav not loaded
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();
        ArticleEntity article = ArticleFactory.CreatePaid(CategoryId, customerId, orderItemId);
        // Customer nav property left null

        // Act
        ArticleDetailDto dto = article.ToArticleDetailDto(Mapper);

        // Assert
        dto.CustomerId.Should().Be(customerId);
        dto.CustomerName.Should().BeNull();
    }

    #endregion

    #region ToArticleCommentDto — body nulling on deleted comments

    [Fact]
    public void ToArticleCommentDto_WhenNotDeleted_ShouldReturnBody()
    {
        // Arrange
        Guid articleId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        ArticleCommentEntity comment = ArticleCommentFactory.Create(articleId, userId);

        // Act
        ArticleCommentDto dto = comment.ToArticleCommentDto(Mapper);

        // Assert
        dto.Body.Should().NotBeNull();
        dto.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void ToArticleCommentDto_WhenDeleted_ShouldReturnNullBody()
    {
        // Arrange
        Guid articleId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        ArticleCommentEntity comment = ArticleCommentFactory.CreateDeleted(articleId, userId);

        // Act
        ArticleCommentDto dto = comment.ToArticleCommentDto(Mapper);

        // Assert
        dto.Body.Should().BeNull();
        dto.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region ToArticleCommentDtos — list mapping

    [Fact]
    public void ToArticleCommentDtos_ShouldMapEachComment()
    {
        // Arrange
        Guid articleId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        List<ArticleCommentEntity> comments =
        [
            ArticleCommentFactory.Create(articleId, userId),
            ArticleCommentFactory.CreateDeleted(articleId, userId),
        ];

        // Act
        IReadOnlyList<ArticleCommentDto> dtos = comments.ToArticleCommentDtos(Mapper);

        // Assert
        dtos.Should().HaveCount(2);
        dtos[0].Body.Should().NotBeNull();
        dtos[1].Body.Should().BeNull();
    }

    [Fact]
    public void ToArticleCommentDtos_WhenEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        IReadOnlyList<ArticleCommentEntity> comments = [];

        // Act
        IReadOnlyList<ArticleCommentDto> dtos = comments.ToArticleCommentDtos(Mapper);

        // Assert
        dtos.Should().BeEmpty();
    }

    #endregion
}
