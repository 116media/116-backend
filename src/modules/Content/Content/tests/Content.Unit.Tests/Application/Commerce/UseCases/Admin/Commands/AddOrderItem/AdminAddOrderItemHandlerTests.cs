using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem.Contracts;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Commerce.UseCases.Admin.Commands.AddOrderItem;

/// <summary>
/// Unit tests for <see cref="AdminAddOrderItemHandler"/>.
/// </summary>
public class AdminAddOrderItemHandlerTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IAddOrderItemService> _serviceMock;
    private readonly AdminAddOrderItemHandler _handler;

    public AdminAddOrderItemHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _serviceMock = MockAddOrderItemService.Create();
        _handler = new AdminAddOrderItemHandler(
            _orderRepositoryMock.Object,
            _serviceMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldReturnOrderItemDto()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid categoryId = Guid.NewGuid();
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, categoryId);
        const string categoryName = "Artist Profile";

        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _serviceMock.SetupCreateItemAsync((item, categoryName, null));

        var command = new AdminAddOrderItemCommand(
            OrderId: order.Id.ToString(),
            ContentKind: EnumCoreContentType.Article,
            CategoryId: categoryId.ToString(),
            PromotionLevelId: null,
            SocialBoost: false,
            IsBonus: false
        );

        // Act
        AdminAddOrderItemResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Item.Id.Should().Be(item.Id);
        result.Item.CategoryName.Should().Be(categoryName);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        _orderRepositoryMock.SetupGetByIdOrThrowNotFound(orderId);

        var command = new AdminAddOrderItemCommand(
            OrderId: orderId.ToString(),
            ContentKind: EnumCoreContentType.Article,
            CategoryId: Guid.NewGuid().ToString(),
            PromotionLevelId: null,
            SocialBoost: false,
            IsBonus: false
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
