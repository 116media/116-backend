namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for one artist directory card.
/// <para>
/// Carries no artist id — the directory links by slug and the public surface never exposes
/// ids — and no bio: thirty biographies for a grid that renders name and count is wasted
/// payload.
/// </para>
/// </summary>
/// <param name="Name">
/// The artist's display name.
/// </param>
/// <param name="Slug">
/// The URL-safe slug used in the artist's public page URL.
/// </param>
/// <param name="AvatarUrl">
/// The publicly accessible URL of the artist's avatar image, or null when none is uploaded.
/// </param>
/// <param name="IsVerified">
/// Whether this profile has been claimed and verified by the artist's own account.
/// </param>
/// <param name="ContentCount">
/// The artist's total item count across every profile surface, computed by the same
/// predicate that decided the artist is listed at all.
/// </param>
public record ArtistSummaryDto(string Name, string Slug, string? AvatarUrl, bool IsVerified, int ContentCount);
