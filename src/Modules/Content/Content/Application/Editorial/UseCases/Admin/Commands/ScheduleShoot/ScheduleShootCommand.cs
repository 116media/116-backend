using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ScheduleShoot;

/// <summary>
/// Command for scheduling or updating a video's shooting date.
/// Used for pre-booked productions where the client pays before the shoot takes place.
/// </summary>
/// <param name="VideoId">The unique identifier of the video to schedule the shoot for.</param>
/// <param name="ShootingScheduledAt">The scheduled shooting date (must be in the future).</param>
public record ScheduleShootCommand(string VideoId, DateTimeOffset ShootingScheduledAt) : ICommand;
