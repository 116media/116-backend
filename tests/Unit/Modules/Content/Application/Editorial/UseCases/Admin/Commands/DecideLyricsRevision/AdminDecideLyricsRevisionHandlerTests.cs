using _116.Content.Application.Editorial.UseCases.Admin.Commands.DecideLyricsRevision;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DecideLyricsRevision;

/// <summary>
/// Unit tests for <see cref="AdminDecideLyricsRevisionHandler"/>.
/// </summary>
public class AdminDecideLyricsRevisionHandlerTests
{
    private readonly Mock<ILyricsRevisionRepository> _revisionRepositoryMock;
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminDecideLyricsRevisionHandler _handler;

    public AdminDecideLyricsRevisionHandlerTests()
    {
        _revisionRepositoryMock = MockLyricsRevisionRepository.Create();
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminDecideLyricsRevisionHandler(
            _revisionRepositoryMock.Object,
            _lyricsRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region Accept Cases

    [Fact]
    public async Task Handle_WhenAcceptTrue_ShouldBypassTallyAndReplaceLyricsTextWithRealModerator()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        LyricsRevisionEntity revision = LyricsRevisionFactory.Create(
            lyrics.Id,
            Guid.NewGuid(),
            "Moderator-approved lyrics text."
        );
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        var moderatorId = Guid.NewGuid();
        var command = new AdminDecideLyricsRevisionCommand(revision.Id, Accept: true, moderatorId);

        // Act
        AdminDecideLyricsRevisionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        revision.Status.Should().Be(EnumRevisionStatus.Accepted);
        revision.DecidedByUserId.Should().Be(moderatorId);
        lyrics.LyricsText.Should().Be("Moderator-approved lyrics text.");
        _revisionRepositoryMock.VerifyUpdateCalled();
        _lyricsRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Reject Cases

    [Fact]
    public async Task Handle_WhenAcceptFalse_ShouldBypassTallyAndRejectWithoutTouchingLyrics()
    {
        // Arrange
        LyricsRevisionEntity revision = LyricsRevisionFactory.Create(Guid.NewGuid());
        var moderatorId = Guid.NewGuid();
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        var command = new AdminDecideLyricsRevisionCommand(revision.Id, Accept: false, moderatorId);

        // Act
        AdminDecideLyricsRevisionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        revision.Status.Should().Be(EnumRevisionStatus.Rejected);
        revision.DecidedByUserId.Should().Be(moderatorId);
        _revisionRepositoryMock.VerifyUpdateCalled();
        _lyricsRepositoryMock.Verify(x => x.Update(It.IsAny<LyricsEntity>()), Times.Never);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion
}
