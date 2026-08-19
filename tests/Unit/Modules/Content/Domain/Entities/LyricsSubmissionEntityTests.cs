using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="LyricsSubmissionEntity"/>.
/// </summary>
public class LyricsSubmissionEntityTests
{
    #region Submit Tests

    [Fact]
    public void Submit_WithValidParams_ShouldAssignAllFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const string songTitle = "Eloko Oyo";
        const string artistName = "Fally Ipupa";
        const string lyricsText = "Some submitted lyrics text.";
        const string language = "fr";

        // Act
        LyricsSubmissionEntity submission = LyricsSubmissionEntity.Submit(
            id,
            songTitle,
            artistName,
            lyricsText,
            language,
            userId
        );

        // Assert
        submission.Id.Should().Be(id);
        submission.SongTitle.Should().Be(songTitle);
        submission.ArtistName.Should().Be(artistName);
        submission.LyricsText.Should().Be(lyricsText);
        submission.Language.Should().Be(language);
        submission.SubmittedByUserId.Should().Be(userId);
    }

    [Fact]
    public void Submit_ShouldStartInPendingStatus()
    {
        // Act
        LyricsSubmissionEntity submission = LyricsSubmissionEntity.Submit(
            Guid.NewGuid(),
            "Eloko Oyo",
            "Fally Ipupa",
            "Some submitted lyrics text.",
            "fr",
            Guid.NewGuid()
        );

        // Assert
        submission.Status.Should().Be(EnumSubmissionStatus.Pending);
        submission.ReviewedByUserId.Should().BeNull();
        submission.ReviewNote.Should().BeNull();
        submission.PublishedLyricsId.Should().BeNull();
    }

    #endregion

    #region Approve Tests

    [Fact]
    public void Approve_ShouldSetStatusReviewedByUserIdAndPublishedLyricsId()
    {
        // Arrange
        LyricsSubmissionEntity submission = CreatePendingSubmission();
        var reviewerId = Guid.NewGuid();
        var publishedLyricsId = Guid.NewGuid();

        // Act
        submission.Approve(reviewerId, publishedLyricsId);

        // Assert
        submission.Status.Should().Be(EnumSubmissionStatus.Approved);
        submission.ReviewedByUserId.Should().Be(reviewerId);
        submission.PublishedLyricsId.Should().Be(publishedLyricsId);
    }

    [Fact]
    public void Approve_ShouldRaiseDecidedEventCarryingThePublishedLyricsId()
    {
        // Arrange
        LyricsSubmissionEntity submission = CreatePendingSubmission();
        var publishedLyricsId = Guid.NewGuid();

        // Act
        submission.Approve(Guid.NewGuid(), publishedLyricsId);

        // Assert
        submission
            .DomainEvents.OfType<LyricsSubmissionDecidedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(
                new LyricsSubmissionDecidedEvent(
                    submission.Id,
                    submission.SubmittedByUserId,
                    EnumSubmissionStatus.Approved,
                    null,
                    publishedLyricsId
                )
            );
    }

    #endregion

    #region Reject Tests

    [Fact]
    public void Reject_ShouldSetStatusReviewedByUserIdAndReviewNote()
    {
        // Arrange
        LyricsSubmissionEntity submission = CreatePendingSubmission();
        var reviewerId = Guid.NewGuid();
        const string note = "Duplicate of an existing song.";

        // Act
        submission.Reject(reviewerId, note);

        // Assert
        submission.Status.Should().Be(EnumSubmissionStatus.Rejected);
        submission.ReviewedByUserId.Should().Be(reviewerId);
        submission.ReviewNote.Should().Be(note);
        submission.PublishedLyricsId.Should().BeNull();
    }

    [Fact]
    public void Reject_ShouldRaiseDecidedEventCarryingTheModeratorNote()
    {
        // Arrange
        LyricsSubmissionEntity submission = CreatePendingSubmission();
        const string note = "Duplicate of an existing song.";

        // Act
        submission.Reject(Guid.NewGuid(), note);

        // Assert
        submission
            .DomainEvents.OfType<LyricsSubmissionDecidedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(
                new LyricsSubmissionDecidedEvent(
                    submission.Id,
                    submission.SubmittedByUserId,
                    EnumSubmissionStatus.Rejected,
                    note,
                    null
                )
            );
    }

    #endregion

    #region RequestRevision Tests

    [Fact]
    public void RequestRevision_ShouldSetStatusReviewedByUserIdAndReviewNote()
    {
        // Arrange
        LyricsSubmissionEntity submission = CreatePendingSubmission();
        var reviewerId = Guid.NewGuid();
        const string note = "Please fix the formatting before resubmitting.";

        // Act
        submission.RequestRevision(reviewerId, note);

        // Assert
        submission.Status.Should().Be(EnumSubmissionStatus.NeedsRevision);
        submission.ReviewedByUserId.Should().Be(reviewerId);
        submission.ReviewNote.Should().Be(note);
        submission.PublishedLyricsId.Should().BeNull();
    }

    [Fact]
    public void RequestRevision_ShouldRaiseDecidedEventCarryingTheModeratorNote()
    {
        // Arrange
        LyricsSubmissionEntity submission = CreatePendingSubmission();
        const string note = "Please fix the formatting before resubmitting.";

        // Act
        submission.RequestRevision(Guid.NewGuid(), note);

        // Assert
        submission
            .DomainEvents.OfType<LyricsSubmissionDecidedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(
                new LyricsSubmissionDecidedEvent(
                    submission.Id,
                    submission.SubmittedByUserId,
                    EnumSubmissionStatus.NeedsRevision,
                    note,
                    null
                )
            );
    }

    #endregion

    private static LyricsSubmissionEntity CreatePendingSubmission()
    {
        return LyricsSubmissionEntity.Submit(
            Guid.NewGuid(),
            "Eloko Oyo",
            "Fally Ipupa",
            "Some submitted lyrics text.",
            "fr",
            Guid.NewGuid()
        );
    }
}
