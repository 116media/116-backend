using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ScheduleShoot;

/// <summary>
/// Command for scheduling or updating a video's shooting date.
/// Used for pre-booked productions where the client pays before the shoot takes place.
/// </summary>
/// <param name="VideoId">The unique identifier of the video to schedule the shoot for.</param>
/// <param name="ShootingScheduledAt">The scheduled shooting date (must be in the future).</param>
public record AdminScheduleShootCommand(string VideoId, DateTimeOffset ShootingScheduledAt)
    : ICommand<AdminScheduleShootResult>;

/// <summary>
/// Result of the <see cref="AdminScheduleShootCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminScheduleShootResult(bool IsSuccess);
