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
        var existingVideoTag = VideoTagEntity.Create(id: Guid.NewGuid(), videoId: video.Id, tagId: existingTag.Id);
        var command = new AdminUpdateVideoTagsCommand(VideoId: video.Id.ToString(), TagNames: new List<string>());

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _videoRepositoryMock.SetupGetTagsByVideoId(video.Id, new List<VideoTagEntity> { existingVideoTag });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _videoRepositoryMock.Verify(x => x.RemoveTag(existingVideoTag), Times.Once);
        _videoRepositoryMock.Verify(
            x => x.AddTagAsync(It.IsAny<VideoTagEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
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

        var linked = new List<VideoTagEntity>();
        _videoRepositoryMock
            .Setup(x => x.AddTagAsync(Capture.In(linked), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        linked.Select(t => t.TagId).Should().Equal(tag1.Id, tag2.Id);
        linked.Should().OnlyContain(t => t.VideoId == video.Id);
        _lookupRepositoryMock.VerifyAddTagNotCalled();
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

        var created = new List<TagEntity>();
        _lookupRepositoryMock
            .Setup(x => x.AddTagAsync(Capture.In(created), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var linked = new List<VideoTagEntity>();
        _videoRepositoryMock
            .Setup(x => x.AddTagAsync(Capture.In(linked), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        created.Select(t => t.Name).Should().Equal("Afrobeats", "Rumba");
        _lookupRepositoryMock.Verify(
            x => x.AddTagAsync(It.Is<TagEntity>(t => t.Name == "Afrobeats"), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _lookupRepositoryMock.Verify(
            x => x.AddTagAsync(It.Is<TagEntity>(t => t.Name == "Rumba"), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _lookupRepositoryMock.Verify(
            x => x.AddTagAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
        linked.Select(t => t.TagId).Should().Equal(created.Select(t => t.Id));
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

        var created = new List<TagEntity>();
        _lookupRepositoryMock
            .Setup(x => x.AddTagAsync(Capture.In(created), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var linked = new List<VideoTagEntity>();
        _videoRepositoryMock
            .Setup(x => x.AddTagAsync(Capture.In(linked), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        created.Select(t => t.Name).Should().Equal("NewArtist");
        _lookupRepositoryMock.Verify(
            x => x.AddTagAsync(It.Is<TagEntity>(t => t.Name == "NewArtist"), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _lookupRepositoryMock.Verify(
            x => x.AddTagAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        linked.Select(t => t.TagId).Should().Equal(existingTag.Id, created[0].Id);
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

        var created = new List<TagEntity>();
        _lookupRepositoryMock
            .Setup(x => x.AddTagAsync(Capture.In(created), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        created.Should().ContainSingle();
        created[0].Name.Should().Be("Café & Crème");
        created[0].Slug.Should().StartWith("cafe-creme-");
        _lookupRepositoryMock.Verify(
            x => x.GetTagByNameAsync("Café & Crème", It.IsAny<CancellationToken>()),
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

        var callOrder = new List<string>();
        _videoRepositoryMock.Setup(x => x.RemoveTag(existingVideoTag)).Callback(() => callOrder.Add("remove"));
        _videoRepositoryMock
            .Setup(x => x.AddTagAsync(It.Is<VideoTagEntity>(t => t.TagId == newTag.Id), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("add"))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        callOrder.Should().Equal("remove", "add");
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
    public async Task Handle_WhenVideoNotFound_ShouldNotModifyTagsOrCommit()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminUpdateVideoTagsCommand(VideoId: nonExistentId.ToString(), TagNames: new List<string>());
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _videoRepositoryMock.Verify(x => x.RemoveTag(It.IsAny<VideoTagEntity>()), Times.Never);
        _videoRepositoryMock.Verify(
            x => x.AddTagAsync(It.IsAny<VideoTagEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
