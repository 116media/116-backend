using _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePromotionLevel;
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

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePromotionLevel;

/// <summary>
/// Unit tests for <see cref="DeactivatePromotionLevelHandler"/>.
/// </summary>
public class DeactivatePromotionLevelHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly DeactivatePromotionLevelHandler _handler;

    public DeactivatePromotionLevelHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new DeactivatePromotionLevelHandler(_lookupRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenActive_ShouldDeactivateAndReturnDto()
    {
        // Arrange
        PromotionLevelEntity active = PromotionLevelFactory.CreateDefault();
        var command = new DeactivatePromotionLevelCommand(Id: active.Id.ToString());

        _lookupRepositoryMock.SetupGetPromotionLevelByIdOrThrow(active);

        // Act
        DeactivatePromotionLevelResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PromotionLevel.IsActive.Should().BeFalse();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenAlreadyInactive_ShouldThrowConflictException()
    {
        // Arrange
        PromotionLevelEntity inactive = PromotionLevelFactory.CreateInactive();
        var command = new DeactivatePromotionLevelCommand(Id: inactive.Id.ToString());

        _lookupRepositoryMock.SetupGetPromotionLevelByIdOrThrow(inactive);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenAlreadyInactive_ShouldNotCommit()
    {
        // Arrange
        PromotionLevelEntity inactive = PromotionLevelFactory.CreateInactive();
        var command = new DeactivatePromotionLevelCommand(Id: inactive.Id.ToString());

        _lookupRepositoryMock.SetupGetPromotionLevelByIdOrThrow(inactive);

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

    [Fact]
    public async Task Handle_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new DeactivatePromotionLevelCommand(Id: nonExistentId.ToString());

        _lookupRepositoryMock.SetupGetPromotionLevelByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
