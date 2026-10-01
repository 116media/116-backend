using _116.Content.Domain.Enums;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Creation behaviour of <see cref="LyricsEntity" />. Its state lives in <c>Entities/LyricsEntity.cs</c>.
/// </summary>
public sealed partial class LyricsEntity
{
    /// <summary>
    /// Creates a new free lyrics page.
    /// </summary>
    /// <param name="id">The unique identifier for the lyrics page.</param>
    /// <param name="categoryId">The category this lyrics page belongs to.</param>
    /// <param name="videoId">Optional linked video UUID.</param>
    /// <param name="songTitle">The song title.</param>
    /// <param name="artistName">The performing artist name.</param>
    /// <param name="lyricsText">The full lyrics text.</param>
    /// <param name="language">ISO 639-1 language code.</param>
    /// <param name="slug">The URL-safe slug.</param>
    /// <param name="authorId">The identity user UUID from JWT claims.</param>
    /// <returns>A new <see cref="LyricsEntity" /> in <c>Draft</c> status.</returns>
    public static LyricsEntity CreateFree(
        Guid id,
        Guid categoryId,
        Guid? videoId,
        string songTitle,
        string artistName,
        string lyricsText,
        string language,
        string slug,
        Guid authorId
    )
    {
        ValidateRequiredFields(songTitle: songTitle, artistName: artistName, lyricsText: lyricsText);

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw new ContentRuleException(ContentRuleCodes.LyricsSlugRequired);
        }

        return new LyricsEntity
        {
            Id = id,
            CategoryId = categoryId,
            VideoId = videoId,
            SongTitle = songTitle,
            ArtistName = artistName,
            LyricsText = lyricsText,
            Language = language,
            Slug = slug,
            AuthorId = authorId,
            Status = EnumContentStatus.Draft,
        };
    }

    /// <summary>
    /// Creates a new paid lyrics page linked to a customer and order item.
    /// Both <paramref name="customerId" /> and <paramref name="orderItemId" /> must be provided together.
    /// </summary>
    /// <param name="id">The unique identifier for the lyrics page.</param>
    /// <param name="customerId">The B2B customer who commissioned this lyrics page.</param>
    /// <param name="orderItemId">The order item this lyrics page fulfils.</param>
    /// <param name="categoryId">The category this lyrics page belongs to.</param>
    /// <param name="videoId">Optional linked video UUID.</param>
    /// <param name="songTitle">The song title.</param>
    /// <param name="artistName">The performing artist name.</param>
    /// <param name="lyricsText">The full lyrics text.</param>
    /// <param name="language">ISO 639-1 language code.</param>
    /// <param name="slug">The URL-safe slug.</param>
    /// <param name="authorId">The identity user UUID from JWT claims.</param>
    /// <returns>A new <see cref="LyricsEntity" /> in <c>Draft</c> status.</returns>
    public static LyricsEntity CreatePaid(
        Guid id,
        Guid customerId,
        Guid orderItemId,
        Guid categoryId,
        Guid? videoId,
        string songTitle,
        string artistName,
        string lyricsText,
        string language,
        string slug,
        Guid authorId
    )
    {
        ValidateRequiredFields(songTitle: songTitle, artistName: artistName, lyricsText: lyricsText);

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw new ContentRuleException(ContentRuleCodes.LyricsSlugRequired);
        }

        return new LyricsEntity
        {
            Id = id,
            CustomerId = customerId,
            OrderItemId = orderItemId,
            CategoryId = categoryId,
            VideoId = videoId,
            SongTitle = songTitle,
            ArtistName = artistName,
            LyricsText = lyricsText,
            Language = language,
            Slug = slug,
            AuthorId = authorId,
            Status = EnumContentStatus.Draft,
        };
    }

    private static void ValidateRequiredFields(string songTitle, string artistName, string lyricsText)
    {
        if (string.IsNullOrWhiteSpace(value: songTitle))
        {
            throw new ContentRuleException(ContentRuleCodes.SongTitleRequired);
        }

        if (string.IsNullOrWhiteSpace(value: artistName))
        {
            throw new ContentRuleException(ContentRuleCodes.LyricsArtistNameRequired);
        }

        if (string.IsNullOrWhiteSpace(value: lyricsText))
        {
            throw new ContentRuleException(ContentRuleCodes.LyricsTextRequired);
        }
    }
}
