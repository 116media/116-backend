using _116.Content.Application.Commerce.Builders;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.Builders;

/// <summary>
/// Unit tests for <see cref="ContentOrderQueryBuilder"/>.
/// </summary>
public class ContentOrderQueryBuilderTests
{
    #region WithStatus Tests

    [Fact]
    public void WithStatus_WhenNullStatus_ShouldReturnBuilderWithNoSpecification()
    {
        // Arrange
        var builder = new ContentOrderQueryBuilder();

        // Act
        builder.WithStatus(null);
        Specification<ContentOrderEntity>? spec = builder.Build();

        // Assert
        spec.Should().BeNull();
    }

    [Fact]
    public void WithStatus_WhenStatusProvided_ShouldReturnBuilderWithSpecification()
    {
        // Arrange
        var builder = new ContentOrderQueryBuilder();

        // Act
        builder.WithStatus(EnumOrderStatus.Draft);
        Specification<ContentOrderEntity>? spec = builder.Build();

        // Assert
        spec.Should().NotBeNull();
    }

    [Fact]
    public void WithStatus_ShouldReturnSameBuilderInstance()
    {
        // Arrange
        var builder = new ContentOrderQueryBuilder();

        // Act
        IContentOrderQueryBuilder result = builder.WithStatus(null);

        // Assert
        result.Should().BeSameAs(builder);
    }

    #endregion

    #region WithCustomerId Tests

    [Fact]
    public void WithCustomerId_WhenNullCustomerId_ShouldReturnBuilderWithNoSpecification()
    {
        // Arrange
        var builder = new ContentOrderQueryBuilder();

        // Act
        builder.WithCustomerId(null);
        Specification<ContentOrderEntity>? spec = builder.Build();

        // Assert
        spec.Should().BeNull();
    }

    [Fact]
    public void WithCustomerId_WhenCustomerIdProvided_ShouldReturnBuilderWithSpecification()
    {
        // Arrange
        var builder = new ContentOrderQueryBuilder();

        // Act
        builder.WithCustomerId(Guid.NewGuid());
        Specification<ContentOrderEntity>? spec = builder.Build();

        // Assert
        spec.Should().NotBeNull();
    }

    [Fact]
    public void WithCustomerId_ShouldReturnSameBuilderInstance()
    {
        // Arrange
        var builder = new ContentOrderQueryBuilder();

        // Act
        IContentOrderQueryBuilder result = builder.WithCustomerId(null);

        // Assert
        result.Should().BeSameAs(builder);
    }

    #endregion

    #region WithSearch Tests

    [Fact]
    public void WithSearch_WhenNullSearch_ShouldReturnBuilderWithNoSpecification()
    {
        // Arrange
        var builder = new ContentOrderQueryBuilder();

        // Act
        builder.WithSearch(null);
        Specification<ContentOrderEntity>? spec = builder.Build();

        // Assert
        spec.Should().BeNull();
    }

    [Fact]
    public void WithSearch_WhenEmptySearch_ShouldReturnBuilderWithNoSpecification()
    {
        // Arrange
        var builder = new ContentOrderQueryBuilder();

        // Act
        builder.WithSearch("   ");
        Specification<ContentOrderEntity>? spec = builder.Build();

        // Assert
        spec.Should().BeNull();
    }

    [Fact]
    public void WithSearch_WhenSearchProvided_ShouldReturnBuilderWithSpecification()
    {
        // Arrange
        var builder = new ContentOrderQueryBuilder();

        // Act
        builder.WithSearch("acme");
        Specification<ContentOrderEntity>? spec = builder.Build();

        // Assert
        spec.Should().NotBeNull();
    }

    [Fact]
    public void WithSearch_ShouldReturnSameBuilderInstance()
    {
        // Arrange
        var builder = new ContentOrderQueryBuilder();

        // Act
        IContentOrderQueryBuilder result = builder.WithSearch(null);

        // Assert
        result.Should().BeSameAs(builder);
    }

    #endregion

    #region CombineSpecification Tests

    [Fact]
    public void Build_WhenBothStatusAndCustomerIdProvided_ShouldCombineSpecifications()
    {
        // Arrange — triggers CombineSpecification's And() branch (_specification is not null)
        var builder = new ContentOrderQueryBuilder();

        // Act
        builder.WithStatus(EnumOrderStatus.Draft);
        builder.WithCustomerId(Guid.NewGuid());
        Specification<ContentOrderEntity>? spec = builder.Build();

        // Assert
        spec.Should().NotBeNull();
    }

    [Fact]
    public void Build_WhenAllFiltersProvided_ShouldCombineAllSpecifications()
    {
        // Arrange
        var builder = new ContentOrderQueryBuilder();

        // Act
        builder.WithStatus(EnumOrderStatus.Draft);
        builder.WithCustomerId(Guid.NewGuid());
        builder.WithSearch("acme");
        Specification<ContentOrderEntity>? spec = builder.Build();

        // Assert
        spec.Should().NotBeNull();
    }

    [Fact]
    public void Build_WhenNoFiltersProvided_ShouldReturnNull()
    {
        // Arrange
        var builder = new ContentOrderQueryBuilder();

        // Act
        Specification<ContentOrderEntity>? spec = builder.Build();

        // Assert
        spec.Should().BeNull();
    }

    #endregion
}
