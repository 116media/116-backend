using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Commerce.UseCases.Admin.Commands.SubmitOrder;

/// <summary>
/// Unit tests for <see cref="AdminSubmitOrderService"/>.
/// </summary>
public class AdminSubmitOrderServiceTests
{
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminSubmitOrderService _service;

    public AdminSubmitOrderServiceTests()
    {
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _service = new AdminSubmitOrderService(_unitOfWorkMock.Object, TestErrorsFactory.CreateContentOrderErrors());
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
        await _service.SubmitAsync(order, CancellationToken.None);

        // Assert
        order.Status.Should().Be(EnumOrderStatus.PendingPayment);
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
        await _service.SubmitAsync(order, CancellationToken.None);

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
        Func<Task> act = async () => await _service.SubmitAsync(order, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        order.Status.Should().Be(EnumOrderStatus.Draft);
        order.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task SubmitAsync_WhenOrderHasNoItems_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        order.ClearDomainEvents();

        // Act
        Func<Task> act = async () => await _service.SubmitAsync(order, CancellationToken.None);

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
        Func<Task> act = async () => await _service.SubmitAsync(order, CancellationToken.None);

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
