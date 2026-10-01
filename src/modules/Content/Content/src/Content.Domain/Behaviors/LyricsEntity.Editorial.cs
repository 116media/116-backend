using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Editorial behaviour of <see cref="LyricsEntity" />. Its state lives in <c>Entities/LyricsEntity.cs</c>.
/// </summary>
public sealed partial class LyricsEntity
{
    /// <summary>
    /// Renames the song, its performing artist and its slug. Slug uniqueness is the handler's
    /// to enforce.
    /// </summary>
    /// <param name="songTitle">The song title.</param>
    /// <param name="artistName">The performing artist name.</param>
    /// <param name="slug">The URL-safe slug. Uniqueness enforced by handler.</param>
    /// <returns><c>true</c> if any value changed; otherwise <c>false</c>.</returns>
    public bool Retitle(string songTitle, string artistName, string slug)
    {
        if (string.IsNullOrWhiteSpace(value: songTitle))
        {
            throw new ContentRuleException(ContentRuleCodes.SongTitleRequired);
        }

        if (string.IsNullOrWhiteSpace(value: artistName))
        {
            throw new ContentRuleException(ContentRuleCodes.LyricsArtistNameRequired);
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw new ContentRuleException(ContentRuleCodes.LyricsSlugRequired);
        }

        if (SongTitle == songTitle && ArtistName == artistName && Slug == slug)
        {
            return false;
        }

        SongTitle = songTitle;
        ArtistName = artistName;
        Slug = slug;

        return true;
    }

    /// <summary>
    /// Revises the lyrics text and the language it is written in.
    /// </summary>
    /// <param name="lyricsText">The full lyrics text.</param>
    /// <param name="language">ISO 639-1 language code.</param>
    /// <returns><c>true</c> if either value changed; otherwise <c>false</c>.</returns>
    public bool ReviseText(string lyricsText, string language)
    {
        if (string.IsNullOrWhiteSpace(value: lyricsText))
        {
            throw new ContentRuleException(ContentRuleCodes.LyricsTextRequired);
        }

        if (LyricsText == lyricsText && Language == language)
        {
            return false;
        }

        LyricsText = lyricsText;
        Language = language;

        return true;
    }

    /// <summary>
    /// Moves the lyrics page to another category. Existence of the category is checked by the handler.
    /// </summary>
    /// <param name="categoryId">The category this lyrics page belongs to.</param>
    /// <returns><c>true</c> if the category changed; otherwise <c>false</c>.</returns>
    public bool Recategorize(Guid categoryId)
    {
        if (CategoryId == categoryId)
        {
            return false;
        }

        CategoryId = categoryId;

        return true;
    }

    /// <summary>
    /// Links the lyrics page to a video, or clears the link. Existence of the video is checked
    /// by the handler.
    /// </summary>
    /// <param name="videoId">Optional linked video UUID.</param>
    /// <returns><c>true</c> if the link changed; otherwise <c>false</c>.</returns>
    public bool Relink(Guid? videoId)
    {
        if (VideoId == videoId)
        {
            return false;
        }

        VideoId = videoId;

        return true;
    }

    /// <summary>
    /// Assigns — or clears — the commission this lyrics page fulfils.
    /// </summary>
    /// <param name="customerId">The B2B customer who commissioned this page. <c>null</c> for free content.</param>
    /// <param name="orderItemId">The order item this page fulfils. <c>null</c> for free content.</param>
    /// <returns><c>true</c> if either value changed; otherwise <c>false</c>.</returns>
    public bool AssignCommission(Guid? customerId, Guid? orderItemId)
    {
        if (CustomerId == customerId && OrderItemId == orderItemId)
        {
            return false;
        }

        CustomerId = customerId;
        OrderItemId = orderItemId;

        return true;
    }

    /// <summary>
    /// Revises the SEO metadata for this lyrics page, including its structured-data payload.
    /// </summary>
    /// <param name="metaTitle">Optional SEO meta title.</param>
    /// <param name="metaDescription">Optional SEO meta description.</param>
    /// <param name="structuredData">Optional schema.org JSON-LD payload.</param>
    /// <returns><c>true</c> if any value changed; otherwise <c>false</c>.</returns>
    public bool ReviseSeo(string? metaTitle, string? metaDescription, string? structuredData)
    {
        if (MetaTitle == metaTitle && MetaDescription == metaDescription && StructuredData == structuredData)
        {
            return false;
        }

        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        StructuredData = structuredData;

        return true;
    }

    /// <summary>
    /// Updates the song-credit fields. Each parameter is independently optional — passing
    /// null for one field clears only that field, leaving the others untouched.
    /// </summary>
    /// <param name="album">
    /// The album name, or null to clear.
    /// </param>
    /// <param name="releaseYear">
    /// The release year, or null to clear.
    /// </param>
    /// <param name="label">
    /// The record label, or null to clear.
    /// </param>
    /// <param name="songwriter">
    /// The credited songwriter, or null to clear.
    /// </param>
    /// <param name="producer">
    /// The credited producer, or null to clear.
    /// </param>
    public void UpdateMetadata(string? album, short? releaseYear, string? label, string? songwriter, string? producer)
    {
        Album = album;
        ReleaseYear = releaseYear;
        Label = label;
        Songwriter = songwriter;
        Producer = producer;
    }

    /// <summary>
    /// Sets or clears the cover/album art file reference.
    /// </summary>
    /// <param name="coverImageFileId">
    /// The FileEntity ID, or null to clear it.
    /// </param>
    public void SetCoverImageFileId(Guid? coverImageFileId)
    {
        CoverImageFileId = coverImageFileId;
    }

    /// <summary>
    /// Links this record to a claimed artist profile. Coexists with, and does not clear,
    /// the free-text <see cref="ArtistName" /> field.
    /// </summary>
    /// <param name="artistId">The <see cref="ArtistEntity" /> ID to link.</param>
    public void LinkArtist(Guid artistId) => ArtistId = artistId;

    /// <summary>
    /// Clears the artist profile link, reverting display to the plain-text
    /// <see cref="ArtistName" /> only.
    /// </summary>
    public void UnlinkArtist() => ArtistId = null;

    /// <summary>
    /// Links this record to a real album. Coexists with, and does not clear, the free-text
    /// <see cref="Album" /> field.
    /// </summary>
    /// <param name="albumId">The <see cref="AlbumEntity" /> ID to link.</param>
    public void LinkAlbum(Guid albumId) => AlbumId = albumId;

    /// <summary>
    /// Clears the album link, reverting display to the plain-text <see cref="Album" /> only.
    /// </summary>
    public void UnlinkAlbum() => AlbumId = null;

    /// <summary>
    /// Replaces the canonical lyrics text — used when a community correction is accepted.
    /// Distinct from the full <see cref="Update" /> call, which requires re-supplying every field.
    /// </summary>
    public void ReplaceLyricsText(string lyricsText) => LyricsText = lyricsText;

    /// <summary>
    /// Replaces the tag set with the given ids, raising one <see cref="TagGraphChangedEvent" />
    /// per tag that joins or leaves. An identical set writes nothing and raises nothing.
    /// </summary>
    /// <param name="tagIds">The complete tag set this row should carry.</param>
    /// <returns><c>true</c> if the set changed; otherwise <c>false</c>.</returns>
    public bool ReplaceTags(IReadOnlyCollection<Guid> tagIds)
    {
        HashSet<Guid> desired = tagIds.ToHashSet();
        HashSet<Guid> current = Tags.Select(tag => tag.TagId).ToHashSet();

        if (desired.SetEquals(current))
        {
            return false;
        }

        foreach (LyricsTagEntity removed in Tags.Where(tag => !desired.Contains(tag.TagId)).ToList())
        {
            Tags.Remove(removed);
            AddDomainEvent(new TagGraphChangedEvent(TagId: removed.TagId));
        }

        foreach (Guid tagId in desired.Where(id => !current.Contains(id)))
        {
            Tags.Add(LyricsTagEntity.Create(id: Guid.NewGuid(), lyricsId: Id, tagId: tagId));
            AddDomainEvent(new TagGraphChangedEvent(TagId: tagId));
        }

        return true;
    }
}
