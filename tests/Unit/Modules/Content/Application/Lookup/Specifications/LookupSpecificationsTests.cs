using _116.Content.Application.Lookup.Specifications;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.Specifications;

/// <summary>
/// Unit tests for lookup specification classes.
/// Specifications using EF.Functions.ILike are evaluated through
/// <see cref="ILikeSpecificationEvaluator" />, which rewrites ILike for in-memory execution.
/// </summary>
public class LookupSpecificationsTests
{
    #region ContentTypeByIdSpecification

    [Fact]
    public void ContentTypeByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.CreateDefault();
        var spec = new ContentTypeByIdSpecification(contentType.Id);
        Func<ContentTypeEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(contentType).Should().BeTrue();
    }

    [Fact]
    public void ContentTypeByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.CreateDefault();
        var spec = new ContentTypeByIdSpecification(Guid.NewGuid());
        Func<ContentTypeEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(contentType).Should().BeFalse();
    }

    #endregion

    #region ActiveContentTypeSpecification

    [Fact]
    public void ActiveContentTypeSpecification_WithActiveContentType_ShouldReturnTrue()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.CreateDefault();
        var spec = new ActiveContentTypeSpecification();
        Func<ContentTypeEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(contentType).Should().BeTrue();
    }

    [Fact]
    public void ActiveContentTypeSpecification_WithInactiveContentType_ShouldReturnFalse()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.CreateInactive();
        var spec = new ActiveContentTypeSpecification();
        Func<ContentTypeEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(contentType).Should().BeFalse();
    }

    #endregion

    #region PricingTierByIdSpecification

    [Fact]
    public void PricingTierByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        PricingTierEntity tier = PricingTierFactory.CreateDefault();
        var spec = new PricingTierByIdSpecification(tier.Id);
        Func<PricingTierEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(tier).Should().BeTrue();
    }

    [Fact]
    public void PricingTierByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        PricingTierEntity tier = PricingTierFactory.CreateDefault();
        var spec = new PricingTierByIdSpecification(Guid.NewGuid());
        Func<PricingTierEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(tier).Should().BeFalse();
    }

    #endregion

    #region PromotionLevelByIdSpecification

    [Fact]
    public void PromotionLevelByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        PromotionLevelEntity level = PromotionLevelFactory.CreateDefault();
        var spec = new PromotionLevelByIdSpecification(level.Id);
        Func<PromotionLevelEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(level).Should().BeTrue();
    }

    [Fact]
    public void PromotionLevelByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        PromotionLevelEntity level = PromotionLevelFactory.CreateDefault();
        var spec = new PromotionLevelByIdSpecification(Guid.NewGuid());
        Func<PromotionLevelEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(level).Should().BeFalse();
    }

    #endregion

    #region ActivePromotionLevelSpecification

    [Fact]
    public void ActivePromotionLevelSpecification_WithActiveLevel_ShouldReturnTrue()
    {
        // Arrange
        PromotionLevelEntity level = PromotionLevelFactory.CreateDefault();
        var spec = new ActivePromotionLevelSpecification();
        Func<PromotionLevelEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(level).Should().BeTrue();
    }

    [Fact]
    public void ActivePromotionLevelSpecification_WithInactiveLevel_ShouldReturnFalse()
    {
        // Arrange
        PromotionLevelEntity level = PromotionLevelFactory.CreateInactive();
        var spec = new ActivePromotionLevelSpecification();
        Func<PromotionLevelEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(level).Should().BeFalse();
    }

    #endregion

    #region TagByIdSpecification

    [Fact]
    public void TagByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        var spec = new TagByIdSpecification(tag.Id);
        Func<TagEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(tag).Should().BeTrue();
    }

    [Fact]
    public void TagByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        var spec = new TagByIdSpecification(Guid.NewGuid());
        Func<TagEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(tag).Should().BeFalse();
    }

    #endregion

    #region TagBySlugSpecification

    [Theory]
    [InlineData(TestConstants.Tag.ValidSlug, true)]
    [InlineData(TestConstants.Tag.AnotherValidSlug, false)]
    public void TagBySlugSpecification_ShouldMatchSlugExactly(string slug, bool expected)
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        var spec = new TagBySlugSpecification(slug);

        // Act
        bool result = spec.IsSatisfiedBy(tag);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region ContentTypeByNameSpecification

    [Theory]
    [InlineData("Article", true)]
    [InlineData("ARTICLE", true)]
    [InlineData("Art", false)]
    [InlineData("Video", false)]
    public void ContentTypeByNameSpecification_ShouldMatchWholeNameCaseInsensitively(string name, bool expected)
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create("Article");
        var spec = new ContentTypeByNameSpecification(name);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(contentType);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region PricingTierByNameSpecification

    [Theory]
    [InlineData("base_upload", true)]
    [InlineData("BASE_UPLOAD", true)]
    [InlineData("base", false)]
    [InlineData("premium_upload", false)]
    public void PricingTierByNameSpecification_ShouldMatchWholeNameCaseInsensitively(string name, bool expected)
    {
        // Arrange
        PricingTierEntity tier = PricingTierFactory.Create("base_upload");
        var spec = new PricingTierByNameSpecification(name);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(tier);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region PromotionLevelByNameSpecification

    [Theory]
    [InlineData("Gold", true)]
    [InlineData("GOLD", true)]
    [InlineData("Gol", false)]
    [InlineData("Silver", false)]
    public void PromotionLevelByNameSpecification_ShouldMatchWholeNameCaseInsensitively(string name, bool expected)
    {
        // Arrange
        PromotionLevelEntity level = new PromotionLevelBuilder().WithName("Gold").Build();
        var spec = new PromotionLevelByNameSpecification(name);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(level);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region TagByNameSpecification — exact equality (no ILike)

    [Fact]
    public void TagByNameSpecification_WithMatchingName_ShouldReturnTrue()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        var spec = new TagByNameSpecification(tag.Name);
        Func<TagEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(tag).Should().BeTrue();
    }

    [Fact]
    public void TagByNameSpecification_WithDifferentName_ShouldReturnFalse()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        var spec = new TagByNameSpecification("nonexistent-tag-name");
        Func<TagEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(tag).Should().BeFalse();
    }

    #endregion

    #region ContentTypeSearchSpecification

    [Theory]
    [InlineData("art", true)]
    [InlineData("ARTICLE", true)]
    [InlineData("video", false)]
    public void ContentTypeSearchSpecification_ShouldMatchNameSubstringCaseInsensitively(string search, bool expected)
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create("Article");
        var spec = new ContentTypeSearchSpecification(search);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(contentType);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region PricingTierSearchSpecification

    [Theory]
    [InlineData("base", true)]
    [InlineData("BASE_UPLOAD", true)]
    [InlineData("standard tier", true)]
    [InlineData("premium", false)]
    public void PricingTierSearchSpecification_ShouldMatchNameOrDescriptionCaseInsensitively(
        string search,
        bool expected
    )
    {
        // Arrange
        PricingTierEntity tier = PricingTierFactory.CreateWithDescription("base_upload", "The standard tier");
        var spec = new PricingTierSearchSpecification(search);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(tier);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region PromotionLevelSearchSpecification

    [Theory]
    [InlineData("feat", true)]
    [InlineData("FEATURED", true)]
    [InlineData("spotlight", false)]
    public void PromotionLevelSearchSpecification_ShouldMatchNameSubstringCaseInsensitively(
        string search,
        bool expected
    )
    {
        // Arrange
        PromotionLevelEntity level = new PromotionLevelBuilder().WithName("Featured").Build();
        var spec = new PromotionLevelSearchSpecification(search);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(level);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region TagSearchSpecification

    [Theory]
    [InlineData("hip", true)]
    [InlineData("HIP-HOP", true)]
    [InlineData("Hip Hop", true)]
    [InlineData("rumba", false)]
    public void TagSearchSpecification_ShouldMatchNameOrSlugCaseInsensitively(string search, bool expected)
    {
        // Arrange
        TagEntity tag = TagFactory.Create("Hip Hop", "hip-hop");
        var spec = new TagSearchSpecification(search);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(tag);

        // Assert
        result.Should().Be(expected);
    }

    #endregion
}
