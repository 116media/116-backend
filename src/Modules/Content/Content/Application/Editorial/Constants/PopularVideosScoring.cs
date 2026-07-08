namespace _116.Content.Application.Editorial.Constants;

/// <summary>
/// Weight constants for the weighted engagement score used to rank popular videos.
/// Unlike articles, videos persist only three native engagement signals —
/// <c>RatingCount</c>, <c>RatingAverage</c>, and <c>ShareCount</c> — so the score combines
/// a quality-weighted rating-volume term with a share term. YouTube's own view/like/comment
/// figures are deliberately excluded: they are fetched client-side, drift, and are not the
/// platform's own engagement. Weights are a tunable product decision — retuning here changes
/// the ranking everywhere without touching handler or query-builder logic.
/// </summary>
public static class PopularVideosScoring
{
    /// <summary>
    /// Weight applied to the rating-volume term <c>RatingCount * RatingAverage</c> (the
    /// approximate total accumulated stars). Primary signal: it rewards how many people
    /// rated and how highly, together, so a broadly well-rated video outranks a rarely-rated
    /// one with a lucky perfect average.
    /// </summary>
    public const int RatingWeight = 3;

    /// <summary>
    /// Weight applied to <c>ShareCount</c>. Amplification signal: a share redistributes the
    /// video to other viewers. Weighted higher per unit than a single star because a share is
    /// a rarer, higher-intent action than a rating.
    /// </summary>
    public const int ShareWeight = 5;
}
