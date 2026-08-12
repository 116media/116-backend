using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
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
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminCreateShortVideoHandler _handler;

    public AdminCreateShortVideoHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();

        _handler = new AdminCreateShortVideoHandler(
            _shortVideoRepositoryMock.Object,
            _fileRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WhenValidStandaloneShortVideo_ShouldCreateInactiveDraftWithoutVideoFile()
    {
        // Arrange
        var command = new AdminCreateShortVideoCommand(
            Title: TestConstants.ShortVideo.ValidTitle,
            Slug: TestConstants.ShortVideo.ValidSlug,
            AuthorId: Guid.NewGuid(),
            VideoId: null
        );

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
        capturedEntity.Should().NotBeNull();
        result.ShortVideo.Id.Should().Be(capturedEntity!.Id);
        result.ShortVideo.IsActive.Should().BeFalse();
        capturedEntity.VideoFileId.Should().BeNull();
        capturedEntity.IsActive.Should().BeFalse();
        _shortVideoRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
        _fileRepositoryMock.Verify(
            x =>
                x.UploadAndStoreVideoFileAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenValidTeaserShortVideo_ShouldCreateAndReturnShortVideo()
    {
        // Arrange
        Guid videoId = Guid.NewGuid();
        var command = new AdminCreateShortVideoCommand(
            Title: TestConstants.ShortVideo.ValidTitle,
            Slug: TestConstants.ShortVideo.ValidSlug,
            AuthorId: Guid.NewGuid(),
            VideoId: videoId
        );

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
        result.ShortVideo.VideoId.Should().Be(videoId);
        result.ShortVideo.HasFullVideo.Should().BeTrue();
        _shortVideoRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenSlugAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        string slug = TestConstants.ShortVideo.ValidSlug;
        var command = new AdminCreateShortVideoCommand(
            Title: TestConstants.ShortVideo.ValidTitle,
            Slug: slug,
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
