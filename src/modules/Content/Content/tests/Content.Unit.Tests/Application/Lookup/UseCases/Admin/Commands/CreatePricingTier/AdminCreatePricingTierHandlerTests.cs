using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePricingTier;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData;
using _116.Tests.TestData.Constants;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Lookup.UseCases.Admin.Commands.CreatePricingTier;

/// <summary>
/// Unit tests for <see cref="AdminCreatePricingTierHandler"/>.
/// </summary>
public class AdminCreatePricingTierHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IPricingTierRepository> _pricingTierRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminCreatePricingTierHandler _handler;

    public AdminCreatePricingTierHandlerTests()
    {
        _pricingTierRepositoryMock = MockPricingTierRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminCreatePricingTierHandler(
            _pricingTierRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenNameDoesNotExist_ShouldCreateAndReturnDto()
    {
        // Arrange
        string name = TestConstants.PricingTier.ValidName;
        var command = new AdminCreatePricingTierCommand(
            Name: name,
            Description: TestConstants.PricingTier.ValidDescription
        );

        _pricingTierRepositoryMock.SetupPricingTierExistsByName(name, false);

        // Act
        AdminCreatePricingTierResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PricingTier.Name.Should().Be(name);
        result.PricingTier.IsActive.Should().BeTrue();

        _pricingTierRepositoryMock.VerifyAddPricingTierCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithDescription_ShouldCreateWithDescription()
    {
        // Arrange
        string name = TestConstants.PricingTier.ValidName;
        string description = TestConstants.PricingTier.ValidDescription;
        var command = new AdminCreatePricingTierCommand(Name: name, Description: description);

        _pricingTierRepositoryMock.SetupPricingTierExistsByName(name, false);

        // Act
        AdminCreatePricingTierResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PricingTier.Description.Should().Be(description);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        string name = TestConstants.PricingTier.ValidName;
        var command = new AdminCreatePricingTierCommand(
            Name: name,
            Description: TestConstants.PricingTier.ValidDescription
        );

        _pricingTierRepositoryMock.SetupPricingTierExistsByName(name, true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ShouldNotAddOrCommit()
    {
        // Arrange
        string name = TestConstants.PricingTier.ValidName;
        var command = new AdminCreatePricingTierCommand(
            Name: name,
            Description: TestConstants.PricingTier.ValidDescription
        );

        _pricingTierRepositoryMock.SetupPricingTierExistsByName(name, true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _pricingTierRepositoryMock.VerifyAddPricingTierNotCalled();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
