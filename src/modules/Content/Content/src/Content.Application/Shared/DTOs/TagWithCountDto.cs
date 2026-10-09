namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// How much content of each kind carries a tag. Tags are one flat vocabulary, so what a tag
/// is "for" is told by these numbers rather than by any field on the tag itself.
/// </summary>
/// <param name="Articles">Articles carrying the tag.</param>
/// <param name="Videos">Videos carrying the tag.</param>
/// <param name="Lyrics">Lyrics pages carrying the tag.</param>
/// <param name="Total">The three added together.</param>
public record TagContentCountsDto(int Articles, int Videos, int Lyrics, int Total);

/// <summary>
/// A content tag alongside how much content carries it, returned by the tag listings. The
/// plain <see cref="TagDto" /> stays the shape embedded in article, video and lyrics detail
/// payloads, where counts would cost a query per tag and mean nothing to the reader.
/// </summary>
/// <param name="Id">The unique identifier of the tag.</param>
/// <param name="Name">The display name of the tag.</param>
/// <param name="Slug">The URL-safe slug for the tag.</param>
/// <param name="Counts">
/// The per-kind counts. The public listings count published content only; the admin listing
/// counts every association so an editor sees a tag still used by a draft.
/// </param>
public record TagWithCountDto(Guid Id, string Name, string Slug, TagContentCountsDto Counts);
