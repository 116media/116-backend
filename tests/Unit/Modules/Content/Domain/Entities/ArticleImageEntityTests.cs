using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="ArticleImageEntity"/>.
/// </summary>
public class ArticleImageEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidParams_ShouldCreateEntity()
    {
        // Arrange
        var id = Guid.NewGuid();
        var articleId = Guid.NewGuid();
        const string storageKey = TestConstants.Content.Editorial.ArticleImage.ValidStorageKey;
        const string url = TestConstants.Content.Editorial.ArticleImage.ValidUrl;

        // Act
        ArticleImageEntity image = ArticleImageEntity.Create(id, articleId, storageKey, url, EnumArticleImageType.Body);

        // Assert
        image.Id.Should().Be(id);
        image.ArticleId.Should().Be(articleId);
        image.StorageKey.Should().Be(storageKey);
        image.Url.Should().Be(url);
        image.ImageType.Should().Be(EnumArticleImageType.Body);
    }

    [Fact]
    public void Create_AsCover_ShouldSetImageTypeCover()
    {
        // Act
        ArticleImageEntity image = ArticleImageEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TestConstants.Content.Editorial.ArticleImage.ValidStorageKey,
            TestConstants.Content.Editorial.ArticleImage.ValidUrl,
            EnumArticleImageType.Cover
        );

        // Assert
        image.ImageType.Should().Be(EnumArticleImageType.Cover);
    }

    [Fact]
    public void Create_AsBody_ShouldSetImageTypeBody()
    {
        // Act
        ArticleImageEntity image = ArticleImageEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TestConstants.Content.Editorial.ArticleImage.ValidStorageKey,
            TestConstants.Content.Editorial.ArticleImage.ValidUrl,
            EnumArticleImageType.Body
        );

        // Assert
        image.ImageType.Should().Be(EnumArticleImageType.Body);
    }

    #endregion
}
