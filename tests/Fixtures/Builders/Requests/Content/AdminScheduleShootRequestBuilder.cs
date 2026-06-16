using _116.Content.Application.Editorial.UseCases.Admin.Commands.ScheduleShoot.V1;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminScheduleShootRequest"/> instances in tests
/// with a valid default future shooting date that satisfies the schedule shoot validator.
/// </summary>
public class AdminScheduleShootRequestBuilder
{
    private DateTimeOffset _shootingScheduledAt;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminScheduleShootRequestBuilder"/> class
    /// with a shooting date set in the future to satisfy the validator.
    /// </summary>
    public AdminScheduleShootRequestBuilder()
    {
        _shootingScheduledAt = DateTimeOffset.UtcNow.AddDays(7);
    }

    /// <summary>
    /// Sets the shooting scheduled date.
    /// </summary>
    /// <param name="shootingScheduledAt">The shooting scheduled date.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminScheduleShootRequestBuilder WithShootingScheduledAt(DateTimeOffset shootingScheduledAt)
    {
        _shootingScheduledAt = shootingScheduledAt;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminScheduleShootRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminScheduleShootRequest instance.</returns>
    public AdminScheduleShootRequest Build()
    {
        return new AdminScheduleShootRequest(ShootingScheduledAt: _shootingScheduledAt);
    }
}
