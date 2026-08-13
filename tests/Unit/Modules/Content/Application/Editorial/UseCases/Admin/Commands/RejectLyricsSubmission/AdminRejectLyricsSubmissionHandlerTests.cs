using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyricsSubmission;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyricsSubmission;

/// <summary>
/// Unit tests for <see cref="AdminRejectLyricsSubmissionHandler"/>.
/// </summary>
public class AdminRejectLyricsSubmissionHandlerTests
{
    private readonly Mock<ILyricsSubmissionRepository> _submissionRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminRejectLyricsSubmissionHandler _handler;

    public AdminRejectLyricsSubmissionHandlerTests()
    {
        _submissionRepositoryMock = MockLyricsSubmissionRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminRejectLyricsSubmissionHandler(
            _submissionRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _i18n
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSetRejectedStatusAndNote()
    {
        // Arrange
        LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create();
        _submissionRepositoryMock.SetupGetByIdOrThrow(submission);
        var reviewerId = Guid.NewGuid();
        const string note = "Duplicate of an existing song.";
        var command = new AdminRejectLyricsSubmissionCommand(submission.Id, note, reviewerId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        submission.Status.Should().Be(EnumSubmissionStatus.Rejected);
        submission.ReviewedByUserId.Should().Be(reviewerId);
        submission.ReviewNote.Should().Be(note);
        _submissionRepositoryMock.VerifyUpdateCalled(submission);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRaiseLyricsSubmissionDecidedEvent()
    {
        // Arrange
        LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create();
        submission.ClearDomainEvents();
        _submissionRepositoryMock.SetupGetByIdOrThrow(submission);
        var reviewerId = Guid.NewGuid();
        const string note = "Duplicate of an existing song.";
        var command = new AdminRejectLyricsSubmissionCommand(submission.Id, note, reviewerId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        submission
            .DomainEvents.OfType<LyricsSubmissionDecidedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(
                new LyricsSubmissionDecidedEvent(
                    SubmissionId: submission.Id,
                    SubmittedByUserId: submission.SubmittedByUserId,
                    Outcome: EnumSubmissionStatus.Rejected,
                    ReviewNote: note,
                    PublishedLyricsId: null
                )
            );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenSubmissionNotPending_ShouldThrowConflictException()
    {
        // Arrange
        LyricsSubmissionEntity submission = LyricsSubmissionFactory.CreateApproved(Guid.NewGuid(), Guid.NewGuid());
        submission.ClearDomainEvents();
        _submissionRepositoryMock.SetupGetByIdOrThrow(submission);
        var command = new AdminRejectLyricsSubmissionCommand(submission.Id, "Too late.", Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        submission.Status.Should().Be(EnumSubmissionStatus.Approved);
        submission.ReviewNote.Should().BeNull();
        submission.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
