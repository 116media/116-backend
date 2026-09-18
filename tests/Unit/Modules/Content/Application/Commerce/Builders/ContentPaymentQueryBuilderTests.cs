using _116.Content.Application.Commerce.Builders;
using _116.Content.Application.Commerce.Builders.Contracts;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.Builders;

/// <summary>
/// Unit tests for <see cref="ContentPaymentQueryBuilder"/>, whose specifications select the
/// orders carrying a payment rather than payment rows.
/// </summary>
public class ContentPaymentQueryBuilderTests
{
    /// <summary>
    /// The customer rows the search filter probes; empty for the tests that do not search.
    /// </summary>
    private static readonly IQueryable<CustomerEntity> NoCustomers = Array.Empty<CustomerEntity>().AsQueryable();

    /// <summary>
    /// Builds an order whose payment is awaiting verification, paid by the given method.
    /// </summary>
    private static ContentOrderEntity CreateOrderWithPendingPayment(EnumPaymentMethod method)
    {
        ContentOrderEntity order = new ContentOrderBuilder().Build();
        ContentPaymentEntity payment = order.AttachPayment();
        payment.AttachProof(proofFileId: Guid.NewGuid(), paymentMethod: method);

        return order;
    }

    /// <summary>
    /// Builds an order whose payment has been verified.
    /// </summary>
    private static ContentOrderEntity CreateOrderWithVerifiedPayment()
    {
        ContentOrderEntity order = CreateOrderWithPendingPayment(EnumPaymentMethod.BankTransfer);
        order.Payment!.Verify(
            adminUserId: Guid.NewGuid(),
            receiptUrl: TestConstants.Commerce.ValidReceiptUrl,
            now: TestConstants.Clock.Instant
        );

        return order;
    }

    /// <summary>
    /// Builds a customer and the paid order they placed, so the search filter has both halves.
    /// </summary>
    private static (CustomerEntity Customer, ContentOrderEntity Order) CreateOrderForCustomer(
        string fullName,
        string email,
        string company
    )
    {
        CustomerEntity customer = new CustomerBuilder()
            .WithFullName(fullName)
            .WithEmail(email)
            .WithCompany(company)
            .Build();
        ContentOrderEntity order = new ContentOrderBuilder().WithCustomer(customer).Build();
        order.AttachPayment();

        return (customer, order);
    }

    #region WithStatus Tests

    [Fact]
    public void WithStatus_WhenNullStatus_ShouldReturnBuilderWithNoSpecification()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();

        // Act
        builder.WithStatus(null);
        Specification<ContentOrderEntity>? spec = builder.Build(NoCustomers);

        // Assert
        spec.Should().BeNull();
    }

    [Fact]
    public void WithStatus_WhenStatusProvided_ShouldMatchOnlyOrdersWhosePaymentIsInThatStatus()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();
        ContentOrderEntity pendingOrder = CreateOrderWithPendingPayment(EnumPaymentMethod.BankTransfer);
        ContentOrderEntity verifiedOrder = CreateOrderWithVerifiedPayment();

        // Act
        builder.WithStatus(EnumPaymentStatus.Pending);
        Specification<ContentOrderEntity>? spec = builder.Build(NoCustomers);

        // Assert
        spec.Should().NotBeNull();
        spec!.IsSatisfiedBy(pendingOrder).Should().BeTrue();
        spec.IsSatisfiedBy(verifiedOrder).Should().BeFalse();
    }

    [Fact]
    public void WithStatus_WhenOrderCarriesNoPayment_ShouldNotMatch()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();
        ContentOrderEntity orderWithoutPayment = new ContentOrderBuilder().Build();

        // Act
        builder.WithStatus(EnumPaymentStatus.Pending);
        Specification<ContentOrderEntity>? spec = builder.Build(NoCustomers);

        // Assert
        spec!.IsSatisfiedBy(orderWithoutPayment).Should().BeFalse();
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
        Specification<ContentOrderEntity>? spec = builder.Build(NoCustomers);

        // Assert
        spec.Should().BeNull();
    }

    [Fact]
    public void WithMethod_WhenMethodProvided_ShouldMatchOnlyOrdersWhosePaymentUsesThatMethod()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();
        ContentOrderEntity bankTransferOrder = CreateOrderWithPendingPayment(EnumPaymentMethod.BankTransfer);
        ContentOrderEntity cashOrder = CreateOrderWithPendingPayment(EnumPaymentMethod.Cash);

        // Act
        builder.WithMethod(EnumPaymentMethod.BankTransfer);
        Specification<ContentOrderEntity>? spec = builder.Build(NoCustomers);

        // Assert
        spec.Should().NotBeNull();
        spec!.IsSatisfiedBy(bankTransferOrder).Should().BeTrue();
        spec.IsSatisfiedBy(cashOrder).Should().BeFalse();
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
        Specification<ContentOrderEntity>? spec = builder.Build(NoCustomers);

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
        Specification<ContentOrderEntity>? spec = builder.Build(NoCustomers);

        // Assert
        spec.Should().BeNull();
    }

    [Fact]
    public void WithSearch_WhenSearchProvided_ShouldMatchOrderCustomerFieldsCaseInsensitively()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();
        (CustomerEntity matchingCustomer, ContentOrderEntity matchingOrder) = CreateOrderForCustomer(
            "Grace Lombe",
            "grace@acme.io",
            "Acme Corp"
        );
        (CustomerEntity otherCustomer, ContentOrderEntity otherOrder) = CreateOrderForCustomer(
            "Didi Mokonzi",
            "didi@kinix.cd",
            "Kinix Media"
        );

        // Act
        builder.WithSearch("acme");
        Specification<ContentOrderEntity>? spec = builder.Build(
            new[] { matchingCustomer, otherCustomer }.AsQueryable()
        );

        // Assert
        spec.Should().NotBeNull();
        spec!.IsSatisfiedInMemoryBy(matchingOrder).Should().BeTrue();
        spec.IsSatisfiedInMemoryBy(otherOrder).Should().BeFalse();
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
    public void Build_WhenStatusAndMethodProvided_ShouldMatchOnlyOrdersSatisfyingBoth()
    {
        // Arrange
        var builder = new ContentPaymentQueryBuilder();
        ContentOrderEntity match = CreateOrderWithPendingPayment(EnumPaymentMethod.BankTransfer);
        ContentOrderEntity wrongMethod = CreateOrderWithPendingPayment(EnumPaymentMethod.Cash);
        ContentOrderEntity wrongStatus = CreateOrderWithVerifiedPayment();

        // Act
        builder.WithStatus(EnumPaymentStatus.Pending);
        builder.WithMethod(EnumPaymentMethod.BankTransfer);
        Specification<ContentOrderEntity>? spec = builder.Build(NoCustomers);

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
        Specification<ContentOrderEntity>? spec = builder.Build(NoCustomers);

        // Assert
        spec.Should().BeNull();
    }

    #endregion
}
