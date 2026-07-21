using _116.Content.Domain.Entities;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="LyricsTranslationRevisionEntity"/> instances in tests.
/// For test code, prefer using LyricsTranslationRevisionFactory instead of direct Builder usage.
/// </summary>
internal class LyricsTranslationRevisionBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _translationId = Guid.NewGuid();
    private string _proposedText = "Texto de traducción propuesto.";
    private string? _editSummary = "Fixed a typo.";
    private Guid _proposedByUserId = Guid.NewGuid();
    private bool _accepted;
    private bool _rejected;
    private Guid? _decidedByUserId;

    /// <summary>
    /// Sets the revision ID.
    /// </summary>
    public LyricsTranslationRevisionBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the translation being corrected.
    /// </summary>
    public LyricsTranslationRevisionBuilder WithTranslationId(Guid translationId)
    {
        _translationId = translationId;
        return this;
    }

    /// <summary>
    /// Sets the proposed replacement text.
    /// </summary>
    public LyricsTranslationRevisionBuilder WithProposedText(string proposedText)
    {
        _proposedText = proposedText;
        return this;
    }

    /// <summary>
    /// Sets the optional edit summary.
    /// </summary>
    public LyricsTranslationRevisionBuilder WithEditSummary(string? editSummary)
    {
        _editSummary = editSummary;
        return this;
    }

    /// <summary>
    /// Sets the identity user UUID of the proposer.
    /// </summary>
    public LyricsTranslationRevisionBuilder WithProposedByUserId(Guid userId)
    {
        _proposedByUserId = userId;
        return this;
    }

    /// <summary>
    /// Transitions the revision to <c>Accepted</c>, optionally via a moderator.
    /// </summary>
    public LyricsTranslationRevisionBuilder AsAccepted(Guid? decidedByUserId = null)
    {
        _accepted = true;
        _rejected = false;
        _decidedByUserId = decidedByUserId;
        return this;
    }

    /// <summary>
    /// Transitions the revision to <c>Rejected</c> by the given moderator.
    /// </summary>
    public LyricsTranslationRevisionBuilder AsRejected(Guid decidedByUserId)
    {
        _rejected = true;
        _accepted = false;
        _decidedByUserId = decidedByUserId;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="LyricsTranslationRevisionEntity"/> instance.
    /// </summary>
    public LyricsTranslationRevisionEntity Build()
    {
        LyricsTranslationRevisionEntity entity = LyricsTranslationRevisionEntity.Propose(
            id: _id,
            translationId: _translationId,
            proposedText: _proposedText,
            editSummary: _editSummary,
            userId: _proposedByUserId
        );

        if (_accepted)
        {
            entity.Accept(_decidedByUserId);
        }
        else if (_rejected)
        {
            entity.Reject(_decidedByUserId!.Value);
        }

        entity.CreatedAt = DateTime.UtcNow;

        return entity;
    }
}
