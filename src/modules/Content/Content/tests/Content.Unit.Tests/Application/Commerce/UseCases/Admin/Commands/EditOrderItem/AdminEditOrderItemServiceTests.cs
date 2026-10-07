using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Commerce.UseCases.Admin.Commands.EditOrderItem;

/// <summary>
/// Unit tests for <see cref="AdminEditOrderItemService"/>: the order, item and association gates
/// and the update through the order aggregate.
/// </summary>
public class AdminEditOrderItemServiceTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock = MockContentOrderRepository.Create();
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = MockCategoryRepository.Create();
    private readonly Mock<IPromotionLevelRepository> _promotionLevelRepositoryMock =
        MockPromotionLevelRepository.Create();
    private readonly AdminEditOrderItemService _service;

    public AdminEditOrderItemServiceTests()
    {
        _service = new AdminEditOrderItemService(
            _orderRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _promotionLevelRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    private static (ContentOrderEntity Order, ContentOrderItemEntity Item) DraftOrderWithItem()
    {
        ContentOrderEntity order = ContentOrderFactory.Create();
        ContentOrderItemEntity item = order.AddItem(
            contentKind: EnumCoreContentType.Video,
            categoryId: Guid.NewGuid(),
            promotionLevelId: null,
            promoPriceSnapshotUsd: null,
            socialBoost: false,
            isBonus: false
        );
        return (order, item);
    }

    private static AdminEditOrderItemCommand Command(Guid orderId, Guid itemId, Guid? categoryId = null)
    {
        return new AdminEditOrderItemCommand(
            orderId.ToString(),
            itemId.ToString(),
            null,
            categoryId?.ToString(),
            null,
            true,
            null
        );
    }

    [Fact]
    public async Task EditAsync_ShouldUpdateTheItemThroughTheOrder()
    {
        // Arrange
        (ContentOrderEntity order, ContentOrderItemEntity item) = DraftOrderWithItem();
        _orderRepositoryMock.SetupGetByIdWithItems(order);

        // Act
        ContentOrderItemEntity result = await _service.EditAsync(Command(order.Id, item.Id), CancellationToken.None);

        // Assert
        result.Should().BeSameAs(item);
        item.SocialBoost.Should().BeTrue();
    }

    [Fact]
    public async Task EditAsync_WithANewCategory_ShouldResolveIt()
    {
        // Arrange
        (ContentOrderEntity order, ContentOrderItemEntity item) = DraftOrderWithItem();
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);

        // Act
        await _service.EditAsync(Command(order.Id, item.Id, category.Id), CancellationToken.None);

        // Assert
        item.CategoryId.Should().Be(category.Id);
    }

    [Fact]
    public async Task EditAsync_WhenTheOrderDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _orderRepositoryMock.SetupGetByIdWithItems(null);

        // Act
        Func<Task> act = async () =>
            await _service.EditAsync(Command(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task EditAsync_WhenTheItemIsNotOnTheOrder_ShouldThrowNotFoundException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        _orderRepositoryMock.SetupGetByIdWithItems(order);

        // Act
        Func<Task> act = async () =>
            await _service.EditAsync(Command(order.Id, Guid.NewGuid()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
