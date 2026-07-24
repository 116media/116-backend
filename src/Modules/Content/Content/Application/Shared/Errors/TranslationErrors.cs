using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Lyrics translation and community review domain error factory providing simple, readable
/// exception creation.
/// Usage: TranslationErrors.NotFound(id) or TranslationErrors.AlreadyVoted()
/// </summary>
public class TranslationErrors(TranslationErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validator extensions.
    /// </summary>
    public TranslationErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when a lyrics translation is not found by its identifier.
    /// </summary>
    public NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("Translation", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a translation revision is not found by its identifier.
    /// </summary>
    public NotFoundException RevisionNotFound(Guid id)
    {
        return new NotFoundException("Translation revision", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a user attempts to vote twice on the same translation revision. The DB-level
    /// unique <c>(RevisionId, UserId)</c> index is the actual enforcement mechanism — this is
    /// the catchable, user-facing error the handler raises after its own pre-check finds an
    /// existing vote, mirroring how <see cref="LyricsErrors.SlugAlreadyExists" /> is backed by a
    /// pre-check against the real uniqueness constraint elsewhere in this module.
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
