using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo;

/// <summary>
/// Command for updating a video's SEO metadata.
/// </summary>
/// <param name="Id">The unique identifier of the video to update.</param>
/// <param name="MetaTitle">Custom SEO meta title (max 70 chars). Falls back to video title if null.</param>
/// <param name="MetaDescription">Custom SEO meta description (max 160 chars).</param>
public record UpdateVideoSeoCommand(string Id, string? MetaTitle, string? MetaDescription)
    : ICommand<UpdateVideoSeoResult>;

/// <summary>
/// Result of the <see cref="UpdateVideoSeoCommand" /> containing the updated video details.
/// </summary>
/// <param name="Video">The updated video detail information.</param>
public record UpdateVideoSeoResult(VideoDetailDto Video);
