using _116.Content.Application.Editorial.UseCases.Admin.Commands.SetLyricsTags;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.SetLyricsTags;

/// <summary>
/// Unit tests for <see cref="AdminSetLyricsTagsHandler"/>.
/// </summary>
public class AdminSetLyricsTagsHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminSetLyricsTagsHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminSetLyricsTagsHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminSetLyricsTagsHandler(_lyricsRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenLyricsExists_ShouldReplaceTagsWithGivenSet()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        var tagIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var command = new AdminSetLyricsTagsCommand(LyricsId: lyrics.Id, TagIds: tagIds);

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _lyricsRepositoryMock
            .Setup(x => x.ReplaceTagsAsync(lyrics.Id, tagIds, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        AdminSetLyricsTagsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _lyricsRepositoryMock.Verify(
            x => x.ReplaceTagsAsync(lyrics.Id, tagIds, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithEmptyTagIds_ShouldReplaceWithEmptySet()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        var tagIds = new List<Guid>();
        var command = new AdminSetLyricsTagsCommand(LyricsId: lyrics.Id, TagIds: tagIds);

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _lyricsRepositoryMock
            .Setup(x => x.ReplaceTagsAsync(lyrics.Id, tagIds, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        AdminSetLyricsTagsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _lyricsRepositoryMock.Verify(
            x => x.ReplaceTagsAsync(lyrics.Id, tagIds, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminSetLyricsTagsCommand(LyricsId: nonExistentId, TagIds: new List<Guid>());
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }
}
