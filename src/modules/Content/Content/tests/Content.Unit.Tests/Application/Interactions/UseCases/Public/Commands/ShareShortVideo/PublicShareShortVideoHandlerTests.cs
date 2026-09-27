using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Interactions.UseCases.Public.Commands.ShareShortVideo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
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
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Interactions.UseCases.Public.Commands.ShareShortVideo;

/// <summary>
/// Unit tests for <see cref="PublicShareShortVideoHandler"/>.
/// </summary>
public class PublicShareShortVideoHandlerTests
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicShareShortVideoHandler _handler;

    public PublicShareShortVideoHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicShareShortVideoHandler(_shortVideoRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenShortVideoExistsAndAnonymous_ShouldAddShareAndCommit()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();

        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        var command = new PublicShareShortVideoCommand(ShortVideoId: shortVideo.Id, UserId: null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _shortVideoRepositoryMock.VerifyAddShareCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenShortVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        _shortVideoRepositoryMock.SetupGetByIdOrThrowNotFound(id);

        var command = new PublicShareShortVideoCommand(ShortVideoId: id, UserId: null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
