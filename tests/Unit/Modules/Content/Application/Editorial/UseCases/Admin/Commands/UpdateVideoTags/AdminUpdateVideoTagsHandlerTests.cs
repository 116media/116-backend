using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoTags;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
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
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenValidTagIds_ShouldReplaceTagsAndReturnSuccess()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        Guid tagId1 = Guid.NewGuid();
        Guid tagId2 = Guid.NewGuid();
        TagEntity tag1 = TagFactory.CreateWithId(tagId1);
        TagEntity tag2 = TagFactory.CreateWithId(tagId2);

        var command = new AdminUpdateVideoTagsCommand(
            VideoId: video.Id.ToString(),
            TagIds: new List<Guid> { tagId1, tagId2 }
        );

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _lookupRepositoryMock.SetupGetTagByIdOrThrow(tag1);
        _lookupRepositoryMock.SetupGetTagByIdOrThrow(tag2);
        _videoRepositoryMock.SetupGetTagsByVideoId(video.Id, new List<VideoTagEntity>());

        // Act
        AdminUpdateVideoTagsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminUpdateVideoTagsCommand(VideoId: nonExistentId.ToString(), TagIds: new List<Guid>());
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenTagNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        Guid nonExistentTagId = Guid.NewGuid();

        var command = new AdminUpdateVideoTagsCommand(
            VideoId: video.Id.ToString(),
            TagIds: new List<Guid> { nonExistentTagId }
        );

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _lookupRepositoryMock.SetupGetTagByIdOrThrowNotFound(nonExistentTagId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
