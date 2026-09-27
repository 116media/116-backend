using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePromotionLevel;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
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
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Lookup.UseCases.Admin.Commands.DeactivatePromotionLevel;

/// <summary>
/// Unit tests for <see cref="AdminDeactivatePromotionLevelHandler"/>.
/// </summary>
public class AdminDeactivatePromotionLevelHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IPromotionLevelRepository> _promotionLevelRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminDeactivatePromotionLevelHandler _handler;

    public AdminDeactivatePromotionLevelHandlerTests()
    {
        _promotionLevelRepositoryMock = MockPromotionLevelRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminDeactivatePromotionLevelHandler(
            _promotionLevelRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenActive_ShouldDeactivateAndReturnDto()
    {
        // Arrange
        PromotionLevelEntity active = PromotionLevelFactory.CreateDefault();
        var command = new AdminDeactivatePromotionLevelCommand(Id: active.Id.ToString());

        _promotionLevelRepositoryMock.SetupGetPromotionLevelByIdOrThrow(active);

        // Act
        AdminDeactivatePromotionLevelResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
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
        var command = new AdminDeactivatePromotionLevelCommand(Id: inactive.Id.ToString());

        _promotionLevelRepositoryMock.SetupGetPromotionLevelByIdOrThrow(inactive);

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
        var command = new AdminDeactivatePromotionLevelCommand(Id: inactive.Id.ToString());

        _promotionLevelRepositoryMock.SetupGetPromotionLevelByIdOrThrow(inactive);

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
        var command = new AdminDeactivatePromotionLevelCommand(Id: nonExistentId.ToString());

        _promotionLevelRepositoryMock.SetupGetPromotionLevelByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
