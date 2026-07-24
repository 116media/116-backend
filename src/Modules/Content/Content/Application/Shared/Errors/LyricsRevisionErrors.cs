using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Lyrics-text community correction domain error factory providing simple, readable exception
/// creation.
/// Usage: LyricsRevisionErrors.RevisionNotFound(id) or LyricsRevisionErrors.AlreadyVoted()
/// </summary>
public class LyricsRevisionErrors(LyricsRevisionErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validator extensions.
    /// </summary>
    public LyricsRevisionErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when a lyrics revision is not found by its identifier.
    /// </summary>
    public NotFoundException RevisionNotFound(Guid id)
    {
        return new NotFoundException("Lyrics revision", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a user attempts to vote twice on the same lyrics revision. The DB-level
    /// unique <c>(RevisionId, UserId)</c> index is the actual enforcement mechanism — this is
    /// the catchable, user-facing error the handler raises after its own pre-check finds an
    /// existing vote, mirroring <see cref="TranslationErrors.AlreadyVoted" />.
    /// </summary>
    public ConflictException AlreadyVoted()
    {
        return new ConflictException(i18n.AlreadyVoted());
    }

    /// <summary>
    /// Throws when a proposed revision's replacement text is required but not provided.
    /// </summary>
    public BadRequestException ProposedTextRequired()
    {
        return new BadRequestException(i18n.ProposedTextRequired());
    }
}
