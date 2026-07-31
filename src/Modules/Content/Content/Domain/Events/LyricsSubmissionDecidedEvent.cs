using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when a community lyrics submission is decided — approved into a
/// published lyrics record, rejected outright, or sent back for revision.
/// Carries the moderator's review note so the fan-out can finally put it in
/// front of its addressee.
/// </summary>
/// <param name="SubmissionId">The decided submission.</param>
/// <param name="SubmittedByUserId">The identity user UUID of the submitter.</param>
/// <param name="Outcome">The decision the submission transitioned to.</param>
/// <param name="ReviewNote">The moderator's note explaining a rejection or revision request, or <c>null</c> on approval.</param>
/// <param name="PublishedLyricsId">The lyrics record created from the submission, or <c>null</c> unless approved.</param>
public record LyricsSubmissionDecidedEvent(
    Guid SubmissionId,
    Guid SubmittedByUserId,
    EnumSubmissionStatus Outcome,
    string? ReviewNote,
    Guid? PublishedLyricsId
) : IDomainEvent;
