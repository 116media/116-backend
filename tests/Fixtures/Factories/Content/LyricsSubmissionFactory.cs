using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="LyricsSubmissionEntity"/> instances in tests.
/// </summary>
public static class LyricsSubmissionFactory
{
    /// <summary>
    /// Creates a pending community lyrics submission with default valid values.
    /// </summary>
    public static LyricsSubmissionEntity Create() => new LyricsSubmissionBuilder().Build();

    /// <summary>
    /// Creates a pending community lyrics submission with a specific ID.
    /// </summary>
    public static LyricsSubmissionEntity CreateWithId(Guid id) => new LyricsSubmissionBuilder().WithId(id).Build();

    /// <summary>
    /// Creates a pending community lyrics submission submitted by a specific user.
    /// </summary>
    public static LyricsSubmissionEntity Create(Guid submittedByUserId) =>
        new LyricsSubmissionBuilder().WithSubmittedByUserId(submittedByUserId).Build();

    /// <summary>
    /// Creates a pending community lyrics submission with specific song title and artist name.
    /// </summary>
    public static LyricsSubmissionEntity Create(string songTitle, string artistName) =>
        new LyricsSubmissionBuilder().WithSongTitle(songTitle).WithArtistName(artistName).Build();

    /// <summary>
    /// Creates a lyrics submission already approved and linked to a published lyrics record.
    /// </summary>
    public static LyricsSubmissionEntity CreateApproved(Guid reviewedByUserId, Guid publishedLyricsId) =>
        new LyricsSubmissionBuilder().AsApproved(reviewedByUserId, publishedLyricsId).Build();

    /// <summary>
    /// Creates a lyrics submission already rejected with a note.
    /// </summary>
    public static LyricsSubmissionEntity CreateRejected(Guid reviewedByUserId, string note = "Not a good fit.") =>
        new LyricsSubmissionBuilder().AsRejected(reviewedByUserId, note).Build();

    /// <summary>
    /// Creates a lyrics submission already sent back for revision with a note.
    /// </summary>
    public static LyricsSubmissionEntity CreateNeedsRevision(
        Guid reviewedByUserId,
        string note = "Please fix formatting."
    ) => new LyricsSubmissionBuilder().AsNeedsRevision(reviewedByUserId, note).Build();
}
