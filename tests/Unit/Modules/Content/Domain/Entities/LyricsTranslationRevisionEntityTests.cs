using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="LyricsTranslationRevisionEntity"/>.
/// </summary>
public class LyricsTranslationRevisionEntityTests
{
    #region Propose Tests

    [Fact]
    public void Propose_WithValidParams_ShouldAssignAllFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var translationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const string proposedText = "Proposed replacement text.";
        const string editSummary = "Fixed a typo.";

        // Act
        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionEntity.Propose(
            id,
            translationId,
            proposedText,
            editSummary,
            userId
        );

        // Assert
        revision.Id.Should().Be(id);
        revision.TranslationId.Should().Be(translationId);
        revision.ProposedText.Should().Be(proposedText);
        revision.EditSummary.Should().Be(editSummary);
        revision.ProposedByUserId.Should().Be(userId);
    }

    [Fact]
    public void Propose_ShouldStartInPendingStatus()
    {
        // Act
        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionEntity.Propose(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Proposed replacement text.",
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
        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionEntity.Propose(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Proposed replacement text.",
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
        LyricsTranslationRevisionEntity revision = CreatePendingRevision();
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
        LyricsTranslationRevisionEntity revision = CreatePendingRevision();

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
        LyricsTranslationRevisionEntity revision = CreatePendingRevision();
        var moderatorId = Guid.NewGuid();

        // Act
        revision.Reject(moderatorId);

        // Assert
        revision.Status.Should().Be(EnumRevisionStatus.Rejected);
        revision.DecidedByUserId.Should().Be(moderatorId);
    }

    #endregion

    private static LyricsTranslationRevisionEntity CreatePendingRevision()
    {
        return LyricsTranslationRevisionEntity.Propose(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Proposed replacement text.",
            "Fixed a typo.",
            Guid.NewGuid()
        );
    }
}
