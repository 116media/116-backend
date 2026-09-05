using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyrics;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Shared.Application.Exceptions;
using _116.Shared.Domain.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyrics;

/// <summary>
/// Unit tests for <see cref="AdminRejectLyricsHandler"/>.
/// </summary>
public class AdminRejectLyricsHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminRejectLyricsHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminRejectLyricsHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminRejectLyricsHandler(
            _lyricsRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenLyricsInPendingReview_ShouldRecordStatusAndReason()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreatePendingReview(CategoryId);
        var command = new AdminRejectLyricsCommand(
            Id: lyrics.Id.ToString(),
            Reason: TestConstants.Lyrics.ValidRejectionReason
        );
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        lyrics.Status.Should().Be(EnumContentStatus.Rejected);
        lyrics.RejectionReason.Should().Be(TestConstants.Lyrics.ValidRejectionReason);
        _lyricsRepositoryMock.VerifyUpdateCalled(lyrics);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsInPendingReview_ShouldRaiseCommissionedContentRejectedEvent()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreatePendingReview(CategoryId);
        lyrics.ClearDomainEvents();
        var command = new AdminRejectLyricsCommand(
            Id: lyrics.Id.ToString(),
            Reason: TestConstants.Lyrics.ValidRejectionReason
        );
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        lyrics
            .DomainEvents.OfType<CommissionedContentRejectedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(
                new CommissionedContentRejectedEvent(
                    ContentId: lyrics.Id,
                    ContentType: EnumCoreContentType.Lyrics,
                    CustomerId: lyrics.CustomerId,
                    Title: lyrics.SongTitle,
                    Reason: TestConstants.Lyrics.ValidRejectionReason
                )
            );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminRejectLyricsCommand(
            Id: nonExistentId.ToString(),
            Reason: TestConstants.Lyrics.ValidRejectionReason
        );
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsAlreadyRejected_ShouldThrowConflictException()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreateRejected(CategoryId);
        lyrics.ClearDomainEvents();
        var command = new AdminRejectLyricsCommand(
            Id: lyrics.Id.ToString(),
            Reason: TestConstants.Lyrics.ValidRejectionReason
        );
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        lyrics.Status.Should().Be(EnumContentStatus.Rejected);
        lyrics.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsInWrongStatus_ShouldThrowDomainRuleException()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        lyrics.ClearDomainEvents();
        var command = new AdminRejectLyricsCommand(
            Id: lyrics.Id.ToString(),
            Reason: TestConstants.Lyrics.ValidRejectionReason
        );
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainRuleException>();
        lyrics.Status.Should().Be(EnumContentStatus.Draft);
        lyrics.RejectionReason.Should().BeNull();
        lyrics.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
