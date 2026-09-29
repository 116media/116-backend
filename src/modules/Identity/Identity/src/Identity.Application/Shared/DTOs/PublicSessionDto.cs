using _116.Identity.Domain.Enums;

namespace _116.Identity.Application.Shared.DTOs;

/// <summary>
/// The public projection of one of the user's own sessions: what is needed to recognize the
/// device. The start time is the display timestamp — named <see cref="StartedAt" /> so the
/// audit convention does not re-attach it.
/// </summary>
public record PublicSessionDto(
    Guid Id,
    string? IpAddress,
    string? UserAgent,
    EnumBrowser Browser,
    EnumDevice Device,
    EnumPlatform Platform,
    EnumClient Client,
    DateTime? StartedAt,
    DateTime ExpiresAt,
    bool IsActive,
    bool IsCurrent = false
);
