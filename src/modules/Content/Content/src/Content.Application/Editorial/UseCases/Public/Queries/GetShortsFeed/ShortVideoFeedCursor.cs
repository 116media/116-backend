using System.Globalization;
using System.Text;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetShortsFeed;

/// <summary>
/// An opaque, URL-safe cursor for the randomized short-video feed. Carries the shuffle
/// seed plus the last returned item's feed sort key so subsequent pages resume the exact
/// same stable ordering without drift.
/// </summary>
/// <param name="Seed">The shuffle seed shared across the whole pagination session.</param>
/// <param name="AfterKey">The last returned item's feed sort key (<c>FeedRank XOR Seed</c>).</param>
public sealed record ShortVideoFeedCursor(long Seed, long AfterKey)
{
    private const char Separator = '|';

    /// <summary>
    /// Encodes the cursor into a base64url token safe for query strings.
    /// </summary>
    /// <returns>The encoded cursor token.</returns>
    public string Encode()
    {
        string raw = string.Create(CultureInfo.InvariantCulture, $"{Seed}{Separator}{AfterKey}");
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    /// <summary>
    /// Attempts to decode a cursor token. Returns false for null, malformed, or
    /// structurally invalid tokens so the caller can start a fresh feed session.
    /// </summary>
    /// <param name="token">The cursor token to decode.</param>
    /// <param name="cursor">The decoded cursor when successful.</param>
    /// <returns>True when the token decodes to a valid cursor.</returns>
    public static bool TryDecode(string? token, out ShortVideoFeedCursor cursor)
    {
        cursor = null!;

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            string base64 = token.Replace('-', '+').Replace('_', '/');
            base64 = (base64.Length % 4) switch
            {
                2 => base64 + "==",
                3 => base64 + "=",
                _ => base64,
            };

            string raw = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            string[] parts = raw.Split(Separator);

            if (parts.Length != 2)
            {
                return false;
            }

            if (!long.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out long seed))
            {
                return false;
            }

            if (!long.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out long afterKey))
            {
                return false;
            }

            cursor = new ShortVideoFeedCursor(seed, afterKey);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
