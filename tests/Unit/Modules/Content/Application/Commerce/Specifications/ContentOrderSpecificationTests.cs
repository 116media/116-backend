using _116.Content.Application.Commerce.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.Specifications;

/// <summary>
/// Unit tests for Commerce order specifications.
/// Specifications using EF.Functions.ILike are evaluated through
/// <see cref="ILikeSpecificationEvaluator" />, which rewrites ILike for in-memory execution.
/// </summary>
public class ContentOrderSpecificationTests
{
    /// <summary>
    /// Builds a customer and the order they placed, with the customer exposed as the query
    /// source the search specification probes.
    /// </summary>
    private static (ContentOrderEntity Order, IQueryable<CustomerEntity> Customers) CreateOrderForCustomer(
        string fullName,
        string email,
        string? company
    )
    {
        CustomerEntity customer = new CustomerBuilder()
            .WithFullName(fullName)
            .WithEmail(email)
            .WithCompany(company)
            .Build();

        return (new ContentOrderBuilder().WithCustomer(customer).Build(), new[] { customer }.AsQueryable());
    }

    #region ContentOrderByIdSpecification

    [Fact]
    public void ByIdSpec_WhenIdMatches_ShouldReturnTrue()
    {
        var id = Guid.NewGuid();
        ContentOrderEntity order = ContentOrderFactory.CreateWithId(id);
        var spec = new ContentOrderByIdSpecification(id);

        bool result = spec.ToExpression().Compile()(order);

        result.Should().BeTrue();
    }

    [Fact]
    public void ByIdSpec_WhenIdDoesNotMatch_ShouldReturnFalse()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateWithId(Guid.NewGuid());
        var spec = new ContentOrderByIdSpecification(Guid.NewGuid());

        bool result = spec.ToExpression().Compile()(order);

        result.Should().BeFalse();
    }

    #endregion

    #region ContentOrderByStatusSpecification

    [Fact]
    public void ByStatusSpec_WhenStatusMatches_ShouldReturnTrue()
    {
        ContentOrderEntity order = ContentOrderFactory.Create();
        var spec = new ContentOrderByStatusSpecification(EnumOrderStatus.Draft);

        bool result = spec.ToExpression().Compile()(order);

        result.Should().BeTrue();
    }

    [Fact]
    public void ByStatusSpec_WhenStatusDoesNotMatch_ShouldReturnFalse()
    {
        ContentOrderEntity order = ContentOrderFactory.Create();
        var spec = new ContentOrderByStatusSpecification(EnumOrderStatus.Paid);

        bool result = spec.ToExpression().Compile()(order);

        result.Should().BeFalse();
    }

    #endregion

    #region ContentOrderByCustomerIdSpecification

    [Fact]
    public void ByCustomerIdSpec_WhenCustomerIdMatches_ShouldReturnTrue()
    {
        var customerId = Guid.NewGuid();
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customerId);
        var spec = new ContentOrderByCustomerIdSpecification(customerId);

        bool result = spec.ToExpression().Compile()(order);

        result.Should().BeTrue();
    }

    [Fact]
    public void ByCustomerIdSpec_WhenCustomerIdDoesNotMatch_ShouldReturnFalse()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(Guid.NewGuid());
        var spec = new ContentOrderByCustomerIdSpecification(Guid.NewGuid());

        bool result = spec.ToExpression().Compile()(order);

        result.Should().BeFalse();
    }

    #endregion

    #region ContentOrderSearchSpecification

    [Theory]
    [InlineData("mokonzi", true)]
    [InlineData("MOKONZI", true)]
    [InlineData("acme.io", true)]
    [InlineData("acme corp", true)]
    [InlineData("kinix", false)]
    public void SearchSpec_ShouldMatchCustomerNameEmailOrCompanyCaseInsensitively(string search, bool expected)
    {
        // Arrange
        (ContentOrderEntity order, IQueryable<CustomerEntity> customers) = CreateOrderForCustomer(
            "Didi Mokonzi",
            "didi@acme.io",
            "Acme Corp"
        );
        var spec = new ContentOrderSearchSpecification(search: search, customers: customers);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(order);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void SearchSpec_WithNullCompany_ShouldNotMatchCompanyTerm()
    {
        // Arrange
        (ContentOrderEntity order, IQueryable<CustomerEntity> customers) = CreateOrderForCustomer(
            "Didi Mokonzi",
            "didi@acme.io",
            company: null
        );
        var spec = new ContentOrderSearchSpecification(search: "corp", customers: customers);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(order);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SearchSpec_WhenTheMatchingCustomerPlacedNoOrder_ShouldReturnFalse()
    {
        // Arrange
        (_, IQueryable<CustomerEntity> customers) = CreateOrderForCustomer("Didi Mokonzi", "didi@acme.io", "Acme Corp");
        ContentOrderEntity otherOrder = ContentOrderFactory.CreateForCustomer(Guid.NewGuid());
        var spec = new ContentOrderSearchSpecification(search: "mokonzi", customers: customers);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(otherOrder);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region OrderHasPaymentSpecification

    [Fact]
    public void OrderHasPaymentSpec_WhenOrderCarriesAPayment_ShouldReturnTrue()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateWithId(Guid.NewGuid());
        order.AttachPayment();
        var spec = new OrderHasPaymentSpecification();

        bool result = spec.IsSatisfiedBy(order);

        result.Should().BeTrue();
    }

    [Fact]
    public void OrderHasPaymentSpec_WhenOrderCarriesNoPayment_ShouldReturnFalse()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateWithId(Guid.NewGuid());
        var spec = new OrderHasPaymentSpecification();

        bool result = spec.IsSatisfiedBy(order);

        result.Should().BeFalse();
    }

    #endregion

    #region OrderPaymentByStatusSpecification

    [Fact]
    public void OrderPaymentByStatusSpec_WhenStatusMatches_ShouldReturnTrue()
    {
        ContentOrderEntity order = CreateOrderWithPayment(EnumPaymentMethod.BankTransfer);
        var spec = new OrderPaymentByStatusSpecification(EnumPaymentStatus.Pending);

        bool result = spec.IsSatisfiedBy(order);

        result.Should().BeTrue();
    }

    [Fact]
    public void OrderPaymentByStatusSpec_WhenStatusDoesNotMatch_ShouldReturnFalse()
    {
        ContentOrderEntity order = CreateOrderWithPayment(EnumPaymentMethod.BankTransfer);
        var spec = new OrderPaymentByStatusSpecification(EnumPaymentStatus.Verified);

        bool result = spec.IsSatisfiedBy(order);

        result.Should().BeFalse();
    }

    [Fact]
    public void OrderPaymentByStatusSpec_WhenOrderCarriesNoPayment_ShouldReturnFalse()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateWithId(Guid.NewGuid());
        var spec = new OrderPaymentByStatusSpecification(EnumPaymentStatus.Pending);

        bool result = spec.IsSatisfiedBy(order);

        result.Should().BeFalse();
    }

    #endregion

    #region OrderPaymentByMethodSpecification

    [Fact]
    public void OrderPaymentByMethodSpec_WhenMethodMatches_ShouldReturnTrue()
    {
        ContentOrderEntity order = CreateOrderWithPayment(EnumPaymentMethod.BankTransfer);
        var spec = new OrderPaymentByMethodSpecification(EnumPaymentMethod.BankTransfer);

        bool result = spec.IsSatisfiedBy(order);

        result.Should().BeTrue();
    }

    [Fact]
    public void OrderPaymentByMethodSpec_WhenMethodDoesNotMatch_ShouldReturnFalse()
    {
        ContentOrderEntity order = CreateOrderWithPayment(EnumPaymentMethod.BankTransfer);
        var spec = new OrderPaymentByMethodSpecification(EnumPaymentMethod.Cash);

        bool result = spec.IsSatisfiedBy(order);

        result.Should().BeFalse();
    }

    [Fact]
    public void OrderPaymentByMethodSpec_WhenOrderCarriesNoPayment_ShouldReturnFalse()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateWithId(Guid.NewGuid());
        var spec = new OrderPaymentByMethodSpecification(EnumPaymentMethod.BankTransfer);

        bool result = spec.IsSatisfiedBy(order);

        result.Should().BeFalse();
    }

    #endregion

    #region ContentOrderByItemIdSpecification

    [Fact]
    public void ContentOrderByItemIdSpecification_WithItemInOrder_ShouldReturnTrue()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateWithId(Guid.NewGuid());
        ContentOrderItemEntity item = CreateItem(order.Id);
        order.AddItem(item);
        var spec = new ContentOrderByItemIdSpecification(orderItemId: item.Id);

        // Act
        bool result = spec.IsSatisfiedBy(order);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ContentOrderByItemIdSpecification_WithItemFromAnotherOrder_ShouldReturnFalse()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateWithId(Guid.NewGuid());
        order.AddItem(CreateItem(order.Id));
        var spec = new ContentOrderByItemIdSpecification(orderItemId: Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(order);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ContentOrderByItemIdSpecification_WithNoItems_ShouldReturnFalse()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateWithId(Guid.NewGuid());
        var spec = new ContentOrderByItemIdSpecification(orderItemId: Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(order);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    private static ContentOrderEntity CreateOrderWithPayment(EnumPaymentMethod method)
    {
        ContentOrderEntity order = ContentOrderFactory.CreateWithId(Guid.NewGuid());
        ContentPaymentEntity payment = order.AttachPayment();
        payment.AttachProof(proofFileId: Guid.NewGuid(), paymentMethod: method);

        return order;
    }

    private static ContentOrderItemEntity CreateItem(Guid orderId) =>
        ContentOrderItemEntity.Create(
            id: Guid.NewGuid(),
            orderId: orderId,
            contentKind: EnumCoreContentType.Article,
            categoryId: Guid.NewGuid(),
            promotionLevelId: null,
            promoPriceSnapshotUsd: null,
            socialBoost: false,
            isBonus: false
        );
}
