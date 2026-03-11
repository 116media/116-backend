using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePricingTier;
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

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePricingTier;

/// <summary>
/// Unit tests for <see cref="UpdatePricingTierHandler"/>.
/// </summary>
public class UpdatePricingTierHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly UpdatePricingTierHandler _handler;

    public UpdatePricingTierHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new UpdatePricingTierHandler(_lookupRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithNewName_ShouldUpdateAndReturnDto()
    {
        // Arrange
        PricingTierEntity existing = PricingTierFactory.CreateDefault();
        string newName = TestConstants.Content.PricingTier.AnotherValidName;
        var command = new UpdatePricingTierCommand(Id: existing.Id.ToString(), Name: newName, Description: null);

        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrow(existing);
        _lookupRepositoryMock.SetupPricingTierExistsByName(newName, false);

        // Act
        UpdatePricingTierResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PricingTier.Name.Should().Be(newName);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithSameName_ShouldAllowUpdateDespiteConflict()
    {
        // Arrange
        string sameName = TestConstants.Content.PricingTier.ValidName;
        PricingTierEntity existing = PricingTierFactory.Create(sameName);
        var command = new UpdatePricingTierCommand(Id: existing.Id.ToString(), Name: sameName, Description: null);

        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrow(existing);
        _lookupRepositoryMock.SetupPricingTierExistsByName(sameName, true);

        // Act
        UpdatePricingTierResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PricingTier.Name.Should().Be(sameName);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithDescription_ShouldUpdateWithDescription()
    {
        // Arrange
        PricingTierEntity existing = PricingTierFactory.CreateDefault();
        string newName = TestConstants.Content.PricingTier.AnotherValidName;
        string description = TestConstants.Content.PricingTier.ValidDescription;
        var command = new UpdatePricingTierCommand(Id: existing.Id.ToString(), Name: newName, Description: description);

        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrow(existing);
        _lookupRepositoryMock.SetupPricingTierExistsByName(newName, false);

        // Act
        UpdatePricingTierResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PricingTier.Description.Should().Be(description);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new UpdatePricingTierCommand(
            Id: nonExistentId.ToString(),
            Name: TestConstants.Content.PricingTier.ValidName,
            Description: null
        );

        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNameConflictWithDifferentEntity_ShouldThrowConflictException()
    {
        // Arrange
        PricingTierEntity existing = PricingTierFactory.CreateDefault();
        string conflictingName = TestConstants.Content.PricingTier.AnotherValidName;
        var command = new UpdatePricingTierCommand(
            Id: existing.Id.ToString(),
            Name: conflictingName,
            Description: null
        );

        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrow(existing);
        _lookupRepositoryMock.SetupPricingTierExistsByName(conflictingName, true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenNameConflictWithDifferentEntity_ShouldNotCommit()
    {
        // Arrange
        PricingTierEntity existing = PricingTierFactory.CreateDefault();
        string conflictingName = TestConstants.Content.PricingTier.AnotherValidName;
        var command = new UpdatePricingTierCommand(
            Id: existing.Id.ToString(),
            Name: conflictingName,
            Description: null
        );

        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrow(existing);
        _lookupRepositoryMock.SetupPricingTierExistsByName(conflictingName, true);

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
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion

    #region Cancellation Token

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        PricingTierEntity existing = PricingTierFactory.CreateDefault();
        string newName = TestConstants.Content.PricingTier.AnotherValidName;
        var command = new UpdatePricingTierCommand(Id: existing.Id.ToString(), Name: newName, Description: null);
        using var cts = new CancellationTokenSource();

        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrow(existing);
        _lookupRepositoryMock.SetupPricingTierExistsByName(newName, false);

        // Act
        UpdatePricingTierResult result = await _handler.Handle(command, cts.Token);

        // Assert
        result.Should().NotBeNull();
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion
}
