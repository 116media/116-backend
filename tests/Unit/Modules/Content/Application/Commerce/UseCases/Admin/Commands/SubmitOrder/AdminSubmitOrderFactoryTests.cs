using _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder;

/// <summary>
/// Unit tests for <see cref="AdminSubmitOrderFactory"/>.
/// </summary>
public class AdminSubmitOrderFactoryTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminSubmitOrderFactory _factory;

    public AdminSubmitOrderFactoryTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _factory = new AdminSubmitOrderFactory(
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentOrderErrors()
        );
    }

    #region Success Cases

    [Fact]
    public async Task SubmitAsync_WhenOrderHasItemWithTier_ShouldTransitionToPendingPaymentAndCreatePayment()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid categoryId = Guid.NewGuid();
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, categoryId);
        ContentItemTierEntity tier = ContentItemTierFactory.CreateDefault(item.Id, Guid.NewGuid());
        item.Tiers.Add(tier);
        order.Items.Add(item);
        order.RecalculateTotalFromItems();

        // Act
        await _factory.SubmitAsync(order, CancellationToken.None);

        // Assert
        order.Status.Should().Be(EnumOrderStatus.PendingPayment);
        _orderRepositoryMock.Verify(
            x =>
                x.AddPaymentAsync(
                    It.Is<ContentPaymentEntity>(p =>
                        p.OrderId == order.Id
                        && p.AmountUsd == tier.PriceSnapshotUsd
                        && p.Status == EnumPaymentStatus.Pending
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _orderRepositoryMock.VerifyUpdateCalled(order);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task SubmitAsync_WhenOrderHasItemWithTier_ShouldRaiseOrderSubmittedEvent()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        order.ClearDomainEvents();
        Guid categoryId = Guid.NewGuid();
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, categoryId);
        ContentItemTierEntity tier = ContentItemTierFactory.CreateDefault(item.Id, Guid.NewGuid());
        item.Tiers.Add(tier);
        order.Items.Add(item);

        // Act
        await _factory.SubmitAsync(order, CancellationToken.None);

        // Assert
        order
            .DomainEvents.OfType<OrderSubmittedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new OrderSubmittedEvent(OrderId: order.Id));
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task SubmitAsync_WhenNoItemsWithTiers_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        order.ClearDomainEvents();
        Guid categoryId = Guid.NewGuid();
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, categoryId);
        order.Items.Add(item);

        // Act
        Func<Task> act = async () => await _factory.SubmitAsync(order, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        order.Status.Should().Be(EnumOrderStatus.Draft);
        order.DomainEvents.Should().BeEmpty();
        _orderRepositoryMock.Verify(
            x => x.AddPaymentAsync(It.IsAny<ContentPaymentEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task SubmitAsync_WhenOrderHasNoItems_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        order.ClearDomainEvents();

        // Act
        Func<Task> act = async () => await _factory.SubmitAsync(order, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        order.Status.Should().Be(EnumOrderStatus.Draft);
        order.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task SubmitAsync_WhenOrderAlreadySubmitted_ShouldThrowConflictException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        order.ClearDomainEvents();
        Guid categoryId = Guid.NewGuid();
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, categoryId);
        ContentItemTierEntity tier = ContentItemTierFactory.CreateDefault(item.Id, Guid.NewGuid());
        item.Tiers.Add(tier);
        order.Items.Add(item);

        // Act
        Func<Task> act = async () => await _factory.SubmitAsync(order, CancellationToken.None);

        // Assert
        (await act.Should().ThrowAsync<ContentRuleException>())
            .Which.Code.Should()
            .Be(ContentRuleCodes.OrderAlreadySubmitted);
        order.Status.Should().Be(EnumOrderStatus.PendingPayment);
        order.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
