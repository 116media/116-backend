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
/// Unit tests for <see cref="ContentPaymentQueryBuilder"/>.
/// </summary>
public class ContentPaymentQueryBuilderTests
{
    /// <summary>
    /// Builds a payment whose Order and Customer navigations are populated, mirroring
    /// the Includes the payment search specification relies on.
    /// </summary>
    private static ContentPaymentEntity CreatePaymentForCustomer(string fullName, string email, string company)
    {
        CustomerEntity customer = new CustomerBuilder()
            .WithFullName(fullName)
            .WithEmail(email)
            .WithCompany(company)
            .Build();
        ContentOrderEntity order = new ContentOrderBuilder().WithCustomer(customer).Build();

        return new ContentPaymentBuilder().WithOrder(order).Build();
    }

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
    public void WithStatus_WhenStatusProvided_ShouldMatchOnlyPaymentsInThatStatus()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();
        ContentPaymentEntity pendingPayment = ContentPaymentFactory.Create(Guid.NewGuid());
        ContentPaymentEntity verifiedPayment = ContentPaymentFactory.CreateVerified(Guid.NewGuid());

        // Act
        builder.WithStatus(EnumPaymentStatus.Pending);
        Specification<ContentPaymentEntity>? spec = builder.Build();

        // Assert
        spec.Should().NotBeNull();
        spec!.IsSatisfiedBy(pendingPayment).Should().BeTrue();
        spec.IsSatisfiedBy(verifiedPayment).Should().BeFalse();
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
    public void WithMethod_WhenMethodProvided_ShouldMatchOnlyPaymentsUsingThatMethod()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();
        ContentPaymentEntity bankTransferPayment = ContentPaymentFactory.CreateWithProof(
            Guid.NewGuid(),
            Guid.NewGuid()
        );
        ContentPaymentEntity cashPayment = new ContentPaymentBuilder()
            .WithProofFileId(Guid.NewGuid(), EnumPaymentMethod.Cash)
            .Build();

        // Act
        builder.WithMethod(EnumPaymentMethod.BankTransfer);
        Specification<ContentPaymentEntity>? spec = builder.Build();

        // Assert
        spec.Should().NotBeNull();
        spec!.IsSatisfiedBy(bankTransferPayment).Should().BeTrue();
        spec.IsSatisfiedBy(cashPayment).Should().BeFalse();
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
    public void WithSearch_WhenSearchProvided_ShouldMatchOrderCustomerFieldsCaseInsensitively()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();
        ContentPaymentEntity matchingPayment = CreatePaymentForCustomer("Grace Lombe", "grace@acme.io", "Acme Corp");
        ContentPaymentEntity otherPayment = CreatePaymentForCustomer("Didi Mokonzi", "didi@kinix.cd", "Kinix Media");

        // Act
        builder.WithSearch("acme");
        Specification<ContentPaymentEntity>? spec = builder.Build();

        // Assert
        spec.Should().NotBeNull();
        spec!.IsSatisfiedInMemoryBy(matchingPayment).Should().BeTrue();
        spec.IsSatisfiedInMemoryBy(otherPayment).Should().BeFalse();
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
    public void Build_WhenStatusAndMethodProvided_ShouldMatchOnlyPaymentsSatisfyingBoth()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();
        ContentPaymentEntity match = ContentPaymentFactory.CreateWithProof(Guid.NewGuid(), Guid.NewGuid());
        ContentPaymentEntity wrongMethod = new ContentPaymentBuilder()
            .WithProofFileId(Guid.NewGuid(), EnumPaymentMethod.Cash)
            .Build();
        ContentPaymentEntity wrongStatus = new ContentPaymentBuilder()
            .WithProofFileId(Guid.NewGuid(), EnumPaymentMethod.BankTransfer)
            .AsVerified(Guid.NewGuid(), "https://cdn.example/receipt.pdf")
            .Build();

        // Act
        builder.WithStatus(EnumPaymentStatus.Pending);
        builder.WithMethod(EnumPaymentMethod.BankTransfer);
        Specification<ContentPaymentEntity>? spec = builder.Build();

        // Assert
        spec.Should().NotBeNull();
        spec!.IsSatisfiedBy(match).Should().BeTrue();
        spec.IsSatisfiedBy(wrongMethod).Should().BeFalse();
        spec.IsSatisfiedBy(wrongStatus).Should().BeFalse();
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
