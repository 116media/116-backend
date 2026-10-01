using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission.Contracts;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission;

/// <summary>
/// Unit tests for <see cref="AdminApproveLyricsSubmissionService"/>: the pending, slug and default
/// category gates and the page built from the submission.
/// </summary>
public class AdminApproveLyricsSubmissionServiceTests
{
    private readonly Mock<ILyricsSubmissionRepository> _submissionRepositoryMock =
        MockLyricsSubmissionRepository.Create();
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock = MockLyricsRepository.Create();
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = MockCategoryRepository.Create();
    private readonly AdminApproveLyricsSubmissionService _service;

    public AdminApproveLyricsSubmissionServiceTests()
    {
        _service = new AdminApproveLyricsSubmissionService(
            _submissionRepositoryMock.Object,
            _lyricsRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task PrepareAsync_WithAPendingSubmission_ShouldBuildTheLyricsUnderTheDefaultCategory()
    {
        // Arrange
        LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create("Song", "Artist");
        CategoryEntity category = CategoryFactory.CreateDefaultForLyrics(Guid.NewGuid());
        Guid reviewerId = Guid.NewGuid();
        _submissionRepositoryMock.SetupGetByIdOrThrow(submission);
        _lyricsRepositoryMock.SetupGetBySlug("song-slug", null);
        _categoryRepositoryMock.SetupGetDefaultLyricsCategory(category);

        // Act
        ApprovedSubmissionData approval = await _service.PrepareAsync(
            submission.Id,
            "song-slug",
            reviewerId,
            CancellationToken.None
        );

        // Assert
        approval.Submission.Should().BeSameAs(submission);
        approval.Lyrics.CategoryId.Should().Be(category.Id);
        approval.Lyrics.SongTitle.Should().Be("Song");
        approval.Lyrics.AuthorId.Should().Be(reviewerId);
    }

    [Fact]
    public async Task PrepareAsync_WhenTheSubmissionIsNotPending_ShouldThrowConflictException()
    {
        // Arrange
        LyricsSubmissionEntity submission = LyricsSubmissionFactory.CreateApproved(Guid.NewGuid(), Guid.NewGuid());
        _submissionRepositoryMock.SetupGetByIdOrThrow(submission);

        // Act
        Func<Task> act = async () =>
            await _service.PrepareAsync(submission.Id, "song-slug", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task PrepareAsync_WhenTheSlugIsTaken_ShouldThrowConflictException()
    {
        // Arrange
        LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create();
        _submissionRepositoryMock.SetupGetByIdOrThrow(submission);
        _lyricsRepositoryMock.SetupGetBySlug("song-slug", LyricsFactory.Create(Guid.NewGuid()));

        // Act
        Func<Task> act = async () =>
            await _service.PrepareAsync(submission.Id, "song-slug", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task PrepareAsync_WhenNoDefaultCategoryIsConfigured_ShouldThrowInternalServerException()
    {
        // Arrange
        LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create();
        _submissionRepositoryMock.SetupGetByIdOrThrow(submission);
        _lyricsRepositoryMock.SetupGetBySlug("song-slug", null);
        _categoryRepositoryMock.SetupGetDefaultLyricsCategory(null);

        // Act
        Func<Task> act = async () =>
            await _service.PrepareAsync(submission.Id, "song-slug", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InternalServerException>();
    }
}
