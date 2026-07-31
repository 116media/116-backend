using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when a video's shooting date is scheduled or rescheduled.
/// </summary>
/// <param name="VideoId">The video whose shoot was scheduled.</param>
/// <param name="CustomerId">The paying customer behind the production, or <c>null</c> for free editorial content.</param>
/// <param name="Title">The video's display title at scheduling time.</param>
/// <param name="ShootDate">The scheduled shoot date.</param>
public record VideoShootScheduledEvent(Guid VideoId, Guid? CustomerId, string Title, DateTimeOffset ShootDate)
    : IDomainEvent;
