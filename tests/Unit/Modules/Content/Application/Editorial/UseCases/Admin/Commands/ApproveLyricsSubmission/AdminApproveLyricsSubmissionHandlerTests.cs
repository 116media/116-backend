using _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission;
using _116.Content.Application.Shared.Errors.Facade;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission;

/// <summary>
/// Unit tests for <see cref="AdminApproveLyricsSubmissionHandler"/>.
/// </summary>
public class AdminApproveLyricsSubmissionHandlerTests
{
    private readonly Mock<ILyricsSubmissionRepository> _submissionRepositoryMock;
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminApproveLyricsSubmissionHandler _handler;

    public AdminApproveLyricsSubmissionHandlerTests()
    {
        _submissionRepositoryMock = MockLyricsSubmissionRepository.Create();
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminApproveLyricsSubmissionHandler(
            _submissionRepositoryMock.Object,
            _lyricsRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _i18n
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateLyricsAndLinkSubmission()
    {
        // Arrange
        LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create("Eloko Oyo", "Fally Ipupa");
        _submissionRepositoryMock.SetupGetByIdOrThrow(submission);
        CategoryEntity category = CategoryFactory.CreateDefaultForLyrics(Guid.NewGuid());
        _categoryRepositoryMock.SetupGetDefaultLyricsCategory(category);
        _lyricsRepositoryMock.SetupGetBySlug("eloko-oyo-lyrics", null);
        var reviewerId = Guid.NewGuid();
        var command = new AdminApproveLyricsSubmissionCommand(submission.Id, "eloko-oyo-lyrics", reviewerId);

        // Act
        AdminApproveLyricsSubmissionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.LyricsId.Should().NotBeEmpty();
        submission.PublishedLyricsId.Should().Be(result.LyricsId);
        submission.ReviewedByUserId.Should().Be(reviewerId);
        _lyricsRepositoryMock.VerifyAddCalled();
        _submissionRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled(2);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenSlugAlreadyExists_ShouldThrowConflictExceptionAndNotCreateLyrics()
    {
        // Arrange
        LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create();
        _submissionRepositoryMock.SetupGetByIdOrThrow(submission);
        LyricsEntity existing = LyricsFactory.CreateWithSlug(Guid.NewGuid(), "taken-slug");
        _lyricsRepositoryMock.SetupGetBySlug("taken-slug", existing);
        var command = new AdminApproveLyricsSubmissionCommand(submission.Id, "taken-slug", Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _lyricsRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<LyricsEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenSubmissionNotPending_ShouldThrowConflictException()
    {
        // Arrange
        LyricsSubmissionEntity submission = LyricsSubmissionFactory.CreateRejected(Guid.NewGuid());
        _submissionRepositoryMock.SetupGetByIdOrThrow(submission);
        var command = new AdminApproveLyricsSubmissionCommand(submission.Id, "eloko-oyo-lyrics", Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
