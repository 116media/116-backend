using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo;

/// <summary>
/// Command for creating a new video draft (step 1 of the video creation flow).
/// The video shell is created with identifiers only; the YouTube ID and thumbnail
/// are attached in subsequent steps.
/// </summary>
/// <param name="CategoryId">The category this video belongs to.</param>
/// <param name="Title">The video display title.</param>
/// <param name="Slug">The URL-safe slug for this video.</param>
/// <param name="AuthorId">The identity user UUID read from JWT claims.</param>
/// <param name="CustomerId">The B2B customer who commissioned this video. Null for free content.</param>
/// <param name="OrderItemId">The order item this video fulfils. Null for free content.</param>
/// <param name="Description">Optional description shown below the video player.</param>
/// <param name="ShootingScheduledAt">Optional scheduled shooting date for pre-booked productions.</param>
public record CreateVideoCommand(
    Guid CategoryId,
    string Title,
    string Slug,
    string AuthorId,
    Guid? CustomerId,
    Guid? OrderItemId,
    string? Description,
    DateTimeOffset? ShootingScheduledAt
) : ICommand<CreateVideoResult>;

/// <summary>
/// Result of the <see cref="CreateVideoCommand" /> containing the newly created video details.
/// </summary>
/// <param name="Video">The created video detail information.</param>
public record CreateVideoResult(VideoDetailDto Video);
