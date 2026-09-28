using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePromotionLevel;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Lookup.UseCases.Admin.Commands.ActivatePromotionLevel;

/// <summary>
/// Unit tests for <see cref="AdminActivatePromotionLevelHandler"/>.
/// </summary>
public class AdminActivatePromotionLevelHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IPromotionLevelRepository> _promotionLevelRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminActivatePromotionLevelHandler _handler;

    public AdminActivatePromotionLevelHandlerTests()
    {
        _promotionLevelRepositoryMock = MockPromotionLevelRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminActivatePromotionLevelHandler(
            _promotionLevelRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenInactive_ShouldActivateAndReturnDto()
    {
        // Arrange
        PromotionLevelEntity inactive = PromotionLevelFactory.CreateInactive();
        var command = new AdminActivatePromotionLevelCommand(Id: inactive.Id.ToString());

        _promotionLevelRepositoryMock.SetupGetPromotionLevelByIdOrThrow(inactive);

        // Act
        AdminActivatePromotionLevelResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
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
        var command = new AdminActivatePromotionLevelCommand(Id: active.Id.ToString());

        _promotionLevelRepositoryMock.SetupGetPromotionLevelByIdOrThrow(active);

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
        var command = new AdminActivatePromotionLevelCommand(Id: active.Id.ToString());

        _promotionLevelRepositoryMock.SetupGetPromotionLevelByIdOrThrow(active);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new AdminActivatePromotionLevelCommand(Id: nonExistentId.ToString());

        _promotionLevelRepositoryMock.SetupGetPromotionLevelByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
