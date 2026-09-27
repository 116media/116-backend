using _116.Mailer.Application.Shared.DTOs;
using _116.Mailer.Domain.Entities;

namespace _116.Mailer.Application.Shared.Mappers;

/// <summary>
/// Maps notification entities to their feed DTOs. The mapper owns list
/// mapping so call sites never repeat the projection.
/// </summary>
public static class NotificationMapper
{
    /// <summary>
    /// Maps a notification entity to its feed DTO.
    /// </summary>
    /// <param name="entity">The notification entity to map.</param>
    /// <returns>The mapped DTO.</returns>
    public static NotificationDto ToNotificationDto(this NotificationEntity entity)
    {
        return new NotificationDto(
            Id: entity.Id,
            Type: entity.Type,
            Title: entity.Title,
            Body: entity.Body,
            LinkPath: entity.LinkPath,
            ReadAt: entity.ReadAt,
            CreatedAt: entity.CreatedAt
        );
    }

    /// <summary>
    /// Maps a list of notification entities to their feed DTOs.
    /// </summary>
    /// <param name="entities">The notification entities to map.</param>
    /// <returns>The mapped DTO list.</returns>
    public static IReadOnlyList<NotificationDto> ToNotificationDtoList(this IEnumerable<NotificationEntity> entities)
    {
        return [.. entities.Select(ToNotificationDto)];
    }
}
