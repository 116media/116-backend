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
    /// <summary>
    /// Creates an order carrying the Customer navigation EF Core would populate, so the mapper
    /// can read Customer.FullName.
    /// </summary>
    private static ContentOrderEntity CreateOrderWithCustomer() =>
        new ContentOrderBuilder().WithCustomer(CustomerFactory.Create()).Build();

    #region ToContentOrderSummaryDto

    [Fact]
    public void ToContentOrderSummaryDto_ShouldMapCustomerName()
    {
        // Arrange
        ContentOrderEntity order = CreateOrderWithCustomer();

        // Act
        var dto = order.ToContentOrderSummaryDto(Mapper);

        // Assert
        dto.CustomerName.Should().Be(order.Customer.FullName);
    }

    [Fact]
    public void ToContentOrderSummaryDto_ShouldMapItemCount()
    {
        // Arrange
        ContentOrderEntity order = CreateOrderWithCustomer();

        // Act
        var dto = order.ToContentOrderSummaryDto(Mapper);

        // Assert
        dto.ItemCount.Should().Be(order.Items.Count);
    }

    [Fact]
    public void ToContentOrderSummaryDto_ShouldMapCoreFields()
    {
        // Arrange
        ContentOrderEntity order = CreateOrderWithCustomer();

        // Act
        var dto = order.ToContentOrderSummaryDto(Mapper);

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
        var dto = order.ToContentOrderDetailDto(Mapper);

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
        var dto = order.ToContentOrderDetailDto(Mapper);

        // Assert
        dto.CustomerName.Should().Be(order.Customer.FullName);
    }

    [Fact]
    public void ToContentOrderDetailDto_ShouldMapCustomerId()
    {
        // Arrange
        ContentOrderEntity order = CreateOrderWithCustomer();

        // Act
        var dto = order.ToContentOrderDetailDto(Mapper);

        // Assert
        dto.CustomerId.Should().Be(order.CustomerId);
    }

    [Fact]
    public void ToContentOrderDetailDto_ShouldMapPackageId()
    {
        // Arrange
        ContentOrderEntity order = CreateOrderWithCustomer();

        // Act
        var dto = order.ToContentOrderDetailDto(Mapper);

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
        IReadOnlyList<ContentOrderSummaryDto> dtos = orders.ToContentOrderSummaryDtos(Mapper);

        // Assert
        dtos.Should().HaveCount(2);
        dtos[0].CustomerName.Should().Be(orders[0].Customer.FullName);
        dtos[1].CustomerName.Should().Be(orders[1].Customer.FullName);
    }

    [Fact]
    public void ToContentOrderDetailDto_ShouldMapPayment_WhenPaymentExists()
    {
        // Arrange
        ContentPaymentEntity payment = ContentPaymentFactory.Create(Guid.NewGuid());
        ContentOrderEntity order = new ContentOrderBuilder()
            .WithCustomer(CustomerFactory.Create())
            .WithPayment(payment)
            .Build();

        // Act
        var dto = order.ToContentOrderDetailDto(Mapper);

        // Assert
        dto.Payment.Should().NotBeNull();
        dto.Payment!.AmountUsd.Should().Be(payment.AmountUsd);
    }

    #endregion
}
