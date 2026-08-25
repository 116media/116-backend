using _116.Content.Domain.Entities;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="LyricsRevisionEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; LyricsRevisionFactory only names chains three or more tests share.
/// </summary>
public class LyricsRevisionBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _lyricsId = Guid.NewGuid();
    private string _proposedText = "Corrected lyrics text.";
    private string? _editSummary = "Fixed a misheard line.";
    private Guid _proposedByUserId = Guid.NewGuid();

    /// <summary>
    /// Sets the lyrics page being corrected.
    /// </summary>
    public LyricsRevisionBuilder WithLyricsId(Guid lyricsId)
    {
        _lyricsId = lyricsId;
        return this;
    }

    /// <summary>
    /// Sets the proposed replacement text.
    /// </summary>
    public LyricsRevisionBuilder WithProposedText(string proposedText)
    {
        _proposedText = proposedText;
        return this;
    }

    /// <summary>
    /// Sets the identity user UUID of the proposer.
    /// </summary>
    public LyricsRevisionBuilder WithProposedByUserId(Guid userId)
    {
        _proposedByUserId = userId;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="LyricsRevisionEntity"/> instance.
    /// </summary>
    public LyricsRevisionEntity Build()
    {
        LyricsRevisionEntity entity = LyricsRevisionEntity.Propose(
            id: _id,
            lyricsId: _lyricsId,
            proposedText: _proposedText,
            editSummary: _editSummary,
            userId: _proposedByUserId
        );

        entity.CreatedAt = DateTime.UtcNow;

        return entity;
    }
}
