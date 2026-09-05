using System.Reflection;
using _116.Content.Domain.Entities;

namespace _116.Tests.Fixtures.Helpers;

/// <summary>
/// Test-only arrangement for the denormalized engagement counters. Production applies these
/// set-based in SQL, so the entities expose no in-memory mutator; these setters state the count a
/// test needs rather than replaying the increments that produced it.
/// </summary>
public static class EngagementCounterExtensions
{
    /// <summary>
    /// Writes a counter property directly, rejecting negatives the SQL clamp could never produce.
    /// </summary>
    /// <param name="entity">The entity carrying the counter.</param>
    /// <param name="property">The counter property name.</param>
    /// <param name="count">The count to arrange.</param>
    private static void Set(object entity, string property, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        PropertyInfo info = entity.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance)!;
        info.SetValue(entity, count);
    }

    /// <summary>
    /// Arranges an article's like count.
    /// </summary>
    public static ArticleEntity WithLikeCount(this ArticleEntity article, int count)
    {
        Set(article, nameof(article.LikeCount), count);
        return article;
    }

    /// <summary>
    /// Arranges an article's comment count.
    /// </summary>
    public static ArticleEntity WithCommentCount(this ArticleEntity article, int count)
    {
        Set(article, nameof(article.CommentCount), count);
        return article;
    }

    /// <summary>
    /// Arranges an article's share count.
    /// </summary>
    public static ArticleEntity WithShareCount(this ArticleEntity article, int count)
    {
        Set(article, nameof(article.ShareCount), count);
        return article;
    }

    /// <summary>
    /// Arranges an article's bookmark count.
    /// </summary>
    public static ArticleEntity WithBookmarkCount(this ArticleEntity article, int count)
    {
        Set(article, nameof(article.BookmarkCount), count);
        return article;
    }

    /// <summary>
    /// Arranges a comment's like count.
    /// </summary>
    public static ArticleCommentEntity WithLikeCount(this ArticleCommentEntity comment, int count)
    {
        Set(comment, nameof(comment.LikeCount), count);
        return comment;
    }

    /// <summary>
    /// Arranges a lyrics page's view count.
    /// </summary>
    public static LyricsEntity WithViewCount(this LyricsEntity lyrics, int count)
    {
        Set(lyrics, nameof(lyrics.ViewCount), count);
        return lyrics;
    }

    /// <summary>
    /// Arranges a lyrics page's like count.
    /// </summary>
    public static LyricsEntity WithLikeCount(this LyricsEntity lyrics, int count)
    {
        Set(lyrics, nameof(lyrics.LikeCount), count);
        return lyrics;
    }

    /// <summary>
    /// Arranges a lyrics page's share count.
    /// </summary>
    public static LyricsEntity WithShareCount(this LyricsEntity lyrics, int count)
    {
        Set(lyrics, nameof(lyrics.ShareCount), count);
        return lyrics;
    }

    /// <summary>
    /// Arranges a video's share count.
    /// </summary>
    public static VideoEntity WithShareCount(this VideoEntity video, int count)
    {
        Set(video, nameof(video.ShareCount), count);
        return video;
    }

    /// <summary>
    /// Arranges a short video's view count.
    /// </summary>
    public static ShortVideoEntity WithViewCount(this ShortVideoEntity shortVideo, int count)
    {
        Set(shortVideo, nameof(shortVideo.ViewCount), count);
        return shortVideo;
    }

    /// <summary>
    /// Arranges a short video's like count.
    /// </summary>
    public static ShortVideoEntity WithLikeCount(this ShortVideoEntity shortVideo, int count)
    {
        Set(shortVideo, nameof(shortVideo.LikeCount), count);
        return shortVideo;
    }

    /// <summary>
    /// Arranges a short video's share count.
    /// </summary>
    public static ShortVideoEntity WithShareCount(this ShortVideoEntity shortVideo, int count)
    {
        Set(shortVideo, nameof(shortVideo.ShareCount), count);
        return shortVideo;
    }

    /// <summary>
    /// Arranges a short video's bookmark count.
    /// </summary>
    public static ShortVideoEntity WithBookmarkCount(this ShortVideoEntity shortVideo, int count)
    {
        Set(shortVideo, nameof(shortVideo.BookmarkCount), count);
        return shortVideo;
    }
}
