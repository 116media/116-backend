using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem;

/// <summary>
/// Unit tests for <see cref="AdminEditOrderItemHandler"/>.
/// </summary>
public class AdminEditOrderItemHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminEditOrderItemHandler _handler;

    public AdminEditOrderItemHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _lookupRepositoryMock = MockLookupRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminEditOrderItemHandler(
            _orderRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _lookupRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenDraftOrder_ShouldApplySocialBoostAndRecalculateTotal()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = new ContentOrderBuilder().WithCustomer(customer).Build();

        Guid categoryId = Guid.NewGuid();
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, categoryId);
        order.Items.Add(item);

        _orderRepositoryMock
            .Setup(x => x.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _orderRepositoryMock.SetupGetItemByIdOrThrow(item);

        var command = new AdminEditOrderItemCommand(
            OrderId: order.Id.ToString(),
            ItemId: item.Id.ToString(),
            ContentKind: null,
            CategoryId: null,
            PromotionLevelId: null,
            SocialBoost: true,
            IsBonus: null
        );

        // Act
        AdminEditOrderItemResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        item.SocialBoost.Should().BeTrue();
        item.CategoryId.Should().Be(categoryId);
        item.PromotionLevelId.Should().BeNull();
        item.PromoPriceSnapshotUsd.Should().BeNull();
        order.TotalAmountUsd.Should().Be(0);
        result.Item.Id.Should().Be(item.Id);
        result.Item.SocialBoost.Should().BeTrue();
        _orderRepositoryMock.Verify(x => x.UpdateItemAsync(item, It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.VerifyUpdateCalled(order);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        _orderRepositoryMock
            .Setup(x => x.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentOrderEntity?)null);

        var command = new AdminEditOrderItemCommand(
            OrderId: orderId.ToString(),
            ItemId: Guid.NewGuid().ToString(),
            ContentKind: null,
            CategoryId: null,
            PromotionLevelId: null,
            SocialBoost: null,
            IsBonus: null
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenNotDraft_ShouldThrowBadRequestException()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = new ContentOrderBuilder().AsSubmitted().WithCustomer(customer).Build();

        _orderRepositoryMock
            .Setup(x => x.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new AdminEditOrderItemCommand(
            OrderId: order.Id.ToString(),
            ItemId: Guid.NewGuid().ToString(),
            ContentKind: null,
            CategoryId: null,
            PromotionLevelId: null,
            SocialBoost: null,
            IsBonus: null
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        _orderRepositoryMock.Verify(
            x => x.GetItemByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenItemNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = new ContentOrderBuilder().WithCustomer(customer).Build();
        Guid missingItemId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(x => x.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _orderRepositoryMock.SetupGetItemByIdOrThrowNotFound(order.Id, missingItemId);

        var command = new AdminEditOrderItemCommand(
            OrderId: order.Id.ToString(),
            ItemId: missingItemId.ToString(),
            ContentKind: null,
            CategoryId: null,
            PromotionLevelId: null,
            SocialBoost: null,
            IsBonus: null
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
