using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetShortsFeed;

/// <summary>
/// An opaque, URL-safe cursor for the randomized short-video feed. Carries the shuffle
/// seed plus the last returned item's keyset position (sort key + id) so subsequent
/// pages resume the exact same stable ordering without drift.
/// </summary>
/// <param name="Seed">The shuffle seed shared across the whole pagination session.</param>
/// <param name="AfterSortKey">The last returned item's feed sort key.</param>
/// <param name="AfterId">The last returned item's id (keyset tie-breaker).</param>
public sealed record ShortVideoFeedCursor(int Seed, string AfterSortKey, Guid AfterId)
{
    private const char Separator = '|';

    /// <summary>
    /// Computes a short video's feed sort key for a seed, identical to the database's
    /// <c>md5(id::text || seed)</c>. Used to build the next cursor from the last returned item
    /// without a second database round-trip.
    /// </summary>
    /// <param name="shortVideoId">The short video id.</param>
    /// <param name="seed">The shuffle seed.</param>
    /// <returns>The 32-character lowercase hex sort key.</returns>
    public static string ComputeSortKey(Guid shortVideoId, int seed)
    {
        string input = shortVideoId.ToString("D") + seed.ToString(CultureInfo.InvariantCulture);
        byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(hash);
    }

    /// <summary>
    /// Encodes the cursor into a base64url token safe for query strings.
    /// </summary>
    /// <returns>The encoded cursor token.</returns>
    public string Encode()
    {
        string raw = string.Create(
            CultureInfo.InvariantCulture,
            $"{Seed}{Separator}{AfterSortKey}{Separator}{AfterId}"
        );
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

            if (parts.Length != 3)
            {
                return false;
            }

            if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int seed))
            {
                return false;
            }

            if (string.IsNullOrEmpty(parts[1]) || !Guid.TryParse(parts[2], out Guid afterId))
            {
                return false;
            }

            cursor = new ShortVideoFeedCursor(seed, parts[1], afterId);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
