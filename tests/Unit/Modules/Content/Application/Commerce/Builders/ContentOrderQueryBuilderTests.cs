using _116.Content.Application.Commerce.Builders;
using _116.Content.Application.Commerce.Builders.Contracts;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Helpers;
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
    public void WithStatus_WhenStatusProvided_ShouldMatchOnlyOrdersInThatStatus()
    {
        // Arrange
        var builder = new ContentOrderQueryBuilder();
        ContentOrderEntity draftOrder = ContentOrderFactory.Create();
        ContentOrderEntity paidOrder = ContentOrderFactory.CreatePaid();

        // Act
        builder.WithStatus(EnumOrderStatus.Draft);
        Specification<ContentOrderEntity>? spec = builder.Build();

        // Assert
        spec.Should().NotBeNull();
        spec!.IsSatisfiedBy(draftOrder).Should().BeTrue();
        spec.IsSatisfiedBy(paidOrder).Should().BeFalse();
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
    public void WithCustomerId_WhenCustomerIdProvided_ShouldMatchOnlyThatCustomersOrders()
    {
        // Arrange
        var builder = new ContentOrderQueryBuilder();
        var customerId = Guid.NewGuid();
        ContentOrderEntity ownOrder = ContentOrderFactory.CreateForCustomer(customerId);
        ContentOrderEntity otherCustomersOrder = ContentOrderFactory.CreateForCustomer(Guid.NewGuid());

        // Act
        builder.WithCustomerId(customerId);
        Specification<ContentOrderEntity>? spec = builder.Build();

        // Assert
        spec.Should().NotBeNull();
        spec!.IsSatisfiedBy(ownOrder).Should().BeTrue();
        spec.IsSatisfiedBy(otherCustomersOrder).Should().BeFalse();
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
    public void WithSearch_WhenSearchProvided_ShouldMatchCustomerFieldsCaseInsensitively()
    {
        // Arrange
        var builder = new ContentOrderQueryBuilder();
        CustomerEntity matchingCustomer = new CustomerBuilder()
            .WithFullName("Grace Lombe")
            .WithEmail("grace@acme.io")
            .WithCompany("Acme Corp")
            .Build();
        CustomerEntity otherCustomer = new CustomerBuilder()
            .WithFullName("Didi Mokonzi")
            .WithEmail("didi@kinix.cd")
            .WithCompany("Kinix Media")
            .Build();
        ContentOrderEntity matchingOrder = new ContentOrderBuilder().WithCustomer(matchingCustomer).Build();
        ContentOrderEntity otherOrder = new ContentOrderBuilder().WithCustomer(otherCustomer).Build();

        // Act
        builder.WithSearch("acme");
        Specification<ContentOrderEntity>? spec = builder.Build();

        // Assert
        spec.Should().NotBeNull();
        spec!.IsSatisfiedInMemoryBy(matchingOrder).Should().BeTrue();
        spec.IsSatisfiedInMemoryBy(otherOrder).Should().BeFalse();
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
    public void Build_WhenBothStatusAndCustomerIdProvided_ShouldMatchOnlyOrdersSatisfyingBoth()
    {
        // Arrange
        var builder = new ContentOrderQueryBuilder();
        var customerId = Guid.NewGuid();
        ContentOrderEntity match = new ContentOrderBuilder().WithCustomerId(customerId).Build();
        ContentOrderEntity wrongCustomer = ContentOrderFactory.CreateForCustomer(Guid.NewGuid());
        ContentOrderEntity paidOrder = new ContentOrderBuilder().WithCustomerId(customerId).AsPaid().Build();

        // Act
        builder.WithStatus(EnumOrderStatus.Draft);
        builder.WithCustomerId(customerId);
        Specification<ContentOrderEntity>? spec = builder.Build();

        // Assert
        spec.Should().NotBeNull();
        spec!.IsSatisfiedBy(match).Should().BeTrue();
        spec.IsSatisfiedBy(wrongCustomer).Should().BeFalse();
        spec.IsSatisfiedBy(paidOrder).Should().BeFalse();
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
