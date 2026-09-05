using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment;

/// <summary>
/// Unit tests for <see cref="AdminVerifyPaymentFactory"/>.
/// </summary>
public class AdminVerifyPaymentFactoryTests
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminVerifyPaymentFactory _factory;

    private static readonly Guid AdminUserId = Guid.NewGuid();
    private const string ReceiptUrl = TestConstants.Commerce.ValidReceiptUrl;

    public AdminVerifyPaymentFactoryTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _factory = new AdminVerifyPaymentFactory(
            _lookupRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task VerifyAsync_ShouldVerifyPaymentMarkOrderPaidAndCommit()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        ContentPaymentEntity payment = ContentPaymentFactory.CreateWithProof(order.Id, Guid.NewGuid());

        // Act
        await _factory.VerifyAsync(order, payment, AdminUserId, ReceiptUrl, CancellationToken.None);

        // Assert
        payment.Status.Should().Be(EnumPaymentStatus.Verified);
        payment.VerifiedById.Should().Be(AdminUserId);
        payment.VerifiedAt.Should().NotBeNull();
        payment.ReceiptUrl.Should().Be(ReceiptUrl);
        order.Status.Should().Be(EnumOrderStatus.Paid);
        _orderRepositoryMock.Verify(x => x.UpdatePaymentAsync(payment, It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.VerifyUpdateCalled(order);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task VerifyAsync_ShouldRaiseOrderPaidEventWithPaymentId()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        order.ClearDomainEvents();
        ContentPaymentEntity payment = ContentPaymentFactory.CreateWithProof(order.Id, Guid.NewGuid());

        // Act
        await _factory.VerifyAsync(order, payment, AdminUserId, ReceiptUrl, CancellationToken.None);

        // Assert
        OrderPaidEvent paidEvent = order.DomainEvents.OfType<OrderPaidEvent>().Should().ContainSingle().Which;
        paidEvent.OrderId.Should().Be(order.Id);
        paidEvent.PaymentId.Should().Be(payment.Id);
        paidEvent.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task VerifyAsync_WithPromotedItem_ShouldResolveDurationAndComputeWindowOnEventPayload()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        order.ClearDomainEvents();
        PromotionLevelEntity promoLevel = PromotionLevelFactory.CreateDefault();
        ContentOrderItemEntity item = ContentOrderItemFactory.CreateWithPromo(
            order.Id,
            Guid.NewGuid(),
            promoLevel.Id,
            promoLevel.PriceUsd
        );
        order.Items.Add(item);
        ContentPaymentEntity payment = ContentPaymentFactory.CreateWithProof(order.Id, Guid.NewGuid());
        _lookupRepositoryMock.SetupGetPromotionLevelByIdOrThrow(promoLevel);

        // Act
        await _factory.VerifyAsync(order, payment, AdminUserId, ReceiptUrl, CancellationToken.None);

        // Assert
        payment.VerifiedAt.Should().NotBeNull();
        DateTimeOffset verifiedAt = payment.VerifiedAt!.Value;
        DateTimeOffset expectedPaidAt = verifiedAt.AddTicks(-(verifiedAt.Ticks % TimeSpan.TicksPerMillisecond));
        OrderPaidEvent paidEvent = order.DomainEvents.OfType<OrderPaidEvent>().Should().ContainSingle().Which;
        paidEvent.PaidAt.Should().Be(expectedPaidAt);
        PaidItemEffect effect = paidEvent.Items.Should().ContainSingle().Which;
        effect.OrderItemId.Should().Be(item.Id);
        effect.PromotionLevelId.Should().Be(promoLevel.Id);
        effect.PromotionUntil.Should().Be(expectedPaidAt.AddDays(promoLevel.DurationDays));
        effect.SocialBoost.Should().Be(item.SocialBoost);
    }

    [Fact]
    public async Task VerifyAsync_WithoutPromotedItems_ShouldNotQueryPromotionLevels()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        ContentOrderItemEntity item = ContentOrderItemFactory.CreateSocialBoost(order.Id, Guid.NewGuid());
        order.Items.Add(item);
        ContentPaymentEntity payment = ContentPaymentFactory.CreateWithProof(order.Id, Guid.NewGuid());

        // Act
        await _factory.VerifyAsync(order, payment, AdminUserId, ReceiptUrl, CancellationToken.None);

        // Assert
        order.Status.Should().Be(EnumOrderStatus.Paid);
        _lookupRepositoryMock.Verify(
            x => x.GetPromotionLevelByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task VerifyAsync_WhenPaymentAlreadyVerified_ShouldThrowConflictAndNotCommit()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        order.ClearDomainEvents();
        ContentPaymentEntity payment = ContentPaymentFactory.CreateVerified(order.Id);

        // Act
        Func<Task> act = () => _factory.VerifyAsync(order, payment, AdminUserId, ReceiptUrl, CancellationToken.None);

        // Assert
        (await act.Should().ThrowAsync<ContentRuleException>())
            .Which.Code.Should()
            .Be(ContentRuleCodes.PaymentAlreadyVerified);
        payment.Status.Should().Be(EnumPaymentStatus.Verified);
        order.Status.Should().Be(EnumOrderStatus.PendingPayment);
        order.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task VerifyAsync_WhenPaymentAlreadyRejected_ShouldThrowConflictAndNotCommit()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        order.ClearDomainEvents();
        ContentPaymentEntity payment = ContentPaymentFactory.CreateRejected(order.Id);

        // Act
        Func<Task> act = () => _factory.VerifyAsync(order, payment, AdminUserId, ReceiptUrl, CancellationToken.None);

        // Assert
        (await act.Should().ThrowAsync<ContentRuleException>())
            .Which.Code.Should()
            .Be(ContentRuleCodes.PaymentAlreadyRejected);
        payment.Status.Should().Be(EnumPaymentStatus.Rejected);
        order.Status.Should().Be(EnumOrderStatus.PendingPayment);
        order.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task VerifyAsync_WhenOrderAlreadyPaid_ShouldThrowConflictAndNotCommit()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreatePaid();
        order.ClearDomainEvents();
        ContentPaymentEntity payment = ContentPaymentFactory.CreateWithProof(order.Id, Guid.NewGuid());

        // Act
        Func<Task> act = () => _factory.VerifyAsync(order, payment, AdminUserId, ReceiptUrl, CancellationToken.None);

        // Assert
        (await act.Should().ThrowAsync<ContentRuleException>())
            .Which.Code.Should()
            .Be(ContentRuleCodes.OrderAlreadyPaid);
        order.Status.Should().Be(EnumOrderStatus.Paid);
        order.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }
}
