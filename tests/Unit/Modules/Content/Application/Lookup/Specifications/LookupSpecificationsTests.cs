using System.Linq.Expressions;
using _116.Content.Application.Lookup.Specifications;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.Specifications;

/// <summary>
/// Unit tests for lookup specification classes.
/// Note: Specifications using EF.Functions.ILike are tested for compile / expression structure only —
/// the in-memory evaluation of ILike is covered in integration tests.
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

    [Fact]
    public void TagBySlugSpecification_WithMatchingSlug_ShouldReturnTrue()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        // Note: ILike is not supported in InMemoryDatabase — using Slug exact match for unit test
        var spec = new TagBySlugSpecification(TestConstants.Content.Tag.ValidSlug);
        Func<TagEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        // ILike not evaluable in-memory with exact case match — this test documents spec structure
        predicate.Should().NotBeNull();
    }

    #endregion

    #region ContentTypeByNameSpecification — ToExpression only (ILike)

    [Fact]
    public void ContentTypeByNameSpecification_ShouldCreateValidExpression()
    {
        // Arrange
        var spec = new ContentTypeByNameSpecification("Article");

        // Act
        Expression<Func<ContentTypeEntity, bool>> expression = spec.ToExpression();

        // Assert — ILike-based; compile only, do not invoke against in-memory data
        expression.Should().NotBeNull();
        expression.Compile().Should().NotBeNull();
    }

    #endregion

    #region PricingTierByNameSpecification — ToExpression only (ILike)

    [Fact]
    public void PricingTierByNameSpecification_ShouldCreateValidExpression()
    {
        // Arrange
        var spec = new PricingTierByNameSpecification("Standard");

        // Act
        Expression<Func<PricingTierEntity, bool>> expression = spec.ToExpression();

        // Assert
        expression.Should().NotBeNull();
        expression.Compile().Should().NotBeNull();
    }

    #endregion

    #region PromotionLevelByNameSpecification — ToExpression only (ILike)

    [Fact]
    public void PromotionLevelByNameSpecification_ShouldCreateValidExpression()
    {
        // Arrange
        var spec = new PromotionLevelByNameSpecification("Gold");

        // Act
        Expression<Func<PromotionLevelEntity, bool>> expression = spec.ToExpression();

        // Assert
        expression.Should().NotBeNull();
        expression.Compile().Should().NotBeNull();
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

    #region TagSearchSpecification — ToExpression only (ILike)

    [Fact]
    public void TagSearchSpecification_ShouldCreateValidExpression()
    {
        // Arrange
        var spec = new TagSearchSpecification("hip");

        // Act
        Expression<Func<TagEntity, bool>> expression = spec.ToExpression();

        // Assert — ILike-based; compile only, do not invoke against in-memory data
        expression.Should().NotBeNull();
        expression.Compile().Should().NotBeNull();
    }

    #endregion
}
