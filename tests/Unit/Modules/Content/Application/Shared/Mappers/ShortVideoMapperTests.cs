using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Contracts.Application;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
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
    private readonly Mock<IFileRepository> _fileRepositoryMock = MockFileRepository.Create();

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

    #region per-user flags

    [Fact]
    public async Task ToShortVideoDtoAsync_WhenFlagsProvided_ShouldStampThem()
    {
        // Arrange
        ShortVideoEntity entity = ShortVideoFactory.Create();

        // Act
        ShortVideoDto dto = await entity.ToShortVideoDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None,
            isLiked: true,
            isBookmarked: true
        );

        // Assert
        dto.IsLiked.Should().BeTrue();
        dto.IsBookmarked.Should().BeTrue();
    }

    [Fact]
    public async Task ToShortVideoDtoAsync_WhenFlagsOmitted_ShouldDefaultToFalse()
    {
        // Arrange
        ShortVideoEntity entity = ShortVideoFactory.Create();

        // Act
        ShortVideoDto dto = await entity.ToShortVideoDtoAsync(
            Mapper,
            _fileRepositoryMock.Object,
            CancellationToken.None
        );

        // Assert
        dto.IsLiked.Should().BeFalse();
        dto.IsBookmarked.Should().BeFalse();
    }

    [Fact]
    public async Task ToShortVideoDtosAsync_WithFlagSets_ShouldStampEachEntityFromItsMembership()
    {
        // Arrange
        ShortVideoEntity liked = ShortVideoFactory.Create();
        ShortVideoEntity bookmarked = ShortVideoFactory.Create();
        ShortVideoEntity neither = ShortVideoFactory.Create();
        IReadOnlyList<ShortVideoEntity> entities = [liked, bookmarked, neither];

        IReadOnlySet<Guid> likedIds = new HashSet<Guid> { liked.Id };
        IReadOnlySet<Guid> bookmarkedIds = new HashSet<Guid> { bookmarked.Id };

        // Act
        IReadOnlyList<ShortVideoDto> dtos = await entities.ToShortVideoDtosAsync(
            Mapper,
            _fileRepositoryMock.Object,
            likedIds,
            bookmarkedIds,
            CancellationToken.None
        );

        // Assert
        dtos.Single(dto => dto.Id == liked.Id).IsLiked.Should().BeTrue();
        dtos.Single(dto => dto.Id == liked.Id).IsBookmarked.Should().BeFalse();
        dtos.Single(dto => dto.Id == bookmarked.Id).IsBookmarked.Should().BeTrue();
        dtos.Single(dto => dto.Id == bookmarked.Id).IsLiked.Should().BeFalse();
        dtos.Single(dto => dto.Id == neither.Id).IsLiked.Should().BeFalse();
        dtos.Single(dto => dto.Id == neither.Id).IsBookmarked.Should().BeFalse();
    }

    #endregion

    #region author + flags

    [Fact]
    public async Task ToShortVideoDtoAsync_WithAuthorAndFlags_ShouldResolveBoth()
    {
        // Arrange
        ShortVideoEntity entity = ShortVideoFactory.Create();
        var userLookup = new Mock<IUserLookupService>();
        userLookup
            .Setup(x => x.GetAuthorInfoByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthorInfo("kinix_editor", "editor@example.com", null, "Admin"));

        // Act
        ShortVideoDto dto = await entity.ToShortVideoDtoAsync(
            Mapper,
            userLookup.Object,
            _fileRepositoryMock.Object,
            CancellationToken.None,
            isLiked: true,
            isBookmarked: true
        );

        // Assert
        dto.Author.Should().NotBeNull();
        dto.Author!.UserName.Should().Be("kinix_editor");
        dto.IsLiked.Should().BeTrue();
        dto.IsBookmarked.Should().BeTrue();
    }

    [Fact]
    public async Task ToShortVideoDtosAsync_WithAuthorAndFlagSets_ShouldBatchResolveAndStampFlags()
    {
        // Arrange
        ShortVideoEntity liked = ShortVideoFactory.Create();
        ShortVideoEntity other = ShortVideoFactory.Create();
        IReadOnlyList<ShortVideoEntity> entities = [liked, other];

        var authors = new Dictionary<Guid, AuthorInfo>
        {
            [liked.AuthorId] = new AuthorInfo("kinix_editor", null, null, "Admin"),
            [other.AuthorId] = new AuthorInfo("kinix_editor", null, null, "Admin"),
        };
        Mock<IUserLookupService> userLookup = MockUserLookupService.Create().SetupGetAuthorInfosByIds(authors);

        // Act
        IReadOnlyList<ShortVideoDto> dtos = await entities.ToShortVideoDtosAsync(
            Mapper,
            userLookup.Object,
            _fileRepositoryMock.Object,
            new HashSet<Guid> { liked.Id },
            new HashSet<Guid>(),
            CancellationToken.None
        );

        // Assert
        dtos.Should().OnlyContain(dto => dto.Author != null && dto.Author.UserName == "kinix_editor");
        dtos.Single(dto => dto.Id == liked.Id).IsLiked.Should().BeTrue();
        dtos.Single(dto => dto.Id == other.Id).IsLiked.Should().BeFalse();
    }

    [Fact]
    public async Task ToShortVideoDtosAsync_ShouldBatchAuthorsAndFilesInOneQueryEach()
    {
        // Arrange
        List<ShortVideoEntity> shorts = ShortVideoFactory.CreateMany(4);
        Mock<IUserLookupService> userLookup = MockUserLookupService.Create();

        // Act
        await shorts.ToShortVideoDtosAsync(
            Mapper,
            userLookup.Object,
            _fileRepositoryMock.Object,
            new HashSet<Guid>(),
            new HashSet<Guid>(),
            CancellationToken.None
        );

        // Assert — one batch call each, not one per item (no N+1)
        userLookup.VerifyGetAuthorInfosByIdsCalledOnce();
        _fileRepositoryMock.VerifyGetByIdsCalledOnce();
        _fileRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void ToShortVideoDto_IoFree_ShouldResolveUrlsAuthorAndFlagsFromMaps()
    {
        // Arrange
        ShortVideoEntity entity = ShortVideoFactory.Create();
        var files = new Dictionary<Guid, FileEntity>
        {
            [entity.VideoFileId!.Value] = FileFactory.CreateWithStorageUrl("https://cdn.example.com/short.mp4"),
        };
        var authors = new Dictionary<Guid, AuthorInfo>
        {
            [entity.AuthorId] = new AuthorInfo("editor", null, null, "Admin"),
        };

        // Act
        ShortVideoDto dto = entity.ToShortVideoDto(
            Mapper,
            files,
            authors,
            new HashSet<Guid> { entity.Id },
            new HashSet<Guid>()
        );

        // Assert
        dto.VideoUrl.Should().Be("https://cdn.example.com/short.mp4");
        dto.Author.Should().NotBeNull();
        dto.Author!.UserName.Should().Be("editor");
        dto.IsLiked.Should().BeTrue();
        dto.IsBookmarked.Should().BeFalse();
    }

    #endregion
}
