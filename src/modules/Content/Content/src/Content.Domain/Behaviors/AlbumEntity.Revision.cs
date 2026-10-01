using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Revision behaviour of <see cref="AlbumEntity" />. Its state lives in <c>Entities/AlbumEntity.cs</c>.
/// </summary>
public partial class AlbumEntity
{
    /// <summary>
    /// Renames the album.
    /// </summary>
    /// <param name="name">The album's display name.</param>
    /// <returns><c>true</c> if the name changed; otherwise <c>false</c>.</returns>
    public bool Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw new ContentRuleException(ContentRuleCodes.AlbumNameRequired);
        }

        if (Name == name)
        {
            return false;
        }

        Name = name;
        MarkArtistChanged();

        return true;
    }

    /// <summary>
    /// Sets or clears the cover art file reference.
    /// </summary>
    /// <param name="coverImageFileId">Optional cover art file reference, or null to clear it.</param>
    /// <returns><c>true</c> if the reference changed; otherwise <c>false</c>.</returns>
    public bool SetCoverImage(Guid? coverImageFileId)
    {
        if (CoverImageFileId == coverImageFileId)
        {
            return false;
        }

        CoverImageFileId = coverImageFileId;
        MarkArtistChanged();

        return true;
    }

    /// <summary>
    /// Revises the release facts — year, label and release kind.
    /// </summary>
    /// <param name="releaseYear">The release year, or null to clear it.</param>
    /// <param name="label">The record label, or null to clear it.</param>
    /// <param name="releaseType">What kind of release this is.</param>
    /// <returns><c>true</c> if any value changed; otherwise <c>false</c>.</returns>
    public bool ReviseRelease(short? releaseYear, string? label, EnumReleaseType releaseType)
    {
        if (ReleaseYear == releaseYear && Label == label && ReleaseType == releaseType)
        {
            return false;
        }

        ReleaseYear = releaseYear;
        Label = label;
        ReleaseType = releaseType;
        MarkArtistChanged();

        return true;
    }

    /// <summary>
    /// Records one change notice for the owning artist per unit of work, however many edit
    /// verbs the handler calls. A standalone album has no artist to notify about.
    /// </summary>
    private void MarkArtistChanged()
    {
        if (ArtistId is not Guid artistId || DomainEvents.OfType<ArtistChangedEvent>().Any())
        {
            return;
        }

        AddDomainEvent(new ArtistChangedEvent(ArtistId: artistId));
    }
}
