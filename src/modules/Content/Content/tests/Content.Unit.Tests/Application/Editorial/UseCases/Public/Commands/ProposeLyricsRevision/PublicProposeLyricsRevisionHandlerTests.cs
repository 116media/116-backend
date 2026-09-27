using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Public.Commands.ProposeLyricsRevision;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Public.Commands.ProposeLyricsRevision;

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

    [Fact]
    public async Task Handle_AgainstAdminCreatedLyricsWithNoSubmissionHistory_ShouldSucceedIdentically()
    {
        // Arrange
        LyricsEntity adminCreatedLyrics = LyricsFactory.Create(Guid.NewGuid());
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
