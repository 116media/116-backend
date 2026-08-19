using _116.Content.Application.Shared.Errors;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="ContentOrderEntity"/> domain behaviour.
/// </summary>
public class ContentOrderEntityTests
{
    private readonly ContentOrderErrors _errors = TestErrorsFactory.CreateContentOrderErrors();

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

        Action act = () => order.EnsureDraft(_errors);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureDraft_WhenSubmitted_ShouldThrowBadRequestException()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();

        Action act = () => order.EnsureDraft(_errors);

        act.Should().Throw<BadRequestException>();
    }

    [Fact]
    public void EnsureDraft_WhenPaid_ShouldThrowBadRequestException()
    {
        ContentOrderEntity order = ContentOrderFactory.CreatePaid();

        Action act = () => order.EnsureDraft(_errors);

        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region RecalculateTotalFromItems (inline)

    [Fact]
    public void RecalculateTotalFromItems_Inline_ShouldSumTierPricesAndPromoPrice()
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

        // Act
        order.RecalculateTotalFromItems();

        // Assert: 100 (tier) + 50 (promo) = 150
        order.TotalAmountUsd.Should().Be(150m);
    }

    #endregion

    #region Submit

    [Fact]
    public void Submit_WhenDraft_ShouldTransitionToPendingPayment()
    {
        ContentOrderEntity order = ContentOrderFactory.Create();

        order.Submit(_errors);

        order.Status.Should().Be(EnumOrderStatus.PendingPayment);
    }

    [Fact]
    public void Submit_WhenDraft_ShouldRaiseOrderSubmittedEvent()
    {
        ContentOrderEntity order = ContentOrderFactory.Create();

        order.Submit(_errors);

        order
            .DomainEvents.OfType<OrderSubmittedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new OrderSubmittedEvent(order.Id));
    }

    [Fact]
    public void Submit_WhenNotDraft_ShouldThrowConflictException()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        order.ClearDomainEvents();

        Action act = () => order.Submit(_errors);

        act.Should().Throw<ConflictException>();
        order.DomainEvents.Should().BeEmpty();
    }

    #endregion

    #region MarkPaid

    [Fact]
    public void MarkPaid_WhenPendingPayment_ShouldTransitionToPaid()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();

        order.MarkPaid(
            paymentId: Guid.NewGuid(),
            verifiedAt: DateTimeOffset.UtcNow,
            promotionDurationsByLevelId: new Dictionary<Guid, int>(),
            errors: _errors
        );

        order.Status.Should().Be(EnumOrderStatus.Paid);
    }

    [Fact]
    public void MarkPaid_WhenPendingPayment_ShouldRaiseOrderPaidEventWithOneEffectPerItem()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        order.ClearDomainEvents();
        Guid paymentId = Guid.NewGuid();
        ContentOrderItemEntity plainItem = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());
        ContentOrderItemEntity boostedItem = ContentOrderItemFactory.CreateSocialBoost(order.Id, Guid.NewGuid());
        order.Items.Add(plainItem);
        order.Items.Add(boostedItem);

        order.MarkPaid(
            paymentId: paymentId,
            verifiedAt: DateTimeOffset.UtcNow,
            promotionDurationsByLevelId: new Dictionary<Guid, int>(),
            errors: _errors
        );

        OrderPaidEvent paidEvent = order.DomainEvents.OfType<OrderPaidEvent>().Should().ContainSingle().Which;
        paidEvent.OrderId.Should().Be(order.Id);
        paidEvent.PaymentId.Should().Be(paymentId);
        paidEvent
            .Items.Should()
            .BeEquivalentTo(
                new[]
                {
                    new PaidItemEffect(plainItem.Id, null, null, SocialBoost: false),
                    new PaidItemEffect(boostedItem.Id, null, null, SocialBoost: true),
                }
            );
    }

    [Fact]
    public void MarkPaid_WithPromotedItem_ShouldComputePromotionWindowFromDurationAtRaiseTime()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        order.ClearDomainEvents();
        Guid promotionLevelId = Guid.NewGuid();
        const int durationDays = 14;
        ContentOrderItemEntity item = ContentOrderItemFactory.CreateWithPromo(
            order.Id,
            Guid.NewGuid(),
            promotionLevelId,
            50m
        );
        order.Items.Add(item);
        DateTimeOffset verifiedAt = new(2026, 6, 30, 10, 15, 42, 123, TimeSpan.Zero);

        order.MarkPaid(
            paymentId: Guid.NewGuid(),
            verifiedAt: verifiedAt,
            promotionDurationsByLevelId: new Dictionary<Guid, int> { [promotionLevelId] = durationDays },
            errors: _errors
        );

        OrderPaidEvent paidEvent = order.DomainEvents.OfType<OrderPaidEvent>().Should().ContainSingle().Which;
        PaidItemEffect effect = paidEvent.Items.Should().ContainSingle().Which;
        effect.OrderItemId.Should().Be(item.Id);
        effect.PromotionLevelId.Should().Be(promotionLevelId);
        effect.PromotionUntil.Should().Be(verifiedAt.AddDays(durationDays));
    }

    [Fact]
    public void MarkPaid_ShouldTruncateThePaidInstantAndEveryWindowToWholeMilliseconds()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        order.ClearDomainEvents();
        Guid promotionLevelId = Guid.NewGuid();
        const int durationDays = 30;
        ContentOrderItemEntity item = ContentOrderItemFactory.CreateWithPromo(
            order.Id,
            Guid.NewGuid(),
            promotionLevelId,
            50m
        );
        order.Items.Add(item);
        DateTimeOffset verifiedAt = new DateTimeOffset(2026, 6, 30, 10, 15, 42, 123, TimeSpan.Zero).AddTicks(4567);
        DateTimeOffset expectedPaidAt = new(2026, 6, 30, 10, 15, 42, 123, TimeSpan.Zero);

        order.MarkPaid(
            paymentId: Guid.NewGuid(),
            verifiedAt: verifiedAt,
            promotionDurationsByLevelId: new Dictionary<Guid, int> { [promotionLevelId] = durationDays },
            errors: _errors
        );

        // The persisted timestamptz keeps microseconds, so a millisecond-aligned
        // value round-trips unchanged and stays comparable to the payload.
        OrderPaidEvent paidEvent = order.DomainEvents.OfType<OrderPaidEvent>().Should().ContainSingle().Which;
        paidEvent.PaidAt.Should().Be(expectedPaidAt);
        (paidEvent.PaidAt.Ticks % TimeSpan.TicksPerMillisecond).Should().Be(0);
        PaidItemEffect effect = paidEvent.Items.Should().ContainSingle().Which;
        effect.PromotionUntil.Should().Be(expectedPaidAt.AddDays(durationDays));
        (effect.PromotionUntil!.Value.Ticks % TimeSpan.TicksPerMillisecond).Should().Be(0);
    }

    [Fact]
    public void MarkPaid_WhenPromotionDurationMissing_ShouldThrowBadRequestException()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        order.ClearDomainEvents();
        ContentOrderItemEntity item = ContentOrderItemFactory.CreateWithPromo(
            order.Id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            50m
        );
        order.Items.Add(item);

        Action act = () =>
            order.MarkPaid(
                paymentId: Guid.NewGuid(),
                verifiedAt: DateTimeOffset.UtcNow,
                promotionDurationsByLevelId: new Dictionary<Guid, int>(),
                errors: _errors
            );

        act.Should().Throw<BadRequestException>();
        order.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void MarkPaid_WhenNotPendingPayment_ShouldThrowConflictExceptionAndRaiseNothing()
    {
        ContentOrderEntity order = ContentOrderFactory.Create();

        Action act = () =>
            order.MarkPaid(
                paymentId: Guid.NewGuid(),
                verifiedAt: DateTimeOffset.UtcNow,
                promotionDurationsByLevelId: new Dictionary<Guid, int>(),
                errors: _errors
            );

        act.Should().Throw<ConflictException>();
        order.DomainEvents.Should().BeEmpty();
    }

    #endregion

    #region Cancel

    [Fact]
    public void Cancel_WhenDraft_ShouldTransitionToCancelled()
    {
        ContentOrderEntity order = ContentOrderFactory.Create();

        order.Cancel(_errors);

        order.Status.Should().Be(EnumOrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenDraft_ShouldRaiseOrderCancelledEvent()
    {
        ContentOrderEntity order = ContentOrderFactory.Create();

        order.Cancel(_errors);

        order
            .DomainEvents.OfType<OrderCancelledEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new OrderCancelledEvent(order.Id));
    }

    [Fact]
    public void Cancel_WhenPaid_ShouldThrowBadRequestExceptionAndRaiseNothing()
    {
        ContentOrderEntity order = ContentOrderFactory.CreatePaid();
        order.ClearDomainEvents();

        Action act = () => order.Cancel(_errors);

        act.Should().Throw<BadRequestException>();
        order.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Cancel_WhenPendingPayment_ShouldTransitionToCancelled()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();

        order.Cancel(_errors);

        order.Status.Should().Be(EnumOrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenCancelled_ShouldThrowConflictException()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateCancelled();

        Action act = () => order.Cancel(_errors);

        act.Should().Throw<ConflictException>();
    }

    #endregion

    #region Update

    [Fact]
    public void Update_WhenDraft_ShouldUpdateCustomerId()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid newCustomerId = Guid.NewGuid();

        // Act
        order.Update(customerId: newCustomerId, packageId: null, _errors);

        // Assert
        order.CustomerId.Should().Be(newCustomerId);
    }

    [Fact]
    public void Update_WhenDraft_ShouldUpdatePackageId()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid newPackageId = Guid.NewGuid();

        // Act
        order.Update(customerId: null, packageId: newPackageId, _errors);

        // Assert
        order.PackageId.Should().Be(newPackageId);
    }

    [Fact]
    public void Update_WhenDraft_WithNullCustomerId_ShouldKeepExisting()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid originalCustomerId = order.CustomerId;

        // Act
        order.Update(customerId: null, packageId: null, _errors);

        // Assert
        order.CustomerId.Should().Be(originalCustomerId);
    }

    [Fact]
    public void Update_WhenNotDraft_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();

        // Act
        Action act = () => order.Update(customerId: Guid.NewGuid(), packageId: null, _errors);

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region RecalculateTotalFromItems

    [Fact]
    public void RecalculateTotalFromItems_ShouldSumAllTiersAndPromos()
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
        ContentItemTierEntity tier1 = ContentItemTierFactory.Create(item.Id, Guid.NewGuid(), 100m);
        ContentItemTierEntity tier2 = ContentItemTierFactory.Create(item.Id, Guid.NewGuid(), 75m);
        item.Tiers.Add(tier1);
        item.Tiers.Add(tier2);
        order.Items.Add(item);

        // Act
        order.RecalculateTotalFromItems();

        // Assert: 100 (tier1) + 75 (tier2) + 50 (promo) = 225
        order.TotalAmountUsd.Should().Be(225m);
    }

    [Fact]
    public void RecalculateTotalFromItems_ShouldExcludeBonusItems()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid categoryId = Guid.NewGuid();

        ContentOrderItemEntity paidItem = ContentOrderItemFactory.CreateWithPromo(
            order.Id,
            categoryId,
            Guid.NewGuid(),
            20m
        );
        ContentItemTierEntity paidTier = ContentItemTierFactory.Create(paidItem.Id, Guid.NewGuid(), 80m);
        paidItem.Tiers.Add(paidTier);
        order.Items.Add(paidItem);

        ContentOrderItemEntity bonusItem = ContentOrderItemFactory.CreateBonus(order.Id, categoryId);
        ContentItemTierEntity bonusTier = ContentItemTierFactory.Create(bonusItem.Id, Guid.NewGuid(), 50m);
        bonusItem.Tiers.Add(bonusTier);
        order.Items.Add(bonusItem);

        // Act
        order.RecalculateTotalFromItems();

        // Assert: 80 (paid tier) + 20 (paid promo) = 100, bonus excluded
        order.TotalAmountUsd.Should().Be(100m);
    }

    [Fact]
    public void RecalculateTotalFromItems_WithNoItems_ShouldSetTotalToZero()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();

        // Act
        order.RecalculateTotalFromItems();

        // Assert
        order.TotalAmountUsd.Should().Be(0m);
    }

    #endregion
}
