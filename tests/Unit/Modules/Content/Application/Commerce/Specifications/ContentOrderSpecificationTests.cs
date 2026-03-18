using _116.Content.Application.Commerce.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.Specifications;

/// <summary>
/// Unit tests for Commerce order specifications.
/// </summary>
public class ContentOrderSpecificationTests
{
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
}
