using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using AwesomeAssertions;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Shared.Mappers;

/// <summary>
/// Unit tests for <see cref="ContentLookups"/>.
/// </summary>
public class ContentLookupsTests
{
    private static readonly Guid ContentTypeId = Guid.NewGuid();

    /// <summary>
    /// The rows a projection names, as the DTO factory hands them to the mapper.
    /// </summary>
    private static ContentLookups Lookups(
        CategoryEntity? category = null,
        CustomerEntity? customer = null,
        PromotionLevelEntity? promotionLevel = null
    ) =>
        new(
            Categories: category is null
                ? new Dictionary<Guid, CategoryEntity>()
                : new Dictionary<Guid, CategoryEntity> { [category.Id] = category },
            Customers: customer is null
                ? new Dictionary<Guid, CustomerEntity>()
                : new Dictionary<Guid, CustomerEntity> { [customer.Id] = customer },
            PromotionLevels: promotionLevel is null
                ? new Dictionary<Guid, PromotionLevelEntity>()
                : new Dictionary<Guid, PromotionLevelEntity> { [promotionLevel.Id] = promotionLevel },
            Tags: new Dictionary<Guid, TagEntity>()
        );

    #region CategoryName

    [Fact]
    public void CategoryName_WhenTheCategoryIsResolved_ShouldReturnItsName()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId, "Gossip", "gossip");

        // Act
        string name = Lookups(category: category).CategoryName(category.Id);

        // Assert
        name.Should().Be("Gossip");
    }

    [Fact]
    public void CategoryName_WhenTheCategoryIsMissing_ShouldReturnEmpty()
    {
        // Arrange
        ContentLookups lookups = Lookups();

        // Act
        string name = lookups.CategoryName(Guid.NewGuid());

        // Assert
        name.Should().BeEmpty();
    }

    #endregion

    #region CategorySlug

    [Fact]
    public void CategorySlug_WhenTheCategoryIsResolved_ShouldReturnItsSlug()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId, "Gossip", "gossip");

        // Act
        string slug = Lookups(category: category).CategorySlug(category.Id);

        // Assert
        slug.Should().Be("gossip");
    }

    [Fact]
    public void CategorySlug_WhenTheCategoryIsMissing_ShouldReturnEmpty()
    {
        // Arrange
        ContentLookups lookups = Lookups();

        // Act
        string slug = lookups.CategorySlug(Guid.NewGuid());

        // Assert
        slug.Should().BeEmpty();
    }

    #endregion

    #region CustomerName

    [Fact]
    public void CustomerName_WhenTheCustomerIsResolved_ShouldReturnTheFullName()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.Create();

        // Act
        string? name = Lookups(customer: customer).CustomerName(customer.Id);

        // Assert
        name.Should().Be(customer.FullName);
    }

    [Fact]
    public void CustomerName_WhenTheContentCarriesNoCustomer_ShouldReturnNull()
    {
        // Arrange
        ContentLookups lookups = Lookups();

        // Act
        string? name = lookups.CustomerName(null);

        // Assert
        name.Should().BeNull();
    }

    [Fact]
    public void CustomerName_WhenTheCustomerIsMissing_ShouldReturnNull()
    {
        // Arrange
        ContentLookups lookups = Lookups();

        // Act
        string? name = lookups.CustomerName(Guid.NewGuid());

        // Assert
        name.Should().BeNull();
    }

    #endregion

    #region PromotionLevelName

    [Fact]
    public void PromotionLevelName_WhenThePromotionIsResolved_ShouldReturnItsName()
    {
        // Arrange
        PromotionLevelEntity promotionLevel = PromotionLevelFactory.Create();

        // Act
        string? name = Lookups(promotionLevel: promotionLevel).PromotionLevelName(promotionLevel.Id);

        // Assert
        name.Should().Be(promotionLevel.Name);
    }

    [Fact]
    public void PromotionLevelName_WhenTheContentCarriesNoPromotion_ShouldReturnNull()
    {
        // Arrange
        ContentLookups lookups = Lookups();

        // Act
        string? name = lookups.PromotionLevelName(null);

        // Assert
        name.Should().BeNull();
    }

    [Fact]
    public void PromotionLevelName_WhenThePromotionIsMissing_ShouldReturnNull()
    {
        // Arrange
        ContentLookups lookups = Lookups();

        // Act
        string? name = lookups.PromotionLevelName(Guid.NewGuid());

        // Assert
        name.Should().BeNull();
    }

    #endregion
}
