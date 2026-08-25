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
    /// Builds an order whose Customer navigation is populated, mirroring the
    /// Include the search specifications rely on.
    /// </summary>
    private static ContentOrderEntity CreateOrderForCustomer(string fullName, string email, string? company)
    {
        CustomerEntity customer = new CustomerBuilder()
            .WithFullName(fullName)
            .WithEmail(email)
            .WithCompany(company)
            .Build();

        return new ContentOrderBuilder().WithCustomer(customer).Build();
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
        ContentOrderEntity order = CreateOrderForCustomer("Didi Mokonzi", "didi@acme.io", "Acme Corp");
        var spec = new ContentOrderSearchSpecification(search);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(order);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void SearchSpec_WithNullCompany_ShouldNotMatchCompanyTerm()
    {
        // Arrange
        ContentOrderEntity order = CreateOrderForCustomer("Didi Mokonzi", "didi@acme.io", company: null);
        var spec = new ContentOrderSearchSpecification("corp");

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(order);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ContentPaymentByOrderIdSpecification

    [Fact]
    public void ContentPaymentByOrderIdSpec_WhenOrderIdMatches_ShouldReturnTrue()
    {
        var orderId = Guid.NewGuid();
        ContentPaymentEntity payment = ContentPaymentFactory.Create(orderId);
        var spec = new ContentPaymentByOrderIdSpecification(orderId);

        bool result = spec.ToExpression().Compile()(payment);

        result.Should().BeTrue();
    }

    [Fact]
    public void ContentPaymentByOrderIdSpec_WhenOrderIdDoesNotMatch_ShouldReturnFalse()
    {
        ContentPaymentEntity payment = ContentPaymentFactory.Create(Guid.NewGuid());
        var spec = new ContentPaymentByOrderIdSpecification(Guid.NewGuid());

        bool result = spec.ToExpression().Compile()(payment);

        result.Should().BeFalse();
    }

    #endregion

    #region ContentOrderItemByIdAndOrderIdSpecification

    [Fact]
    public void ContentOrderItemByIdAndOrderIdSpec_WhenBothMatch_ShouldReturnTrue()
    {
        var orderId = Guid.NewGuid();
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(orderId, Guid.NewGuid());
        var spec = new ContentOrderItemByIdAndOrderIdSpecification(orderId, item.Id);

        bool result = spec.ToExpression().Compile()(item);

        result.Should().BeTrue();
    }

    [Fact]
    public void ContentOrderItemByIdAndOrderIdSpec_WhenItemIdDoesNotMatch_ShouldReturnFalse()
    {
        var orderId = Guid.NewGuid();
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(orderId, Guid.NewGuid());
        var spec = new ContentOrderItemByIdAndOrderIdSpecification(orderId, Guid.NewGuid());

        bool result = spec.ToExpression().Compile()(item);

        result.Should().BeFalse();
    }

    [Fact]
    public void ContentOrderItemByIdAndOrderIdSpec_WhenOrderIdDoesNotMatch_ShouldReturnFalse()
    {
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        var spec = new ContentOrderItemByIdAndOrderIdSpecification(Guid.NewGuid(), item.Id);

        bool result = spec.ToExpression().Compile()(item);

        result.Should().BeFalse();
    }

    #endregion

    #region ContentPaymentByStatusSpecification

    [Fact]
    public void PaymentByStatusSpec_WhenStatusMatches_ShouldReturnTrue()
    {
        ContentPaymentEntity payment = ContentPaymentFactory.Create(Guid.NewGuid());
        var spec = new ContentPaymentByStatusSpecification(EnumPaymentStatus.Pending);

        bool result = spec.ToExpression().Compile()(payment);

        result.Should().BeTrue();
    }

    [Fact]
    public void PaymentByStatusSpec_WhenStatusDoesNotMatch_ShouldReturnFalse()
    {
        ContentPaymentEntity payment = ContentPaymentFactory.Create(Guid.NewGuid());
        var spec = new ContentPaymentByStatusSpecification(EnumPaymentStatus.Verified);

        bool result = spec.ToExpression().Compile()(payment);

        result.Should().BeFalse();
    }

    #endregion

    #region ContentPaymentByMethodSpecification

    [Fact]
    public void PaymentByMethodSpec_WhenMethodMatches_ShouldReturnTrue()
    {
        ContentPaymentEntity payment = ContentPaymentFactory.CreateWithProof(Guid.NewGuid(), Guid.NewGuid());
        var spec = new ContentPaymentByMethodSpecification(EnumPaymentMethod.BankTransfer);

        bool result = spec.ToExpression().Compile()(payment);

        result.Should().BeTrue();
    }

    [Fact]
    public void PaymentByMethodSpec_WhenMethodDoesNotMatch_ShouldReturnFalse()
    {
        ContentPaymentEntity payment = ContentPaymentFactory.CreateWithProof(Guid.NewGuid(), Guid.NewGuid());
        var spec = new ContentPaymentByMethodSpecification(EnumPaymentMethod.Cash);

        bool result = spec.ToExpression().Compile()(payment);

        result.Should().BeFalse();
    }

    #endregion

    #region ContentPaymentSearchSpecification

    [Theory]
    [InlineData("mokonzi", true)]
    [InlineData("MOKONZI", true)]
    [InlineData("acme.io", true)]
    [InlineData("acme corp", true)]
    [InlineData("kinix", false)]
    public void PaymentSearchSpec_ShouldMatchOrderCustomerNameEmailOrCompanyCaseInsensitively(
        string search,
        bool expected
    )
    {
        // Arrange
        ContentOrderEntity order = CreateOrderForCustomer("Didi Mokonzi", "didi@acme.io", "Acme Corp");
        ContentPaymentEntity payment = new ContentPaymentBuilder().WithOrder(order).Build();
        var spec = new ContentPaymentSearchSpecification(search);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(payment);

        // Assert
        result.Should().Be(expected);
    }

    #endregion
}
