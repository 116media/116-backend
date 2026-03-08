using _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePromotionLevel;
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

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePromotionLevel;

/// <summary>
/// Unit tests for <see cref="ActivatePromotionLevelHandler"/>.
/// </summary>
public class ActivatePromotionLevelHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly ActivatePromotionLevelHandler _handler;

    public ActivatePromotionLevelHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new ActivatePromotionLevelHandler(_lookupRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenInactive_ShouldActivateAndReturnDto()
    {
        // Arrange
        PromotionLevelEntity inactive = PromotionLevelFactory.CreateInactive();
        var command = new ActivatePromotionLevelCommand(Id: inactive.Id);

        _lookupRepositoryMock.SetupGetPromotionLevelByIdOrThrow(inactive);

        // Act
        ActivatePromotionLevelResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PromotionLevel.IsActive.Should().BeTrue();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenAlreadyActive_ShouldThrowConflictException()
    {
        // Arrange
        PromotionLevelEntity active = PromotionLevelFactory.CreateDefault();
        var command = new ActivatePromotionLevelCommand(Id: active.Id);

        _lookupRepositoryMock.SetupGetPromotionLevelByIdOrThrow(active);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenAlreadyActive_ShouldNotCommit()
    {
        // Arrange
        PromotionLevelEntity active = PromotionLevelFactory.CreateDefault();
        var command = new ActivatePromotionLevelCommand(Id: active.Id);

        _lookupRepositoryMock.SetupGetPromotionLevelByIdOrThrow(active);

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
        Guid nonExistentId = Guid.NewGuid();
        var command = new ActivatePromotionLevelCommand(Id: nonExistentId);

        _lookupRepositoryMock.SetupGetPromotionLevelByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
