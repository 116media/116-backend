using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategoryPricing;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategoryPricing;

/// <summary>
/// Unit tests for <see cref="UpdateCategoryPricingHandler"/>.
/// </summary>
public class UpdateCategoryPricingHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly UpdateCategoryPricingHandler _handler;

    public UpdateCategoryPricingHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new UpdateCategoryPricingHandler(_categoryRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenPricingExists_ShouldUpdateAndReturnDto()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity pricingTier = PricingTierFactory.CreateDefault();
        CategoryPricingEntity pricing = CategoryPricingFactory.Create(
            category.Id,
            pricingTier.Id,
            TestConstants.Content.CategoryPricing.ValidPriceUsd
        );

        var command = new UpdateCategoryPricingCommand(
            CategoryId: category.Id.ToString(),
            PricingTierId: pricingTier.Id.ToString(),
            PriceUsd: TestConstants.Content.CategoryPricing.UpdatedPriceUsd
        );

        _categoryRepositoryMock.SetupGetPricing(category.Id, pricingTier.Id, pricing);

        // Act
        UpdateCategoryPricingResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Pricing.Should().NotBeNull();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenPricingNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var tierId = Guid.NewGuid();

        var command = new UpdateCategoryPricingCommand(
            CategoryId: categoryId.ToString(),
            PricingTierId: tierId.ToString(),
            PriceUsd: TestConstants.Content.CategoryPricing.ValidPriceUsd
        );

        _categoryRepositoryMock.SetupGetPricing(categoryId, tierId, null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
