using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeactivateShortVideo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DeactivateShortVideo;

/// <summary>
/// Unit tests for <see cref="AdminDeactivateShortVideoHandler"/>.
/// </summary>
public class AdminDeactivateShortVideoHandlerTests
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminDeactivateShortVideoHandler _handler;

    public AdminDeactivateShortVideoHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminDeactivateShortVideoHandler(_shortVideoRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenShortVideoIsActive_ShouldDeactivateAndReturnSuccess()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create(); // active by default
        var command = new AdminDeactivateShortVideoCommand(Id: shortVideo.Id.ToString());
        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        // Act
        AdminDeactivateShortVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _shortVideoRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenShortVideoAlreadyInactive_ShouldThrowConflictException()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.CreateInactive();
        var command = new AdminDeactivateShortVideoCommand(Id: shortVideo.Id.ToString());
        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenShortVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminDeactivateShortVideoCommand(Id: nonExistentId.ToString());
        _shortVideoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
