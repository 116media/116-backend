using _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
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
    private readonly Mock<_116.Content.Application.Shared.Repositories.IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<_116.Content.Application.Shared.Persistence.IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminSubmitOrderFactory _factory;

    public AdminSubmitOrderFactoryTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _factory = new AdminSubmitOrderFactory(_orderRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task SubmitAsync_WhenOrderHasItemWithTier_ShouldCreatePaymentAndCommit()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid categoryId = Guid.NewGuid();
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, categoryId);
        ContentItemTierEntity tier = ContentItemTierFactory.CreateDefault(item.Id, Guid.NewGuid());
        item.Tiers.Add(tier);
        order.Items.Add(item);

        // Act
        await _factory.SubmitAsync(order, CancellationToken.None);

        // Assert
        _orderRepositoryMock.VerifyAddPaymentCalled();
        _orderRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task SubmitAsync_WhenNoItemsWithTiers_ShouldThrowBadRequestException()
    {
        // Arrange — order with an item but no tiers on it
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid categoryId = Guid.NewGuid();
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, categoryId);
        order.Items.Add(item);

        // Act
        Func<Task> act = async () => await _factory.SubmitAsync(order, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task SubmitAsync_WhenOrderHasNoItems_ShouldThrowBadRequestException()
    {
        // Arrange — completely empty order
        ContentOrderEntity order = ContentOrderFactory.Create();

        // Act
        Func<Task> act = async () => await _factory.SubmitAsync(order, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion
}
