using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.CancelOrder;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.RejectPayment;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveItemTier;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveOrderItem;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment;
using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetAllOrders;
using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetAllPayments;
using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetCustomerOrders;
using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderById;
using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderPayment;
using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetPendingPaymentOrders;
using _116.Shared.Application.Metadata;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.MetaFields;

/// <summary>
/// Tests that all Commerce MetaField static fields are correctly initialized.
/// Accessing each static readonly field triggers its initializer, ensuring full coverage.
/// </summary>
public class CommerceMetaFieldTests
{
    #region Command MetaFields

    [Fact]
    public void AdminCreateOrderMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminCreateOrderMetaField.AdminCreateOrder;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminAddOrderItemMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminAddOrderItemMetaField.AdminAddOrderItem;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminAddItemTierMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminAddItemTierMetaField.AdminAddItemTier;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminSubmitOrderMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminSubmitOrderMetaField.AdminSubmitOrder;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminCancelOrderMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminCancelOrderMetaField.AdminCancelOrder;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminAttachPaymentProofMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminAttachPaymentProofMetaField.AdminAttachPaymentProof;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminVerifyPaymentMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminVerifyPaymentMetaField.AdminVerifyPayment;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminRejectPaymentMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminRejectPaymentMetaField.AdminRejectPayment;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Query MetaFields

    [Fact]
    public void AdminGetAllOrdersMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetAllOrdersMetaField.AdminGetAllOrders;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminGetOrderByIdMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetOrderByIdMetaField.AdminGetOrderById;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminGetCustomerOrdersMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetCustomerOrdersMetaField.AdminGetCustomerOrders;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminGetOrderPaymentMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetOrderPaymentMetaField.AdminGetOrderPayment;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminGetPendingPaymentOrdersMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetPendingPaymentOrdersMetaField.AdminGetPendingPaymentOrders;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminGetAllPaymentsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetAllPaymentsMetaField.AdminGetAllPayments;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Editing MetaFields

    [Fact]
    public void AdminEditOrderMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminEditOrderMetaField.AdminEditOrder;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminRemoveOrderItemMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminRemoveOrderItemMetaField.AdminRemoveOrderItem;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminRemoveItemTierMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminRemoveItemTierMetaField.AdminRemoveItemTier;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminEditOrderItemMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminEditOrderItemMetaField.AdminEditOrderItem;
        metadata.Should().NotBeNull();
    }

    #endregion
}
