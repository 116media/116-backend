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
/// Unit tests for <see cref="RemoveCategoryPricingHandler"/>.
/// </summary>
public class RemoveCategoryPricingHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly RemoveCategoryPricingHandler _handler;

    public RemoveCategoryPricingHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new RemoveCategoryPricingHandler(_categoryRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
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

        var command = new RemoveCategoryPricingCommand(CategoryId: category.Id, PricingTierId: pricingTier.Id);

        _categoryRepositoryMock.SetupGetPricing(category.Id, pricingTier.Id, pricing);
        _categoryRepositoryMock.SetupGetPricingByCategory(category.Id, new List<CategoryPricingEntity>());

        // Act
        RemoveCategoryPricingResult result = await _handler.Handle(command, CancellationToken.None);

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

        var command = new RemoveCategoryPricingCommand(CategoryId: category.Id, PricingTierId: tier1.Id);

        _categoryRepositoryMock.SetupGetPricing(category.Id, tier1.Id, pricingToRemove);
        _categoryRepositoryMock.SetupGetPricingByCategory(category.Id, new List<CategoryPricingEntity> { remaining });

        // Act
        RemoveCategoryPricingResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Pricing.Should().HaveCount(1);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenPricingNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        Guid tierId = Guid.NewGuid();

        var command = new RemoveCategoryPricingCommand(CategoryId: categoryId, PricingTierId: tierId);

        _categoryRepositoryMock.SetupGetPricing(categoryId, tierId, null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
