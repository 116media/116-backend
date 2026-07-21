using _116.Content.Application.Editorial.UseCases.Admin.Commands.RequestLyricsSubmissionRevision;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RequestLyricsSubmissionRevision;

/// <summary>
/// Unit tests for <see cref="AdminRequestLyricsSubmissionRevisionHandler"/>.
/// </summary>
public class AdminRequestLyricsSubmissionRevisionHandlerTests
{
    private readonly Mock<ILyricsSubmissionRepository> _submissionRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminRequestLyricsSubmissionRevisionHandler _handler;

    public AdminRequestLyricsSubmissionRevisionHandlerTests()
    {
        _submissionRepositoryMock = MockLyricsSubmissionRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminRequestLyricsSubmissionRevisionHandler(
            _submissionRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _i18n
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSetNeedsRevisionStatusAndNote()
    {
        // Arrange
        LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create();
        _submissionRepositoryMock.SetupGetByIdOrThrow(submission);
        var reviewerId = Guid.NewGuid();
        const string note = "Please fix the line breaks before resubmitting.";
        var command = new AdminRequestLyricsSubmissionRevisionCommand(submission.Id, note, reviewerId);

        // Act
        AdminRequestLyricsSubmissionRevisionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        submission.Status.Should().Be(EnumSubmissionStatus.NeedsRevision);
        submission.ReviewedByUserId.Should().Be(reviewerId);
        submission.ReviewNote.Should().Be(note);
        _submissionRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenSubmissionNotPending_ShouldThrowConflictException()
    {
        // Arrange
        LyricsSubmissionEntity submission = LyricsSubmissionFactory.CreateRejected(Guid.NewGuid());
        _submissionRepositoryMock.SetupGetByIdOrThrow(submission);
        var command = new AdminRequestLyricsSubmissionRevisionCommand(submission.Id, "Too late.", Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
