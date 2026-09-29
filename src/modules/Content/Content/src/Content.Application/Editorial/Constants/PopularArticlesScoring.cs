namespace _116.Content.Application.Editorial.Constants;

/// <summary>
/// Weight constants for the weighted engagement score used to rank popular articles.
/// The score is a linear combination of the article's persisted engagement counters.
/// Weights are ordinal (like &gt; comment &gt; share &gt; bookmark) and are a tunable product
/// decision — retuning here changes the ranking everywhere without touching handler or
/// query-builder logic.
/// </summary>
public static class PopularArticlesScoring
{
    /// <summary>
    /// Weight applied to <c>LikeCount</c>. Highest weight: a like is the most direct
    /// expression of reader approval and the primary popularity signal.
    /// </summary>
    public const int LikeWeight = 4;

    /// <summary>
    /// Weight applied to <c>CommentCount</c>. High weight: commenting is high-effort and
    /// signals discussion around the article.
    /// </summary>
    public const int CommentWeight = 3;

    /// <summary>
    /// Weight applied to <c>ShareCount</c>. Medium weight: a share redistributes the
    /// article to other readers.
    /// </summary>
    public const int ShareWeight = 2;

    /// <summary>
    /// Weight applied to <c>BookmarkCount</c>. Baseline weight: a bookmark is private
    /// intent to return, meaningful but non-amplifying.
    /// </summary>
    public const int BookmarkWeight = 1;
}
