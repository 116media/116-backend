using _116.Content.Domain.Enums;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for a single artist social platform link.
/// <para>
/// Carries no identifiers: the client renders the row and follows the URLs, and never
/// addresses a link individually.
/// </para>
/// </summary>
/// <param name="Platform">
/// The social platform this link points to.
/// </param>
/// <param name="Url">
/// The outbound profile URL on that platform. Always https — enforced on write.
/// </param>
public record ArtistSocialLinkDto(EnumSocialPlatform Platform, string Url);
