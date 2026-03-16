using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;

/// <summary>
/// Command for replacing the lyrics text of an existing lyrics page.
/// </summary>
/// <param name="Id">The unique identifier of the lyrics record to update.</param>
/// <param name="LyricsText">The new full lyrics text to replace the existing content.</param>
public record AdminUpdateLyricsCommand(string Id, string LyricsText) : ICommand<AdminUpdateLyricsResult>;

/// <summary>
/// Result of the <see cref="AdminUpdateLyricsCommand" /> containing the updated lyrics details.
/// </summary>
/// <param name="Lyrics">The updated lyrics information.</param>
public record AdminUpdateLyricsResult(LyricsDto Lyrics);
