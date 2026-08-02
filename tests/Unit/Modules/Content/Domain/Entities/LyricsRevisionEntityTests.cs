using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="LyricsRevisionEntity"/>.
/// </summary>
public class LyricsRevisionEntityTests
{
    #region Propose Tests

    [Fact]
    public void Propose_WithValidParams_ShouldAssignAllFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lyricsId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const string proposedText = "Corrected lyrics text.";
        const string editSummary = "Fixed a misheard line.";

        // Act
        LyricsRevisionEntity revision = LyricsRevisionEntity.Propose(id, lyricsId, proposedText, editSummary, userId);

        // Assert
        revision.Id.Should().Be(id);
        revision.LyricsId.Should().Be(lyricsId);
        revision.ProposedText.Should().Be(proposedText);
        revision.EditSummary.Should().Be(editSummary);
        revision.ProposedByUserId.Should().Be(userId);
    }

    [Fact]
    public void Propose_ShouldStartInPendingStatus()
    {
        // Act
        LyricsRevisionEntity revision = LyricsRevisionEntity.Propose(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Corrected lyrics text.",
            null,
            Guid.NewGuid()
        );

        // Assert
        revision.Status.Should().Be(EnumRevisionStatus.Pending);
        revision.DecidedByUserId.Should().BeNull();
    }

    [Fact]
    public void Propose_WithNullEditSummary_ShouldAllowNull()
    {
        // Act
        LyricsRevisionEntity revision = LyricsRevisionEntity.Propose(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Corrected lyrics text.",
            null,
            Guid.NewGuid()
        );

        // Assert
        revision.EditSummary.Should().BeNull();
    }

    #endregion

    #region Accept Tests

    [Fact]
    public void Accept_WithModeratorId_ShouldSetStatusAndDecidedByUserId()
    {
        // Arrange
        LyricsRevisionEntity revision = CreatePendingRevision();
        var moderatorId = Guid.NewGuid();

        // Act
        revision.Accept(moderatorId);

        // Assert
        revision.Status.Should().Be(EnumRevisionStatus.Accepted);
        revision.DecidedByUserId.Should().Be(moderatorId);
    }

    [Fact]
    public void Accept_WithNullDecidedByUserId_ShouldAcceptAsAutoAccepted()
    {
        // Arrange
        LyricsRevisionEntity revision = CreatePendingRevision();

        // Act
        revision.Accept(null);

        // Assert
        revision.Status.Should().Be(EnumRevisionStatus.Accepted);
        revision.DecidedByUserId.Should().BeNull();
    }

    #endregion

    #region Reject Tests

    [Fact]
    public void Reject_ShouldSetStatusAndDecidedByUserId()
    {
        // Arrange
        LyricsRevisionEntity revision = CreatePendingRevision();
        var moderatorId = Guid.NewGuid();

        // Act
        revision.Reject(moderatorId);

        // Assert
        revision.Status.Should().Be(EnumRevisionStatus.Rejected);
        revision.DecidedByUserId.Should().Be(moderatorId);
    }

    #endregion

    private static LyricsRevisionEntity CreatePendingRevision()
    {
        return LyricsRevisionEntity.Propose(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Corrected lyrics text.",
            "Fixed a misheard line.",
            Guid.NewGuid()
        );
    }
}
