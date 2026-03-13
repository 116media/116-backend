using Quartz;

namespace _116.Shared.Application.Jobs;

/// <summary>
/// Marker interface for all scheduled background jobs across modules.
/// Extends Quartz's <see cref="IJob" /> so that the Shared infrastructure can
/// scan and register implementations automatically, while keeping modules
/// decoupled from direct Quartz registration details.
/// </summary>
public interface IScheduledJob : IJob { }
