using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Mappers;

/// <summary>
/// Unit tests for <see cref="ContentOrderMapper"/> extension methods.
/// </summary>
public class ContentOrderMapperTests : BaseContentHandlerTest
{
    private readonly CustomerEntity _customer = CustomerFactory.Create();

    /// <summary>
    /// Creates an order placed by the shared customer, whose row the mapper reads from lookups.
    /// </summary>
    private ContentOrderEntity CreateOrderWithCustomer() => new ContentOrderBuilder().WithCustomer(_customer).Build();

    /// <summary>
    /// The resolved customer rows, keyed by id, as the DTO factory hands them to the mapper.
    /// </summary>
    private IReadOnlyDictionary<Guid, CustomerEntity> Customers =>
        new Dictionary<Guid, CustomerEntity> { [_customer.Id] = _customer };

    /// <summary>
    /// The lookups an order detail projection reads; only customers are populated here.
    /// </summary>
    private OrderLookups Lookups =>
        new(
            Customers: Customers,
            Categories: new Dictionary<Guid, CategoryEntity>(),
            PromotionLevels: new Dictionary<Guid, PromotionLevelEntity>(),
            PricingTiers: new Dictionary<Guid, PricingTierEntity>()
        );

    #region ToContentOrderSummaryDto

    [Fact]
    public void ToContentOrderSummaryDto_ShouldMapCustomerName()
    {
        // Arrange
        ContentOrderEntity order = CreateOrderWithCustomer();

        // Act
        var dto = order.ToContentOrderSummaryDto(Mapper, Customers);

        // Assert
        dto.CustomerName.Should().Be(_customer.FullName);
    }

    [Fact]
    public void ToContentOrderSummaryDto_ShouldMapItemCount()
    {
        // Arrange
        ContentOrderEntity order = CreateOrderWithCustomer();

        // Act
        var dto = order.ToContentOrderSummaryDto(Mapper, Customers);

        // Assert
        dto.ItemCount.Should().Be(order.Items.Count);
    }

    [Fact]
    public void ToContentOrderSummaryDto_ShouldMapCoreFields()
    {
        // Arrange
        ContentOrderEntity order = CreateOrderWithCustomer();

        // Act
        var dto = order.ToContentOrderSummaryDto(Mapper, Customers);

        // Assert
        dto.Id.Should().Be(order.Id);
        dto.Status.Should().Be(order.Status);
        dto.TotalAmountUsd.Should().Be(order.TotalAmountUsd);
    }

    #endregion

    #region ToContentOrderDetailDto

    [Fact]
    public void ToContentOrderDetailDto_ShouldMapEmptyItemsAndNullPayment()
    {
        // Arrange
        ContentOrderEntity order = CreateOrderWithCustomer();

        // Act
        var dto = order.ToContentOrderDetailDto(Mapper, Lookups);

        // Assert
        dto.Id.Should().Be(order.Id);
        dto.Items.Should().BeEmpty();
        dto.Payment.Should().BeNull();
    }

    [Fact]
    public void ToContentOrderDetailDto_ShouldMapCustomerName()
    {
        // Arrange
        ContentOrderEntity order = CreateOrderWithCustomer();

        // Act
        var dto = order.ToContentOrderDetailDto(Mapper, Lookups);

        // Assert
        dto.CustomerName.Should().Be(_customer.FullName);
    }

    [Fact]
    public void ToContentOrderDetailDto_ShouldMapCustomerId()
    {
        // Arrange
        ContentOrderEntity order = CreateOrderWithCustomer();

        // Act
        var dto = order.ToContentOrderDetailDto(Mapper, Lookups);

        // Assert
        dto.CustomerId.Should().Be(order.CustomerId);
    }

    [Fact]
    public void ToContentOrderDetailDto_ShouldMapPackageId()
    {
        // Arrange
        ContentOrderEntity order = CreateOrderWithCustomer();

        // Act
        var dto = order.ToContentOrderDetailDto(Mapper, Lookups);

        // Assert
        dto.PackageId.Should().Be(order.PackageId);
    }

    #endregion

    #region ToContentOrderSummaryDtos

    [Fact]
    public void ToContentOrderSummaryDtos_ShouldMapEachOrder()
    {
        // Arrange
        List<ContentOrderEntity> orders = [CreateOrderWithCustomer(), CreateOrderWithCustomer()];

        // Act
        IReadOnlyList<ContentOrderSummaryDto> dtos = orders.ToContentOrderSummaryDtos(Mapper, Customers);

        // Assert
        dtos.Should().HaveCount(2);
        dtos[0].CustomerName.Should().Be(_customer.FullName);
        dtos[1].CustomerName.Should().Be(_customer.FullName);
    }

    [Fact]
    public void ToContentOrderDetailDto_ShouldMapPayment_WhenPaymentExists()
    {
        // Arrange
        ContentOrderEntity order = CreateOrderWithCustomer();
        ContentPaymentEntity payment = order.AttachPayment();

        // Act
        var dto = order.ToContentOrderDetailDto(Mapper, Lookups);

        // Assert
        dto.Payment.Should().NotBeNull();
        dto.Payment!.AmountUsd.Should().Be(payment.AmountUsd);
    }

    #endregion
}
