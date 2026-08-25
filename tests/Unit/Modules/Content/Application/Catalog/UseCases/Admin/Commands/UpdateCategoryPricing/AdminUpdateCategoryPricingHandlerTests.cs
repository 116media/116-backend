using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategoryPricing;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategoryPricing;

/// <summary>
/// Unit tests for <see cref="AdminUpdateCategoryPricingHandler"/>.
/// </summary>
public class AdminUpdateCategoryPricingHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdateCategoryPricingHandler _handler;

    public AdminUpdateCategoryPricingHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUpdateCategoryPricingHandler(
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
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
            TestConstants.CategoryPricing.ValidPriceUsd
        );

        var command = new AdminUpdateCategoryPricingCommand(
            CategoryId: category.Id.ToString(),
            PricingTierId: pricingTier.Id.ToString(),
            PriceUsd: TestConstants.CategoryPricing.UpdatedPriceUsd
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _categoryRepositoryMock.SetupGetPricing(category.Id, pricingTier.Id, pricing);

        // Act
        AdminUpdateCategoryPricingResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Pricing.PriceUsd.Should().Be(TestConstants.CategoryPricing.UpdatedPriceUsd);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenPricingNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        var tierId = Guid.NewGuid();

        var command = new AdminUpdateCategoryPricingCommand(
            CategoryId: category.Id.ToString(),
            PricingTierId: tierId.ToString(),
            PriceUsd: TestConstants.CategoryPricing.ValidPriceUsd
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _categoryRepositoryMock.SetupGetPricing(category.Id, tierId, null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldThrowNotFoundExceptionWithoutReadingPricing()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var tierId = Guid.NewGuid();

        var command = new AdminUpdateCategoryPricingCommand(
            CategoryId: categoryId.ToString(),
            PricingTierId: tierId.ToString(),
            PriceUsd: TestConstants.CategoryPricing.ValidPriceUsd
        );

        _categoryRepositoryMock.SetupGetByIdOrThrowNotFound(categoryId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _categoryRepositoryMock.Verify(
            x => x.GetPricingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    #endregion
}
