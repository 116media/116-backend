using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Services;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo;

/// <summary>
/// Unit tests for <see cref="AdminCreateShortVideoHandler"/>.
/// </summary>
public class AdminCreateShortVideoHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<ICloudinaryService> _cloudinaryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminCreateShortVideoHandler _handler;

    public AdminCreateShortVideoHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _cloudinaryMock = MockCloudinaryService.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminCreateShortVideoHandler(
            _shortVideoRepositoryMock.Object,
            _cloudinaryMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    [Fact]
    public async Task Handle_WhenValidStandaloneShortVideo_ShouldCreateAndReturnShortVideo()
    {
        // Arrange
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        var command = new AdminCreateShortVideoCommand(
            Title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            Slug: TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            VideoFile: fileMock,
            AuthorId: Guid.NewGuid(),
            VideoId: null
        );

        // Capture the entity created internally by the handler so GetByIdOrThrowAsync returns it.
        ShortVideoEntity? capturedEntity = null;
        _shortVideoRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ShortVideoEntity>(), It.IsAny<CancellationToken>()))
            .Callback<ShortVideoEntity, CancellationToken>((e, _) => capturedEntity = e)
            .Returns(Task.CompletedTask);
        _shortVideoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => capturedEntity!);

        // Act
        AdminCreateShortVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ShortVideo.Should().NotBeNull();
        _shortVideoRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
        _cloudinaryMock.VerifyUploadCalled();
    }

    [Fact]
    public async Task Handle_WhenValidTeaserShortVideo_ShouldCreateAndReturnShortVideo()
    {
        // Arrange
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        Guid videoId = Guid.NewGuid();
        var command = new AdminCreateShortVideoCommand(
            Title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            Slug: TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            VideoFile: fileMock,
            AuthorId: Guid.NewGuid(),
            VideoId: videoId
        );

        // Capture the entity created internally by the handler so GetByIdOrThrowAsync returns it.
        ShortVideoEntity? capturedEntity = null;
        _shortVideoRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ShortVideoEntity>(), It.IsAny<CancellationToken>()))
            .Callback<ShortVideoEntity, CancellationToken>((e, _) => capturedEntity = e)
            .Returns(Task.CompletedTask);
        _shortVideoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => capturedEntity!);

        // Act
        AdminCreateShortVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ShortVideo.Should().NotBeNull();
        _shortVideoRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenSlugAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        IFormFile fileMock = MockYoutubeThumbnailService.CreateMockFormFile();
        string slug = TestConstants.Content.Editorial.ShortVideo.ValidSlug;
        var command = new AdminCreateShortVideoCommand(
            Title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            Slug: slug,
            VideoFile: fileMock,
            AuthorId: Guid.NewGuid(),
            VideoId: null
        );

        ShortVideoEntity existing = ShortVideoFactory.CreateWithSlug(slug);
        _shortVideoRepositoryMock.SetupGetBySlug(slug, existing);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }
}
