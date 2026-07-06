using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Mappers;

/// <summary>
/// Unit tests for <see cref="ShortVideoMapper"/> extension methods, covering file-URL resolution
/// and the auto-thumbnail generation fallback.
/// </summary>
public class ShortVideoMapperTests : BaseContentHandlerTest
{
    private readonly Mock<IFileRepository> _fileRepositoryMock = new();

    private void SetupFile(Guid fileId, FileEntity file)
    {
        _fileRepositoryMock.Setup(x => x.GetByIdAsync(fileId, It.IsAny<CancellationToken>())).ReturnsAsync(file);
    }

    #region ToShortVideoDtoAsync — video url resolution

    [Fact]
    public async Task ToShortVideoDtoAsync_ShouldResolveVideoUrlFromFile()
    {
        // Arrange
        ShortVideoEntity entity = ShortVideoFactory.Create();
        const string videoUrl = "https://res.cloudinary.com/demo/video/upload/v1/shorts/sample.mp4";
        SetupFile(entity.VideoFileId!.Value, FileFactory.CreateWithStorageUrl(videoUrl));

        // Act
        ShortVideoDto dto = await entity.ToShortVideoDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.VideoUrl.Should().Be(videoUrl);
    }

    [Fact]
    public async Task ToShortVideoDtoAsync_WhenNoVideoFile_ShouldMapUrlsAsNull()
    {
        // Arrange — draft short video has neither a video file nor a thumbnail file
        ShortVideoEntity entity = ShortVideoFactory.CreateDraft();

        // Act
        ShortVideoDto dto = await entity.ToShortVideoDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.VideoUrl.Should().BeNull();
        dto.ThumbnailUrl.Should().BeNull();
    }

    #endregion

    #region ToShortVideoDtoAsync — auto-thumbnail generation

    [Fact]
    public async Task ToShortVideoDtoAsync_WhenNoManualThumbnail_ShouldGenerateAutoThumbnailUrl()
    {
        // Arrange — a short video with a Cloudinary video file but no uploaded thumbnail
        ShortVideoEntity entity = ShortVideoFactory.Create();
        entity.ThumbnailFileId.Should().BeNull();

        const string videoUrl = "https://res.cloudinary.com/demo/video/upload/v1/shorts/sample.mp4";
        SetupFile(entity.VideoFileId!.Value, FileFactory.CreateWithStorageUrl(videoUrl));

        // Act
        ShortVideoDto dto = await entity.ToShortVideoDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert — screenshot transformation inserted and extension changed to jpg
        dto.ThumbnailUrl.Should()
            .Be("https://res.cloudinary.com/demo/video/upload/so_1,q_auto,f_auto,w_720/v1/shorts/sample.jpg");
    }

    [Fact]
    public async Task ToShortVideoDtoAsync_WhenManualThumbnailExists_ShouldUseUploadedThumbnailUrl()
    {
        // Arrange — a short video with both a video file and an uploaded thumbnail file
        ShortVideoEntity entity = ShortVideoFactory.CreateWithThumbnail();
        const string videoUrl = "https://res.cloudinary.com/demo/video/upload/v1/shorts/sample.mp4";
        const string thumbnailUrl = "https://res.cloudinary.com/demo/image/upload/v1/shorts/custom-thumb.jpg";
        SetupFile(entity.VideoFileId!.Value, FileFactory.CreateWithStorageUrl(videoUrl));
        SetupFile(entity.ThumbnailFileId!.Value, FileFactory.CreateWithStorageUrl(thumbnailUrl));

        // Act
        ShortVideoDto dto = await entity.ToShortVideoDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert — uses the uploaded thumbnail, not a generated one
        dto.ThumbnailUrl.Should().Be(thumbnailUrl);
    }

    #endregion

    #region ToShortVideoDtosAsync — list mapping

    [Fact]
    public async Task ToShortVideoDtosAsync_ShouldMapAllEntities()
    {
        // Arrange
        IReadOnlyList<ShortVideoEntity> entities = ShortVideoFactory.CreateMany(3);

        // Act
        IReadOnlyList<ShortVideoDto> dtos = await entities.ToShortVideoDtosAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dtos.Should().HaveCount(3);
    }

    [Fact]
    public async Task ToShortVideoDtosAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        IReadOnlyList<ShortVideoEntity> entities = [];

        // Act
        IReadOnlyList<ShortVideoDto> dtos = await entities.ToShortVideoDtosAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dtos.Should().BeEmpty();
    }

    #endregion
}
