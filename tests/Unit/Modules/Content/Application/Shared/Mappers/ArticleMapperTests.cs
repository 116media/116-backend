using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Application.DTOs;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Mappers;

/// <summary>
/// Unit tests for <see cref="ArticleMapper"/>.
/// </summary>
public class ArticleMapperTests : BaseContentHandlerTest
{
    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly Guid ContentTypeId = Guid.NewGuid();

    private readonly Mock<IFileRepository> _fileRepositoryMock;

    public ArticleMapperTests()
    {
        _fileRepositoryMock = MockFileRepository.Create();
        FileEntity videoFile = FileFactory.CreateVideo();
        _fileRepositoryMock.SetupGetById(videoFile);
    }

    #region Register

    [Fact]
    public void Register_ShouldNotThrowException()
    {
        Action act = () => MappingRegistration.CreateConfiguration();

        act.Should().NotThrow();
    }

    #endregion

    #region ToArticleSummaryDtoAsync — category name

    [Fact]
    public async Task ToArticleSummaryDtoAsync_WhenCategoryIsNull_ShouldMapCategoryNameAsEmptyString()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        // Category nav property is null by default (not loaded)

        // Act
        ArticleSummaryDto dto = await article.ToArticleSummaryDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.CategoryName.Should().BeEmpty();
    }

    [Fact]
    public async Task ToArticleSummaryDtoAsync_WhenCategoryIsLoaded_ShouldMapCategoryName()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        ArticleEntity article = new ArticleBuilder(CategoryId).WithCategory(category).Build();

        // Act
        ArticleSummaryDto dto = await article.ToArticleSummaryDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.CategoryName.Should().Be(category.Name);
    }

    #endregion

    #region ToArticleSummaryDtoAsync — core fields

    [Fact]
    public async Task ToArticleSummaryDtoAsync_ShouldMapCoreFields()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);

        // Act
        ArticleSummaryDto dto = await article.ToArticleSummaryDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.Id.Should().Be(article.Id);
        dto.CategoryId.Should().Be(article.CategoryId);
        dto.Title.Should().Be(article.Title);
        dto.Slug.Should().Be(article.Slug);
        dto.Headline.Should().Be(article.Headline);
        dto.AuthorId.Should().Be(article.AuthorId.ToString());
        dto.Status.Should().Be(article.Status);
        dto.IsPromoted.Should().Be(article.IsPromoted);
        dto.PublishedAt.Should().Be(article.PublishedAt);
    }

    #endregion

    #region ToArticleSummaryDtoAsync — AuditableDto fields

    [Fact]
    public void ToArticleSummaryDtoAsync_ShouldInheritAuditableDto()
    {
        typeof(ArticleSummaryDto).Should().BeAssignableTo<AuditableDto>();
    }

    [Fact]
    public async Task ToArticleSummaryDtoAsync_ShouldMapAuditFields()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.CreatedAt = new DateTime(2025, 3, 1, 8, 0, 0, DateTimeKind.Utc);
        article.CreatedBy = "creator";
        article.UpdatedAt = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        article.UpdatedBy = "updater";

        // Act
        ArticleSummaryDto dto = await article.ToArticleSummaryDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.CreatedAt.Should().Be(article.CreatedAt);
        dto.CreatedBy.Should().Be("creator");
        dto.UpdatedAt.Should().Be(article.UpdatedAt);
        dto.UpdatedBy.Should().Be("updater");
    }

    #endregion

    #region ToArticleSummaryDtoAsync — promoted fields

    [Fact]
    public async Task ToArticleSummaryDtoAsync_WhenPromoted_ShouldMapIsPromotedTrue()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePromoted(CategoryId);

        // Act
        ArticleSummaryDto dto = await article.ToArticleSummaryDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.IsPromoted.Should().BeTrue();
    }

    [Fact]
    public async Task ToArticleSummaryDtoAsync_WhenNotPromoted_ShouldMapIsPromotedFalse()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);

        // Act
        ArticleSummaryDto dto = await article.ToArticleSummaryDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.IsPromoted.Should().BeFalse();
    }

    #endregion

    #region ToArticleSummaryDtoAsync — PromotionLevel nav property null safety

    [Fact]
    public async Task ToArticleSummaryDtoAsync_WhenPromotionLevelIsNull_ShouldNotThrow()
    {
        // Arrange — article with IsPromoted=true but PromotionLevel nav not loaded (null)
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7));
        // PromotionLevel nav property remains null (EF not loaded)

        // Act
        Func<Task> act = async () =>
            await article.ToArticleSummaryDtoAsync(Mapper, _fileRepositoryMock.Object, CancellationToken.None);

        // Assert — must not NPE
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region ToArticleSummaryDtosAsync — list mapping

    [Fact]
    public async Task ToArticleSummaryDtosAsync_ShouldMapAllEntities()
    {
        // Arrange
        IReadOnlyList<ArticleEntity> articles = ArticleFactory.CreateMany(CategoryId, 3);

        // Act
        IReadOnlyList<ArticleSummaryDto> dtos = await articles.ToArticleSummaryDtosAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dtos.Should().HaveCount(3);
    }

    [Fact]
    public async Task ToArticleSummaryDtosAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        IReadOnlyList<ArticleEntity> articles = [];

        // Act
        IReadOnlyList<ArticleSummaryDto> dtos = await articles.ToArticleSummaryDtosAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dtos.Should().BeEmpty();
    }

    [Fact]
    public async Task ToArticleSummaryDtosAsync_ShouldPreserveOrder()
    {
        // Arrange
        ArticleEntity first = ArticleFactory.Create(CategoryId);
        ArticleEntity second = ArticleFactory.Create(CategoryId);
        IReadOnlyList<ArticleEntity> articles = [first, second];

        // Act
        IReadOnlyList<ArticleSummaryDto> dtos = await articles.ToArticleSummaryDtosAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dtos[0].Id.Should().Be(first.Id);
        dtos[1].Id.Should().Be(second.Id);
    }

    #endregion

    #region ToArticleDetailDtoAsync — AuditableDto inheritance

    [Fact]
    public void ToArticleDetailDtoAsync_ShouldInheritAuditableDto()
    {
        typeof(ArticleDetailDto).Should().BeAssignableTo<AuditableDto>();
    }

    [Fact]
    public async Task ToArticleDetailDtoAsync_ShouldMapCreatedAtFromEntity()
    {
        // Arrange
        DateTime createdAt = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.CreatedAt = createdAt;

        // Act
        ArticleDetailDto dto = await article.ToArticleDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public async Task ToArticleDetailDtoAsync_ShouldMapCreatedByFromEntity()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.CreatedBy = "system";

        // Act
        ArticleDetailDto dto = await article.ToArticleDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.CreatedBy.Should().Be("system");
    }

    [Fact]
    public async Task ToArticleDetailDtoAsync_ShouldMapUpdatedAtFromEntity()
    {
        // Arrange
        DateTime updatedAt = new DateTime(2025, 6, 20, 14, 0, 0, DateTimeKind.Utc);
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.UpdatedAt = updatedAt;

        // Act
        ArticleDetailDto dto = await article.ToArticleDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public async Task ToArticleDetailDtoAsync_ShouldMapUpdatedByFromEntity()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.UpdatedBy = "admin-user";

        // Act
        ArticleDetailDto dto = await article.ToArticleDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.UpdatedBy.Should().Be("admin-user");
    }

    #endregion

    #region ToArticleDetailDtoAsync — core field mapping

    [Fact]
    public async Task ToArticleDetailDtoAsync_ShouldMapCoreFields()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);

        // Act
        ArticleDetailDto dto = await article.ToArticleDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.Id.Should().Be(article.Id);
        dto.CategoryId.Should().Be(article.CategoryId);
        dto.AuthorId.Should().Be(article.AuthorId.ToString());
        dto.Status.Should().Be(article.Status);
        dto.Title.Should().Be(article.Title);
        dto.Slug.Should().Be(article.Slug);
        dto.Headline.Should().Be(article.Headline);
        dto.Body.Should().Be(article.Body);
        dto.PublishedAt.Should().Be(article.PublishedAt);
    }

    #endregion

    #region ToArticleDetailDtoAsync — category name

    [Fact]
    public async Task ToArticleDetailDtoAsync_WhenCategoryIsNull_ShouldMapCategoryNameAsEmptyString()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);

        // Act
        ArticleDetailDto dto = await article.ToArticleDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.CategoryName.Should().BeEmpty();
    }

    [Fact]
    public async Task ToArticleDetailDtoAsync_WhenCategoryIsLoaded_ShouldMapCategoryName()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        ArticleEntity article = new ArticleBuilder(CategoryId).WithCategory(category).Build();

        // Act
        ArticleDetailDto dto = await article.ToArticleDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.CategoryName.Should().Be(category.Name);
    }

    #endregion

    #region ToArticleDetailDtoAsync — promotion fields

    [Fact]
    public async Task ToArticleDetailDtoAsync_WhenPromoted_ShouldMapPromotionFields()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePromoted(CategoryId);

        // Act
        ArticleDetailDto dto = await article.ToArticleDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.IsPromoted.Should().BeTrue();
        dto.PromotedUntil.Should().NotBeNull();
    }

    [Fact]
    public async Task ToArticleDetailDtoAsync_WhenNotPromoted_ShouldMapPromotionFieldsAsDefault()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);

        // Act
        ArticleDetailDto dto = await article.ToArticleDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.IsPromoted.Should().BeFalse();
        dto.PromotedUntil.Should().BeNull();
    }

    [Fact]
    public async Task ToArticleDetailDtoAsync_WhenPromotionLevelIsNull_ShouldNotThrow()
    {
        // Arrange — article has IsPromoted=true but PromotionLevel nav not loaded
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7));

        // Act
        Func<Task> act = async () =>
            await article.ToArticleDetailDtoAsync(Mapper, _fileRepositoryMock.Object, CancellationToken.None);

        // Assert — must not NPE
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region ToArticleDetailDtoAsync — read time computation

    [Fact]
    public async Task ToArticleDetailDtoAsync_WhenBodyIsEmpty_ShouldReturnReadTimeOfOne()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        // Body is empty string by default on new articles

        // Act
        ArticleDetailDto dto = await article.ToArticleDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert — Math.Max(1, ceil(0/200)) = 1
        dto.ReadTimeInMinutes.Should().Be(1);
    }

    [Theory]
    [InlineData(200, 1)]
    [InlineData(201, 2)]
    [InlineData(400, 2)]
    [InlineData(401, 3)]
    public async Task ToArticleDetailDtoAsync_ShouldComputeReadTimeInMinutes(int wordCount, int expectedMinutes)
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.Update(
            categoryId: CategoryId,
            title: article.Title,
            slug: article.Slug,
            headline: "headline",
            body: string.Join(" ", Enumerable.Repeat("word", wordCount)),
            customerId: null,
            orderItemId: null,
            socialBoost: false,
            metaTitle: null,
            metaDescription: null
        );

        // Act
        ArticleDetailDto dto = await article.ToArticleDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.ReadTimeInMinutes.Should().Be(expectedMinutes);
    }

    #endregion

    #region ToArticleDetailDtoAsync — SEO and optional fields

    [Fact]
    public async Task ToArticleDetailDtoAsync_ShouldMapRejectionReasonAndSocialBoost()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.Update(
            categoryId: CategoryId,
            title: article.Title,
            slug: article.Slug,
            headline: "headline",
            body: "body",
            customerId: null,
            orderItemId: null,
            socialBoost: true,
            metaTitle: "SEO Title",
            metaDescription: "SEO Description"
        );

        // Act
        ArticleDetailDto dto = await article.ToArticleDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.SocialBoost.Should().BeTrue();
        dto.MetaTitle.Should().Be("SEO Title");
        dto.MetaDescription.Should().Be("SEO Description");
    }

    #endregion

    #region ToArticleDetailDtoAsync — customer and order item mapping

    [Fact]
    public async Task ToArticleDetailDtoAsync_WhenFreeArticle_ShouldMapCustomerFieldsAsNull()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        ArticleEntity article = new ArticleBuilder(CategoryId).WithCategory(category).Build();

        // Act
        ArticleDetailDto dto = await article.ToArticleDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.CustomerId.Should().BeNull();
        dto.CustomerName.Should().BeNull();
        dto.OrderItemId.Should().BeNull();
    }

    [Fact]
    public async Task ToArticleDetailDtoAsync_WhenPaidArticle_ShouldMapCustomerAndOrderItemId()
    {
        // Arrange
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();

        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        CustomerEntity customer = CustomerFactory.Create();
        ArticleEntity article = new ArticleBuilder(CategoryId)
            .WithCustomer(customerId, orderItemId)
            .WithCategory(category)
            .WithCustomerNavigation(customer)
            .Build();

        // Act
        ArticleDetailDto dto = await article.ToArticleDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.CustomerId.Should().Be(customerId);
        dto.CustomerName.Should().Be(customer.FullName);
        dto.OrderItemId.Should().Be(orderItemId);
    }

    [Fact]
    public async Task ToArticleDetailDtoAsync_WhenCustomerNavIsNull_ShouldMapCustomerNameAsNull()
    {
        // Arrange — CustomerId is set but Customer nav not loaded
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();
        ArticleEntity article = ArticleFactory.CreatePaid(CategoryId, customerId, orderItemId);
        // Customer nav property left null

        // Act
        ArticleDetailDto dto = await article.ToArticleDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.CustomerId.Should().Be(customerId);
        dto.CustomerName.Should().BeNull();
    }

    #endregion

    #region ToArticleSummaryDtoAsync — engagement counters

    [Fact]
    public async Task ToArticleSummaryDtoAsync_ShouldMapEngagementCounters()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.IncrementLikeCount();
        article.IncrementLikeCount();
        article.IncrementCommentCount();
        article.IncrementShareCount();
        article.IncrementBookmarkCount();
        article.IncrementBookmarkCount();
        article.IncrementBookmarkCount();

        // Act
        ArticleSummaryDto dto = await article.ToArticleSummaryDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.LikeCount.Should().Be(2);
        dto.CommentCount.Should().Be(1);
        dto.ShareCount.Should().Be(1);
        dto.BookmarkCount.Should().Be(3);
    }

    [Fact]
    public async Task ToArticleSummaryDtoAsync_WhenNoInteractions_ShouldMapCountersAsZero()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);

        // Act
        ArticleSummaryDto dto = await article.ToArticleSummaryDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.LikeCount.Should().Be(0);
        dto.CommentCount.Should().Be(0);
        dto.ShareCount.Should().Be(0);
        dto.BookmarkCount.Should().Be(0);
    }

    #endregion

    #region ToArticleDetailDtoAsync — engagement counters

    [Fact]
    public async Task ToArticleDetailDtoAsync_ShouldMapEngagementCounters()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.IncrementLikeCount();
        article.IncrementCommentCount();
        article.IncrementCommentCount();
        article.IncrementShareCount();
        article.IncrementShareCount();
        article.IncrementShareCount();
        article.IncrementBookmarkCount();

        // Act
        ArticleDetailDto dto = await article.ToArticleDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.LikeCount.Should().Be(1);
        dto.CommentCount.Should().Be(2);
        dto.ShareCount.Should().Be(3);
        dto.BookmarkCount.Should().Be(1);
    }

    [Fact]
    public async Task ToArticleDetailDtoAsync_WhenNoInteractions_ShouldMapCountersAsZero()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);

        // Act
        ArticleDetailDto dto = await article.ToArticleDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.LikeCount.Should().Be(0);
        dto.CommentCount.Should().Be(0);
        dto.ShareCount.Should().Be(0);
        dto.BookmarkCount.Should().Be(0);
    }

    [Fact]
    public async Task ToArticleDetailDtoAsync_WhenLikeCountDecremented_ShouldNotGoBelowZero()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.DecrementLikeCount();

        // Act
        ArticleDetailDto dto = await article.ToArticleDetailDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.LikeCount.Should().Be(0);
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
        IReadOnlyList<ArticleCommentDto> dtos = comments.ToArticleCommentDtos(
            Mapper,
            new Dictionary<Guid, AuthorDto>()
        );

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
        IReadOnlyList<ArticleCommentDto> dtos = comments.ToArticleCommentDtos(
            Mapper,
            new Dictionary<Guid, AuthorDto>()
        );

        // Assert
        dtos.Should().BeEmpty();
    }

    #endregion
}
