using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePricingTier;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.CreatePricingTier;

/// <summary>
/// Unit tests for <see cref="CreatePricingTierHandler"/>.
/// </summary>
public class CreatePricingTierHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly CreatePricingTierHandler _handler;

    public CreatePricingTierHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new CreatePricingTierHandler(_lookupRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenNameDoesNotExist_ShouldCreateAndReturnDto()
    {
        // Arrange
        string name = TestConstants.Content.PricingTier.ValidName;
        var command = new CreatePricingTierCommand(Name: name, Description: null);

        _lookupRepositoryMock.SetupPricingTierExistsByName(name, false);

        // Act
        CreatePricingTierResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PricingTier.Name.Should().Be(name);
        result.PricingTier.IsActive.Should().BeTrue();

        _lookupRepositoryMock.VerifyAddPricingTierCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithDescription_ShouldCreateWithDescription()
    {
        // Arrange
        string name = TestConstants.Content.PricingTier.ValidName;
        string description = TestConstants.Content.PricingTier.ValidDescription;
        var command = new CreatePricingTierCommand(Name: name, Description: description);

        _lookupRepositoryMock.SetupPricingTierExistsByName(name, false);

        // Act
        CreatePricingTierResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PricingTier.Description.Should().Be(description);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        string name = TestConstants.Content.PricingTier.ValidName;
        var command = new CreatePricingTierCommand(Name: name, Description: null);

        _lookupRepositoryMock.SetupPricingTierExistsByName(name, true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ShouldNotAddOrCommit()
    {
        // Arrange
        string name = TestConstants.Content.PricingTier.ValidName;
        var command = new CreatePricingTierCommand(Name: name, Description: null);

        _lookupRepositoryMock.SetupPricingTierExistsByName(name, true);

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (ConflictException)
        {
            // Expected
        }

        // Assert
        _lookupRepositoryMock.VerifyAddPricingTierNotCalled();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
