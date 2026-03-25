using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
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

    #region Register

    [Fact]
    public void Register_ShouldNotThrowException()
    {
        // Act
        Action act = () => MappingRegistration.CreateConfiguration();

        // Assert
        act.Should().NotThrow();
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
        dto.AuthorId.Should().Be(article.AuthorId.ToString());
        dto.Status.Should().Be(article.Status);
        dto.Title.Should().Be(article.Title);
        dto.Slug.Should().Be(article.Slug);
    }

    #endregion

    #region ToArticleDetailDto — read time computation

    [Fact]
    public void ToArticleDetailDto_ShouldComputeReadTimeInMinutes()
    {
        // Arrange — 200 words → 1 min, 400 words → 2 min
        ArticleEntity article200 = ArticleFactory.Create(CategoryId);
        article200.Update(
            categoryId: CategoryId,
            title: article200.Title,
            slug: article200.Slug,
            headline: "headline",
            body: string.Join(" ", Enumerable.Repeat("word", 200)),
            coverImageUrl: null,
            customerId: null,
            orderItemId: null,
            socialBoost: false,
            isFeatured: false,
            featuredUntil: null,
            metaTitle: null,
            metaDescription: null
        );

        ArticleEntity article400 = ArticleFactory.Create(CategoryId);
        article400.Update(
            categoryId: CategoryId,
            title: article400.Title,
            slug: article400.Slug,
            headline: "headline",
            body: string.Join(" ", Enumerable.Repeat("word", 400)),
            coverImageUrl: null,
            customerId: null,
            orderItemId: null,
            socialBoost: false,
            isFeatured: false,
            featuredUntil: null,
            metaTitle: null,
            metaDescription: null
        );

        // Act
        ArticleDetailDto dto200 = article200.ToArticleDetailDto(Mapper);
        ArticleDetailDto dto400 = article400.ToArticleDetailDto(Mapper);

        // Assert
        dto200.ReadTimeInMinutes.Should().Be(1);
        dto400.ReadTimeInMinutes.Should().Be(2);
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

    #endregion
}
