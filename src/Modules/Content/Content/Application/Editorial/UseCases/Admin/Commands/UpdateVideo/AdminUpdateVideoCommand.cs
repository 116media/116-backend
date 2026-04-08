using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo;

/// <summary>
/// Command for updating all editable fields of a video.
/// Permitted when the video status is <c>Draft</c>, <c>PendingPayment</c>,
/// <c>PendingReview</c>, or <c>Rejected</c>.
/// </summary>
/// <param name="Id">The unique identifier of the video to update.</param>
/// <param name="CategoryId">The category this video belongs to.</param>
/// <param name="Title">The video display title.</param>
/// <param name="Slug">The URL-safe slug for this video.</param>
/// <param name="Description">The description shown below the video player.</param>
/// <param name="CustomerId">The B2B customer who commissioned this video. <c>null</c> for free content.</param>
/// <param name="OrderItemId">The order item this video fulfils. Required when <c>CustomerId</c> is set.</param>
/// <param name="SocialBoost">Whether to flag this video for manual social media promotion.</param>
/// <param name="IsFeatured">Whether to activate a featured homepage placement.</param>
/// <param name="FeaturedUntil">When the featured placement expires. Required when <c>IsFeatured</c> is true.</param>
/// <param name="MetaTitle">Custom SEO meta title.</param>
/// <param name="MetaDescription">Custom SEO meta description.</param>
public record AdminUpdateVideoCommand(
    string Id,
    Guid CategoryId,
    string Title,
    string Slug,
    string Description,
    Guid? CustomerId,
    Guid? OrderItemId,
    bool SocialBoost,
    bool IsFeatured,
    DateTimeOffset? FeaturedUntil,
    string? MetaTitle,
    string? MetaDescription
) : ICommand<AdminUpdateVideoResult>;

/// <summary>
/// Result of the <see cref="AdminUpdateVideoCommand" /> containing the updated video details.
/// </summary>
/// <param name="Video">The updated video detail information.</param>
public record AdminUpdateVideoResult(VideoDetailDto Video);
