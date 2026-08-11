using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="LyricsSubmissionEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; LyricsSubmissionFactory only names chains three or more tests share.
/// </summary>
public class LyricsSubmissionBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _songTitle = TestConstants.Lyrics.ValidSongTitle;
    private string _artistName = TestConstants.Lyrics.ValidArtistName;
    private string _lyricsText = TestConstants.Lyrics.ValidLyricsText;
    private string _language = TestConstants.Lyrics.ValidLanguage;
    private Guid _submittedByUserId = Guid.NewGuid();
    private bool _approved;
    private bool _rejected;
    private bool _needsRevision;
    private Guid _reviewedByUserId;
    private string? _reviewNote;
    private Guid? _publishedLyricsId;

    /// <summary>
    /// Sets the submitted song title.
    /// </summary>
    public LyricsSubmissionBuilder WithSongTitle(string songTitle)
    {
        _songTitle = songTitle;
        return this;
    }

    /// <summary>
    /// Sets the submitted performing artist name.
    /// </summary>
    public LyricsSubmissionBuilder WithArtistName(string artistName)
    {
        _artistName = artistName;
        return this;
    }

    /// <summary>
    /// Sets the identity user UUID of the submitter.
    /// </summary>
    public LyricsSubmissionBuilder WithSubmittedByUserId(Guid userId)
    {
        _submittedByUserId = userId;
        return this;
    }

    /// <summary>
    /// Transitions the submission to <c>Approved</c>, linked to the given published lyrics record.
    /// </summary>
    public LyricsSubmissionBuilder AsApproved(Guid reviewedByUserId, Guid publishedLyricsId)
    {
        _approved = true;
        _rejected = false;
        _needsRevision = false;
        _reviewedByUserId = reviewedByUserId;
        _publishedLyricsId = publishedLyricsId;
        return this;
    }

    /// <summary>
    /// Transitions the submission to <c>Rejected</c> with a mandatory note.
    /// </summary>
    public LyricsSubmissionBuilder AsRejected(Guid reviewedByUserId, string note)
    {
        _rejected = true;
        _approved = false;
        _needsRevision = false;
        _reviewedByUserId = reviewedByUserId;
        _reviewNote = note;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="LyricsSubmissionEntity"/> instance.
    /// </summary>
    public LyricsSubmissionEntity Build()
    {
        LyricsSubmissionEntity entity = LyricsSubmissionEntity.Submit(
            id: _id,
            songTitle: _songTitle,
            artistName: _artistName,
            lyricsText: _lyricsText,
            language: _language,
            userId: _submittedByUserId
        );

        if (_approved)
        {
            entity.Approve(_reviewedByUserId, _publishedLyricsId!.Value);
        }
        else if (_rejected)
        {
            entity.Reject(_reviewedByUserId, _reviewNote!);
        }
        else if (_needsRevision)
        {
            entity.RequestRevision(_reviewedByUserId, _reviewNote!);
        }

        entity.CreatedAt = DateTime.UtcNow;

        return entity;
    }
}
