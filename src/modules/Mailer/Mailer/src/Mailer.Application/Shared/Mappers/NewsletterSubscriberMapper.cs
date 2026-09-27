using _116.Mailer.Application.Shared.DTOs;
using _116.Mailer.Domain.Entities;

namespace _116.Mailer.Application.Shared.Mappers;

/// <summary>
/// Maps newsletter subscriber entities to their admin-facing DTOs. The mapper
/// owns list mapping so call sites never repeat the projection.
/// </summary>
public static class NewsletterSubscriberMapper
{
    /// <summary>
    /// Maps a subscriber entity to its admin-facing DTO.
    /// </summary>
    /// <param name="entity">The subscriber entity to map.</param>
    /// <returns>The mapped DTO.</returns>
    public static NewsletterSubscriberDto ToNewsletterSubscriberDto(this NewsletterSubscriberEntity entity)
    {
        return new NewsletterSubscriberDto(
            Id: entity.Id,
            Email: entity.Email,
            Status: entity.Status,
            ConfirmedAt: entity.ConfirmedAt,
            UnsubscribedAt: entity.UnsubscribedAt,
            CreatedAt: entity.CreatedAt
        );
    }

    /// <summary>
    /// Maps a list of subscriber entities to their admin-facing DTOs.
    /// </summary>
    /// <param name="entities">The subscriber entities to map.</param>
    /// <returns>The mapped DTO list.</returns>
    public static IReadOnlyList<NewsletterSubscriberDto> ToNewsletterSubscriberDtoList(
        this IEnumerable<NewsletterSubscriberEntity> entities
    )
    {
        return [.. entities.Select(ToNewsletterSubscriberDto)];
    }
}
