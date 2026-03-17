using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain;

/// <summary>
/// Unit tests for <see cref="ContentOrderEntity"/> domain behaviour.
/// </summary>
public class ContentOrderEntityTests
{
    #region Create

    [Fact]
    public void Create_ShouldSetId_CustomerId_StatusDraft_TotalZero()
    {
        Guid id = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();

        ContentOrderEntity order = ContentOrderEntity.Create(id, customerId, null);

        order.Id.Should().Be(id);
        order.CustomerId.Should().Be(customerId);
        order.PackageId.Should().BeNull();
        order.Status.Should().Be(EnumOrderStatus.Draft);
        order.TotalAmountUsd.Should().Be(0m);
    }

    [Fact]
    public void Create_WithPackageId_ShouldSetPackageId()
    {
        Guid packageId = Guid.NewGuid();

        ContentOrderEntity order = ContentOrderEntity.Create(Guid.NewGuid(), Guid.NewGuid(), packageId);

        order.PackageId.Should().Be(packageId);
    }

    #endregion

    #region EnsureDraft

    [Fact]
    public void EnsureDraft_WhenDraft_ShouldNotThrow()
    {
        ContentOrderEntity order = ContentOrderFactory.Create();

        Action act = () => order.EnsureDraft();

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureDraft_WhenSubmitted_ShouldThrowBadRequestException()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();

        Action act = () => order.EnsureDraft();

        act.Should().Throw<BadRequestException>();
    }

    [Fact]
    public void EnsureDraft_WhenPaid_ShouldThrowBadRequestException()
    {
        ContentOrderEntity order = ContentOrderFactory.CreatePaid();

        Action act = () => order.EnsureDraft();

        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region RecalculateTotal

    [Fact]
    public void RecalculateTotal_ShouldSumTierPricesAndPromoPrice()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid categoryId = Guid.NewGuid();
        ContentOrderItemEntity item = ContentOrderItemFactory.CreateWithPromo(
            order.Id,
            categoryId,
            Guid.NewGuid(),
            50m
        );
        ContentItemTierEntity tier = ContentItemTierFactory.Create(item.Id, Guid.NewGuid(), 100m);
        item.Tiers.Add(tier);
        order.Items.Add(item);

        // Act — no new tier being added in this call, so newTierPrice = 0
        order.RecalculateTotal(newTierPrice: 0m);

        // Assert: 100 (tier) + 0 (new) + 50 (promo) = 150
        order.TotalAmountUsd.Should().Be(150m);
    }

    #endregion

    #region Submit

    [Fact]
    public void Submit_WhenDraft_ShouldTransitionToPendingPayment()
    {
        ContentOrderEntity order = ContentOrderFactory.Create();

        order.Submit();

        order.Status.Should().Be(EnumOrderStatus.PendingPayment);
    }

    [Fact]
    public void Submit_WhenNotDraft_ShouldThrowConflictException()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();

        Action act = () => order.Submit();

        act.Should().Throw<ConflictException>();
    }

    #endregion

    #region MarkPaid

    [Fact]
    public void MarkPaid_WhenPendingPayment_ShouldTransitionToPaid()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();

        order.MarkPaid();

        order.Status.Should().Be(EnumOrderStatus.Paid);
    }

    [Fact]
    public void MarkPaid_WhenNotPendingPayment_ShouldThrowConflictException()
    {
        ContentOrderEntity order = ContentOrderFactory.Create();

        Action act = () => order.MarkPaid();

        act.Should().Throw<ConflictException>();
    }

    #endregion

    #region Cancel

    [Fact]
    public void Cancel_WhenDraft_ShouldTransitionToCancelled()
    {
        ContentOrderEntity order = ContentOrderFactory.Create();

        order.Cancel();

        order.Status.Should().Be(EnumOrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenPaid_ShouldThrowBadRequestException()
    {
        ContentOrderEntity order = ContentOrderFactory.CreatePaid();

        Action act = () => order.Cancel();

        act.Should().Throw<BadRequestException>();
    }

    [Fact]
    public void Cancel_WhenPendingPayment_ShouldTransitionToCancelled()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();

        order.Cancel();

        order.Status.Should().Be(EnumOrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenCancelled_ShouldThrowConflictException()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateCancelled();

        Action act = () => order.Cancel();

        act.Should().Throw<ConflictException>();
    }

    #endregion
}
