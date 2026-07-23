using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.SubmitLyrics;

/// <summary>
/// Command to submit a new song to the platform. Gated purely by whether the submitter owns a
/// claimed <see cref="ArtistEntity" /> (<see cref="ArtistEntity.UserId" />, never by comparing
/// the submitted artist name text): an owner's song is created directly as a real
/// <see cref="LyricsEntity" />, skipping the moderation queue entirely; anyone else's song is
/// queued as a <see cref="LyricsSubmissionEntity" /> for review.
/// </summary>
/// <param name="SongTitle">The title of the submitted song.</param>
/// <param name="ArtistName">
/// The performing artist name as entered by the submitter. Ignored — and not required — when
/// the submitter owns a claimed artist profile, in which case the owned profile's own name is
/// used instead. Required when they do not.
/// </param>
/// <param name="LyricsText">The full submitted lyrics text.</param>
/// <param name="Language">ISO 639-1 language code of the submitted lyrics.</param>
/// <param name="Slug">
/// The URL-safe slug for the lyrics page. Only used, and only required, on the
/// verified-artist fast path, where a real lyrics record is created immediately. A queued
/// community submission has no slug yet — one is assigned only once an admin approves it.
/// </param>
/// <param name="UserId">The identity user UUID of the submitter, from JWT claims.</param>
public record PublicSubmitLyricsCommand(
    string SongTitle,
    string? ArtistName,
    string LyricsText,
    string Language,
    string? Slug,
    Guid UserId
) : ICommand<PublicSubmitLyricsResult>;

/// <summary>
/// Result of the <see cref="PublicSubmitLyricsCommand" />.
/// </summary>
/// <param name="WentToQueue">
/// <c>true</c> if the submission entered the community moderation queue; <c>false</c> if it
/// was created directly as a lyrics record via the verified-artist fast path.
/// </param>
/// <param name="SubmissionId">
/// The identifier of the queued submission, or null when the verified-artist fast path was
/// used instead.
/// </param>
/// <param name="LyricsId">
/// The identifier of the directly created lyrics record, or null when the submission went to
/// the moderation queue instead.
/// </param>
public record PublicSubmitLyricsResult(bool WentToQueue, Guid? SubmissionId, Guid? LyricsId);
