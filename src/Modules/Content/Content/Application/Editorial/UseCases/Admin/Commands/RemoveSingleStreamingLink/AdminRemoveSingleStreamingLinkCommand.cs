using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RemoveSingleStreamingLink;

/// <summary>
/// Command to remove a standalone single's curated streaming link for a single platform,
/// reverting that platform's public link back to the generated search-query fallback. A no-op
/// if no curated link exists for the given lyrics page and platform.
/// </summary>
/// <param name="LyricsId">The standalone single (lyrics page) this link belongs to.</param>
/// <param name="Platform">The streaming platform whose curated link is being removed.</param>
public record AdminRemoveSingleStreamingLinkCommand(Guid LyricsId, EnumStreamingPlatform Platform)
    : ICommand<AdminRemoveSingleStreamingLinkResult>;

/// <summary>
/// Result of the <see cref="AdminRemoveSingleStreamingLinkCommand" /> indicating whether a curated link was removed.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation completed successfully.</param>
public record AdminRemoveSingleStreamingLinkResult(bool IsSuccess);
