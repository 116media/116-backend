using _116.Shared.Application.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace _116.Shared.Application.Extensions;

/// <summary>
/// Extension methods for registering Quartz.NET scheduled jobs.
/// </summary>
public static class QuartzExtension
{
    /// <summary>
    /// Registers a single <see cref="IScheduledJob" /> implementation with Quartz using a cron schedule.
    /// Adds the Quartz hosted service on the first call; subsequent calls add additional jobs
    /// to the same scheduler instance.
    /// </summary>
    /// <typeparam name="TJob">The job implementation type, must implement <see cref="IScheduledJob" />.</typeparam>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="cronExpression">
    /// A valid Quartz cron expression defining the job schedule.
    /// Example: <c>"0 0 * * * ?"</c> runs at the top of every hour.
    /// </param>
    /// <returns>The updated <see cref="IServiceCollection" /> for chaining.</returns>
    public static IServiceCollection AddScheduledJob<TJob>(this IServiceCollection services, string cronExpression)
        where TJob : class, IScheduledJob
    {
        services.AddQuartz(q =>
        {
            var jobKey = new JobKey(typeof(TJob).Name);

            q.AddJob<TJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(opts =>
                opts.ForJob(jobKey).WithIdentity($"{typeof(TJob).Name}-trigger").WithCronSchedule(cronExpression)
            );
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }
}
