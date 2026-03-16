using _116.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing;

/// <summary>
/// Unit tests for <see cref="AdminRemoveCategoryPricingHandler"/>.
/// </summary>
public class AdminRemoveCategoryPricingHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminRemoveCategoryPricingHandler _handler;

    public AdminRemoveCategoryPricingHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminRemoveCategoryPricingHandler(
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenPricingExists_ShouldRemoveAndReturnRemainingList()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity pricingTier = PricingTierFactory.CreateDefault();
        CategoryPricingEntity pricing = CategoryPricingFactory.Create(category.Id, pricingTier.Id);

        var command = new AdminRemoveCategoryPricingCommand(
            CategoryId: category.Id.ToString(),
            PricingTierId: pricingTier.Id
        );

        _categoryRepositoryMock.SetupGetPricing(category.Id, pricingTier.Id, pricing);
        _categoryRepositoryMock.SetupGetPricingByCategory(category.Id, new List<CategoryPricingEntity>());

        // Act
        AdminRemoveCategoryPricingResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Pricing.Should().BeEmpty();

        _categoryRepositoryMock.VerifyRemovePricingCalled(pricing);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenPricingRemoved_ShouldReturnRemainingPricingList()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity tier1 = PricingTierFactory.Create("tier-one");
        PricingTierEntity tier2 = PricingTierFactory.Create("tier-two");

        CategoryPricingEntity pricingToRemove = CategoryPricingFactory.Create(category.Id, tier1.Id);
        CategoryPricingEntity remaining = CategoryPricingFactory.Create(category.Id, tier2.Id);

        var command = new AdminRemoveCategoryPricingCommand(
            CategoryId: category.Id.ToString(),
            PricingTierId: tier1.Id
        );

        _categoryRepositoryMock.SetupGetPricing(category.Id, tier1.Id, pricingToRemove);
        _categoryRepositoryMock.SetupGetPricingByCategory(category.Id, new List<CategoryPricingEntity> { remaining });

        // Act
        AdminRemoveCategoryPricingResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Pricing.Should().ContainSingle();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenPricingNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var tierId = Guid.NewGuid();

        var command = new AdminRemoveCategoryPricingCommand(CategoryId: categoryId.ToString(), PricingTierId: tierId);

        _categoryRepositoryMock.SetupGetPricing(categoryId, tierId, null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
