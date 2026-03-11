using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing;
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

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing;

/// <summary>
/// Unit tests for <see cref="AddCategoryPricingHandler"/>.
/// </summary>
public class AddCategoryPricingHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AddCategoryPricingHandler _handler;

    public AddCategoryPricingHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _lookupRepositoryMock = MockLookupRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AddCategoryPricingHandler(
            _categoryRepositoryMock.Object,
            _lookupRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldAddPricingAndReturnDto()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity pricingTier = PricingTierFactory.CreateDefault();
        decimal priceUsd = TestConstants.Content.CategoryPricing.ValidPriceUsd;

        var command = new AddCategoryPricingCommand(
            CategoryId: category.Id.ToString(),
            PricingTierId: pricingTier.Id,
            PriceUsd: priceUsd
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrow(pricingTier);
        _categoryRepositoryMock.SetupGetPricing(category.Id, pricingTier.Id, null);

        CategoryPricingEntity created = CategoryPricingFactory.Create(category.Id, pricingTier.Id, priceUsd);
        _categoryRepositoryMock
            .SetupSequence(x => x.GetPricingAsync(category.Id, pricingTier.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CategoryPricingEntity?)null)
            .ReturnsAsync(created);

        // Act
        AddCategoryPricingResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Pricing.Should().NotBeNull();

        _categoryRepositoryMock.VerifyAddPricingCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentCategoryId = Guid.NewGuid();

        var command = new AddCategoryPricingCommand(
            CategoryId: nonExistentCategoryId.ToString(),
            PricingTierId: Guid.NewGuid(),
            PriceUsd: TestConstants.Content.CategoryPricing.ValidPriceUsd
        );

        _categoryRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentCategoryId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPricingTierNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        var nonExistentTierId = Guid.NewGuid();

        var command = new AddCategoryPricingCommand(
            CategoryId: category.Id.ToString(),
            PricingTierId: nonExistentTierId,
            PriceUsd: TestConstants.Content.CategoryPricing.ValidPriceUsd
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrowNotFound(nonExistentTierId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPricingTierInactive_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity inactiveTier = PricingTierFactory.CreateInactive();

        var command = new AddCategoryPricingCommand(
            CategoryId: category.Id.ToString(),
            PricingTierId: inactiveTier.Id,
            PriceUsd: TestConstants.Content.CategoryPricing.ValidPriceUsd
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrow(inactiveTier);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenPricingAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity pricingTier = PricingTierFactory.CreateDefault();

        var command = new AddCategoryPricingCommand(
            CategoryId: category.Id.ToString(),
            PricingTierId: pricingTier.Id,
            PriceUsd: TestConstants.Content.CategoryPricing.ValidPriceUsd
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrow(pricingTier);

        CategoryPricingEntity existing = CategoryPricingFactory.Create(category.Id, pricingTier.Id);
        _categoryRepositoryMock.SetupGetPricing(category.Id, pricingTier.Id, existing);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
