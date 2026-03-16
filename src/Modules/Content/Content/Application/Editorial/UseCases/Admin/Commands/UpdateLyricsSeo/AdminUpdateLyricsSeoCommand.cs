using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsSeo;

/// <summary>
/// Command for updating the SEO metadata of an existing lyrics page.
/// </summary>
/// <param name="Id">The unique identifier of the lyrics record to update.</param>
/// <param name="MetaTitle">Custom SEO meta title (max 70 chars). Null clears the value.</param>
/// <param name="MetaDescription">Custom SEO meta description (max 160 chars). Null clears the value.</param>
/// <param name="MetaKeywords">SEO meta keywords (comma-separated, max 300 chars). Null clears the value.</param>
/// <param name="StructuredData">Schema.org JSON-LD structured data. Null clears the value.</param>
public record AdminUpdateLyricsSeoCommand(
    string Id,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,
    string? StructuredData
) : ICommand<AdminUpdateLyricsSeoResult>;

/// <summary>
/// Result of the <see cref="AdminUpdateLyricsSeoCommand" /> containing the updated lyrics details.
/// </summary>
/// <param name="Lyrics">The updated lyrics information.</param>
public record AdminUpdateLyricsSeoResult(LyricsDto Lyrics);
