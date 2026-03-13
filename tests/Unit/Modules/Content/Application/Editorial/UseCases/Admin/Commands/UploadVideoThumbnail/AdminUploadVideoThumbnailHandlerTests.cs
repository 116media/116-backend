using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail;

/// <summary>
/// Unit tests for <see cref="AdminUploadVideoThumbnailHandler"/>.
/// </summary>
public class AdminUploadVideoThumbnailHandlerTests
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICloudinaryService> _cloudinaryMock;
    private readonly AdminUploadVideoThumbnailHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminUploadVideoThumbnailHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _cloudinaryMock = MockCloudinaryService.Create();
        _handler = new AdminUploadVideoThumbnailHandler(
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _cloudinaryMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenVideoHasNoExistingThumbnail_ShouldUploadAndReturnUrls()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminUploadVideoThumbnailCommand(VideoId: video.Id.ToString(), File: fileMock);

        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        AdminUploadVideoThumbnailResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ThumbnailUrl.Should().NotBeNullOrEmpty();
        result.ThumbnailStorageKey.Should().NotBeNullOrEmpty();
        _cloudinaryMock.VerifyUploadCalled();
        _videoRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
        _cloudinaryMock.VerifyDeleteImageNotCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoHasExistingThumbnail_ShouldDeleteOldThumbnailAfterUpload()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreateWithThumbnail(CategoryId);
        string oldKey = video.ThumbnailStorageKey!;
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminUploadVideoThumbnailCommand(VideoId: video.Id.ToString(), File: fileMock);

        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        AdminUploadVideoThumbnailResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _cloudinaryMock.VerifyUploadCalled();
        _cloudinaryMock.VerifyDeleteImageCalled(oldKey);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminUploadVideoThumbnailCommand(VideoId: nonExistentId.ToString(), File: fileMock);
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
