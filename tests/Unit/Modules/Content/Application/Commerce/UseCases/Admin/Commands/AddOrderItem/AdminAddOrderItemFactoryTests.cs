using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem;

/// <summary>
/// Unit tests for <see cref="AdminAddOrderItemFactory"/>.
/// </summary>
public class AdminAddOrderItemFactoryTests
{
    private readonly Mock<_116.Content.Application.Shared.Repositories.ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<_116.Content.Application.Shared.Repositories.ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<_116.Content.Application.Shared.Repositories.IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<_116.Content.Application.Shared.Persistence.IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminAddOrderItemFactory _factory;

    public AdminAddOrderItemFactoryTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _lookupRepositoryMock = MockLookupRepository.Create();
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _factory = new AdminAddOrderItemFactory(
            _categoryRepositoryMock.Object,
            _lookupRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task CreateItemAsync_WhenAllValid_NoPromo_ShouldReturnItemWithCategoryName()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);
        _categoryRepositoryMock.SetupGetByIdAsync(category.Id, category);

        // Act
        (ContentOrderItemEntity item, string categoryName, string? promoName) = await _factory.CreateItemAsync(
            order,
            EnumCoreContentType.Article,
            category.Id,
            promotionLevelId: null,
            socialBoost: false,
            isBonus: false,
            CancellationToken.None
        );

        // Assert
        item.Should().NotBeNull();
        item.OrderId.Should().Be(order.Id);
        item.CategoryId.Should().Be(category.Id);
        categoryName.Should().Be(category.Name);
        promoName.Should().BeNull();
        _orderRepositoryMock.VerifyAddItemCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task CreateItemAsync_WithPromoLevel_ShouldSnapshotPromoPrice()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);
        PromotionLevelEntity promoLevel = PromotionLevelFactory.CreateDefault();
        _categoryRepositoryMock.SetupGetByIdAsync(category.Id, category);
        _lookupRepositoryMock.SetupGetPromotionLevelByIdOrThrow(promoLevel);

        // Act
        (ContentOrderItemEntity item, string categoryName, string? promoName) = await _factory.CreateItemAsync(
            order,
            EnumCoreContentType.Article,
            category.Id,
            promotionLevelId: promoLevel.Id,
            socialBoost: false,
            isBonus: false,
            CancellationToken.None
        );

        // Assert
        item.Should().NotBeNull();
        item.PromotionLevelId.Should().Be(promoLevel.Id);
        item.PromoPriceSnapshotUsd.Should().Be(promoLevel.PriceUsd);
        categoryName.Should().Be(category.Name);
        promoName.Should().Be(promoLevel.Name);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task CreateItemAsync_WhenOrderNotDraft_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();

        // Act
        Func<Task> act = async () =>
            await _factory.CreateItemAsync(
                order,
                EnumCoreContentType.Article,
                Guid.NewGuid(),
                promotionLevelId: null,
                socialBoost: false,
                isBonus: false,
                CancellationToken.None
            );

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task CreateItemAsync_WhenCategoryNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid categoryId = Guid.NewGuid();
        _categoryRepositoryMock.SetupGetByIdAsync(categoryId, null);

        // Act
        Func<Task> act = async () =>
            await _factory.CreateItemAsync(
                order,
                EnumCoreContentType.Article,
                categoryId,
                promotionLevelId: null,
                socialBoost: false,
                isBonus: false,
                CancellationToken.None
            );

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateItemAsync_WhenPromoLevelNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);
        Guid missingPromoId = Guid.NewGuid();
        _categoryRepositoryMock.SetupGetByIdAsync(category.Id, category);
        _lookupRepositoryMock.SetupGetPromotionLevelByIdOrThrowNotFound(missingPromoId);

        // Act
        Func<Task> act = async () =>
            await _factory.CreateItemAsync(
                order,
                EnumCoreContentType.Article,
                category.Id,
                promotionLevelId: missingPromoId,
                socialBoost: false,
                isBonus: false,
                CancellationToken.None
            );

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateItemAsync_WhenPromoLevelInactive_ShouldThrowNotFoundException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);
        PromotionLevelEntity inactivePromo = PromotionLevelFactory.CreateInactive();
        _categoryRepositoryMock.SetupGetByIdAsync(category.Id, category);
        _lookupRepositoryMock.SetupGetPromotionLevelByIdOrThrow(inactivePromo);

        // Act
        Func<Task> act = async () =>
            await _factory.CreateItemAsync(
                order,
                EnumCoreContentType.Article,
                category.Id,
                promotionLevelId: inactivePromo.Id,
                socialBoost: false,
                isBonus: false,
                CancellationToken.None
            );

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateItemAsync_WhenCategoryNotCommissionable_ShouldThrowNotFoundException()
    {
        // Arrange — free category is not commissionable
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity freeCategory = CategoryFactory.CreateFree(contentTypeId);
        _categoryRepositoryMock.SetupGetByIdAsync(freeCategory.Id, freeCategory);

        // Act
        Func<Task> act = async () =>
            await _factory.CreateItemAsync(
                order,
                EnumCoreContentType.Article,
                freeCategory.Id,
                promotionLevelId: null,
                socialBoost: false,
                isBonus: false,
                CancellationToken.None
            );

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
