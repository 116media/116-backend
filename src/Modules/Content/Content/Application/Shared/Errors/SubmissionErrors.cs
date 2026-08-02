using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Community lyrics submission domain error factory providing simple, readable exception
/// creation.
/// Usage: SubmissionErrors.NotFound(id) or SubmissionErrors.NotPending()
/// </summary>
public class SubmissionErrors(SubmissionErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validator extensions.
    /// </summary>
    public SubmissionErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when a lyrics submission is not found by its identifier.
    /// </summary>
    public NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("LyricsSubmission", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a moderator attempts to approve, reject, or request a revision on a
    /// submission that has already left the <c>Pending</c> status.
    /// </summary>
    public ConflictException NotPending()
    {
        return new ConflictException(i18n.NotPending());
    }
}
