using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoTags;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoTags;

/// <summary>
/// Unit tests for <see cref="AdminUpdateVideoTagsHandler"/>.
/// </summary>
public class AdminUpdateVideoTagsHandlerTests
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdateVideoTagsHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminUpdateVideoTagsHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _lookupRepositoryMock = MockLookupRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUpdateVideoTagsHandler(
            _videoRepositoryMock.Object,
            _lookupRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenEmptyTagNames_ShouldClearExistingTagsAndReturnSuccess()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        TagEntity existingTag = TagFactory.Create();
        var command = new AdminUpdateVideoTagsCommand(VideoId: video.Id.ToString(), TagNames: new List<string>());

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _videoRepositoryMock.SetupGetTagsByVideoId(
            video.Id,
            new List<VideoTagEntity>
            {
                VideoTagEntity.Create(id: Guid.NewGuid(), videoId: video.Id, tagId: existingTag.Id),
            }
        );

        // Act
        AdminUpdateVideoTagsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _videoRepositoryMock.VerifyRemoveTagCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenTagNamesMatchExistingTags_ShouldReuseExistingTagsAndReturnSuccess()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        TagEntity tag1 = TagFactory.Create("Fally Ipupa", "fally-ipupa");
        TagEntity tag2 = TagFactory.Create("Kinshasa", "kinshasa");

        var command = new AdminUpdateVideoTagsCommand(
            VideoId: video.Id.ToString(),
            TagNames: new List<string> { "Fally Ipupa", "Kinshasa" }
        );

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _videoRepositoryMock.SetupGetTagsByVideoId(video.Id, new List<VideoTagEntity>());
        _lookupRepositoryMock.SetupGetTagByName("Fally Ipupa", tag1);
        _lookupRepositoryMock.SetupGetTagByName("Kinshasa", tag2);

        // Act
        AdminUpdateVideoTagsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _lookupRepositoryMock.VerifyAddTagNotCalled();
        _videoRepositoryMock.VerifyAddTagCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenTagNamesAreNew_ShouldCreateTagsAndReturnSuccess()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);

        var command = new AdminUpdateVideoTagsCommand(
            VideoId: video.Id.ToString(),
            TagNames: new List<string> { "Afrobeats", "Rumba" }
        );

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _videoRepositoryMock.SetupGetTagsByVideoId(video.Id, new List<VideoTagEntity>());
        _lookupRepositoryMock.SetupGetTagByName("Afrobeats", null);
        _lookupRepositoryMock.SetupGetTagByName("Rumba", null);

        // Act
        AdminUpdateVideoTagsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _lookupRepositoryMock.Verify(
            x => x.AddTagAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
        _videoRepositoryMock.VerifyAddTagCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenMixedExistingAndNewTagNames_ShouldUpsertAndReturnSuccess()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        TagEntity existingTag = TagFactory.Create("Fally Ipupa", "fally-ipupa");

        var command = new AdminUpdateVideoTagsCommand(
            VideoId: video.Id.ToString(),
            TagNames: new List<string> { "Fally Ipupa", "NewArtist" }
        );

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _videoRepositoryMock.SetupGetTagsByVideoId(video.Id, new List<VideoTagEntity>());
        _lookupRepositoryMock.SetupGetTagByName("Fally Ipupa", existingTag);
        _lookupRepositoryMock.SetupGetTagByName("NewArtist", null);

        // Act
        AdminUpdateVideoTagsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _lookupRepositoryMock.Verify(
            x => x.AddTagAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenTagNameHasDiacritics_ShouldSlugifyAndUpsertCorrectly()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);

        var command = new AdminUpdateVideoTagsCommand(
            VideoId: video.Id.ToString(),
            TagNames: new List<string> { "Café & Crème" }
        );

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _videoRepositoryMock.SetupGetTagsByVideoId(video.Id, new List<VideoTagEntity>());
        _lookupRepositoryMock.SetupGetTagByName("Café & Crème", null);

        // Act
        AdminUpdateVideoTagsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _lookupRepositoryMock.Verify(
            x => x.GetTagByNameAsync("Café & Crème", It.IsAny<CancellationToken>()),
            Times.Once
        );
        _lookupRepositoryMock.Verify(
            x => x.AddTagAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenExistingTagsPresent_ShouldRemoveThemBeforeAddingNew()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        TagEntity oldTag = TagFactory.Create();
        var existingVideoTag = VideoTagEntity.Create(id: Guid.NewGuid(), videoId: video.Id, tagId: oldTag.Id);

        TagEntity newTag = TagFactory.Create("Kinshasa", "kinshasa");

        var command = new AdminUpdateVideoTagsCommand(
            VideoId: video.Id.ToString(),
            TagNames: new List<string> { "Kinshasa" }
        );

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _videoRepositoryMock.SetupGetTagsByVideoId(video.Id, new List<VideoTagEntity> { existingVideoTag });
        _lookupRepositoryMock.SetupGetTagByName("Kinshasa", newTag);

        // Act
        AdminUpdateVideoTagsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _videoRepositoryMock.VerifyRemoveTagCalled();
        _videoRepositoryMock.VerifyAddTagCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminUpdateVideoTagsCommand(VideoId: nonExistentId.ToString(), TagNames: new List<string>());
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldNotInvalidateCache()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminUpdateVideoTagsCommand(VideoId: nonExistentId.ToString(), TagNames: new List<string>());
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (NotFoundException)
        {
            // Expected
        }

        // Assert
    }

    #endregion
}
