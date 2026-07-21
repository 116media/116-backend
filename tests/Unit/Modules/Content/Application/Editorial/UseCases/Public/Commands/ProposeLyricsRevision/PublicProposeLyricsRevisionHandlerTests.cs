using _116.Content.Application.Editorial.UseCases.Public.Commands.ProposeLyricsRevision;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Commands.ProposeLyricsRevision;

/// <summary>
/// Unit tests for <see cref="PublicProposeLyricsRevisionHandler"/>.
/// </summary>
public class PublicProposeLyricsRevisionHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<ILyricsRevisionRepository> _revisionRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicProposeLyricsRevisionHandler _handler;

    public PublicProposeLyricsRevisionHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _revisionRepositoryMock = MockLyricsRevisionRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicProposeLyricsRevisionHandler(
            _lyricsRepositoryMock.Object,
            _revisionRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreatePendingRevision()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        var command = new PublicProposeLyricsRevisionCommand(
            lyrics.Id,
            "Corrected lyrics text.",
            "Fixed a misheard line.",
            Guid.NewGuid()
        );

        // Act
        PublicProposeLyricsRevisionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.RevisionId.Should().NotBeEmpty();
        _revisionRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    /// <summary>
    /// Proves there is no trust exemption based on origin: a correction proposed against a
    /// lyrics page created via the plain admin/CreateFree path — with no submission history at
    /// all — succeeds identically to any other lyrics page, since the handler only ever confirms
    /// the lyrics record exists and never inspects how it came to exist.
    /// </summary>
    [Fact]
    public async Task Handle_AgainstAdminCreatedLyricsWithNoSubmissionHistory_ShouldSucceedIdentically()
    {
        // Arrange
        LyricsEntity adminCreatedLyrics = LyricsFactory.CreateFree(Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(adminCreatedLyrics);
        var command = new PublicProposeLyricsRevisionCommand(
            adminCreatedLyrics.Id,
            "Corrected lyrics text.",
            null,
            Guid.NewGuid()
        );

        // Act
        PublicProposeLyricsRevisionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.RevisionId.Should().NotBeEmpty();
        _revisionRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var lyricsId = Guid.NewGuid();
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(lyricsId);
        var command = new PublicProposeLyricsRevisionCommand(lyricsId, "Corrected lyrics text.", null, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _revisionRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<LyricsRevisionEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    #endregion
}
