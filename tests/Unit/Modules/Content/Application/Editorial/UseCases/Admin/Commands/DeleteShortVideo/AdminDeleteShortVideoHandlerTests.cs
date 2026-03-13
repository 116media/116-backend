using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteShortVideo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Services;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DeleteShortVideo;

/// <summary>
/// Unit tests for <see cref="AdminDeleteShortVideoHandler"/>.
/// </summary>
public class AdminDeleteShortVideoHandlerTests
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<ICloudinaryService> _cloudinaryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminDeleteShortVideoHandler _handler;

    public AdminDeleteShortVideoHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _cloudinaryMock = MockCloudinaryService.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminDeleteShortVideoHandler(
            _shortVideoRepositoryMock.Object,
            _cloudinaryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenShortVideoHasThumbnail_ShouldDeleteBothAssetsAndReturnSuccess()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.CreateWithThumbnail();
        var command = new AdminDeleteShortVideoCommand(Id: shortVideo.Id.ToString());
        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        // Act
        AdminDeleteShortVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // thumbnail deleted + video deleted = 2 calls to DeleteImageAsync
        _cloudinaryMock.Verify(
            x => x.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
        _shortVideoRepositoryMock.VerifyRemoveCalled(shortVideo);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenShortVideoHasNoThumbnail_ShouldDeleteOnlyVideoAssetAndReturnSuccess()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create(); // no thumbnail
        var command = new AdminDeleteShortVideoCommand(Id: shortVideo.Id.ToString());
        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        // Act
        AdminDeleteShortVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // only video deleted = 1 call to DeleteImageAsync
        _cloudinaryMock.Verify(x => x.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _shortVideoRepositoryMock.VerifyRemoveCalled(shortVideo);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenShortVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminDeleteShortVideoCommand(Id: nonExistentId.ToString());
        _shortVideoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
