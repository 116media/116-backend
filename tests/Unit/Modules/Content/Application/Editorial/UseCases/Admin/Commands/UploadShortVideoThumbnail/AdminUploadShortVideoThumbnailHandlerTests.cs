using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail;
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
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail;

/// <summary>
/// Unit tests for <see cref="AdminUploadShortVideoThumbnailHandler"/>.
/// </summary>
public class AdminUploadShortVideoThumbnailHandlerTests
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<ICloudinaryService> _cloudinaryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUploadShortVideoThumbnailHandler _handler;

    public AdminUploadShortVideoThumbnailHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _cloudinaryMock = MockCloudinaryService.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUploadShortVideoThumbnailHandler(
            _shortVideoRepositoryMock.Object,
            _cloudinaryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenShortVideoHasNoExistingThumbnail_ShouldUploadAndReturnUrls()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminUploadShortVideoThumbnailCommand(ShortVideoId: shortVideo.Id.ToString(), File: fileMock);

        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        // Act
        AdminUploadShortVideoThumbnailResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ThumbnailUrl.Should().NotBeNullOrEmpty();
        result.ThumbnailStorageKey.Should().NotBeNullOrEmpty();
        _cloudinaryMock.VerifyUploadCalled();
        _shortVideoRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
        _cloudinaryMock.VerifyDeleteImageNotCalled();
    }

    [Fact]
    public async Task Handle_WhenShortVideoHasExistingThumbnail_ShouldDeleteOldThumbnailAfterUpload()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.CreateWithThumbnail();
        string oldKey = shortVideo.ThumbnailStorageKey!;
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminUploadShortVideoThumbnailCommand(ShortVideoId: shortVideo.Id.ToString(), File: fileMock);

        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        // Act
        AdminUploadShortVideoThumbnailResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _cloudinaryMock.VerifyUploadCalled();
        _cloudinaryMock.VerifyDeleteImageCalled(oldKey);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenShortVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminUploadShortVideoThumbnailCommand(ShortVideoId: nonExistentId.ToString(), File: fileMock);
        _shortVideoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
