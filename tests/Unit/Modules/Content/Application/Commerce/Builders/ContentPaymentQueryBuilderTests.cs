using _116.Content.Application.Commerce.Builders;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.Builders;

/// <summary>
/// Unit tests for <see cref="ContentPaymentQueryBuilder"/>.
/// </summary>
public class ContentPaymentQueryBuilderTests
{
    #region WithStatus Tests

    [Fact]
    public void WithStatus_WhenNullStatus_ShouldReturnBuilderWithNoSpecification()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();

        // Act
        builder.WithStatus(null);
        Specification<ContentPaymentEntity>? spec = builder.Build();

        // Assert
        spec.Should().BeNull();
    }

    [Fact]
    public void WithStatus_WhenStatusProvided_ShouldReturnBuilderWithSpecification()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();

        // Act
        builder.WithStatus(EnumPaymentStatus.Pending);
        Specification<ContentPaymentEntity>? spec = builder.Build();

        // Assert
        spec.Should().NotBeNull();
    }

    [Fact]
    public void WithStatus_ShouldReturnSameBuilderInstance()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();

        // Act
        IContentPaymentQueryBuilder result = builder.WithStatus(null);

        // Assert
        result.Should().BeSameAs(builder);
    }

    #endregion

    #region WithMethod Tests

    [Fact]
    public void WithMethod_WhenNullMethod_ShouldReturnBuilderWithNoSpecification()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();

        // Act
        builder.WithMethod(null);
        Specification<ContentPaymentEntity>? spec = builder.Build();

        // Assert
        spec.Should().BeNull();
    }

    [Fact]
    public void WithMethod_WhenMethodProvided_ShouldReturnBuilderWithSpecification()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();

        // Act
        builder.WithMethod(EnumPaymentMethod.BankTransfer);
        Specification<ContentPaymentEntity>? spec = builder.Build();

        // Assert
        spec.Should().NotBeNull();
    }

    [Fact]
    public void WithMethod_ShouldReturnSameBuilderInstance()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();

        // Act
        IContentPaymentQueryBuilder result = builder.WithMethod(null);

        // Assert
        result.Should().BeSameAs(builder);
    }

    #endregion

    #region WithSearch Tests

    [Fact]
    public void WithSearch_WhenNullSearch_ShouldReturnBuilderWithNoSpecification()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();

        // Act
        builder.WithSearch(null);
        Specification<ContentPaymentEntity>? spec = builder.Build();

        // Assert
        spec.Should().BeNull();
    }

    [Fact]
    public void WithSearch_WhenEmptySearch_ShouldReturnBuilderWithNoSpecification()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();

        // Act
        builder.WithSearch("   ");
        Specification<ContentPaymentEntity>? spec = builder.Build();

        // Assert
        spec.Should().BeNull();
    }

    [Fact]
    public void WithSearch_WhenSearchProvided_ShouldReturnBuilderWithSpecification()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();

        // Act
        builder.WithSearch("acme");
        Specification<ContentPaymentEntity>? spec = builder.Build();

        // Assert
        spec.Should().NotBeNull();
    }

    [Fact]
    public void WithSearch_ShouldReturnSameBuilderInstance()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();

        // Act
        IContentPaymentQueryBuilder result = builder.WithSearch(null);

        // Assert
        result.Should().BeSameAs(builder);
    }

    #endregion

    #region CombineSpecification Tests

    [Fact]
    public void Build_WhenAllFiltersProvided_ShouldCombineAllSpecifications()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();

        // Act
        builder.WithStatus(EnumPaymentStatus.Pending);
        builder.WithMethod(EnumPaymentMethod.BankTransfer);
        builder.WithSearch("acme");
        Specification<ContentPaymentEntity>? spec = builder.Build();

        // Assert
        spec.Should().NotBeNull();
    }

    [Fact]
    public void Build_WhenNoFiltersProvided_ShouldReturnNull()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();

        // Act
        Specification<ContentPaymentEntity>? spec = builder.Build();

        // Assert
        spec.Should().BeNull();
    }

    #endregion
}
