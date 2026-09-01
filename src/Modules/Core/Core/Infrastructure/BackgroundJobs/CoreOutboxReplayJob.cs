using _116.Core.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace _116.Core.Infrastructure.BackgroundJobs;

/// <summary>
/// Replays the Core module's undispatched domain events.
/// </summary>
/// <param name="scopeFactory">Factory creating the scope each replay batch runs in.</param>
/// <param name="logger">Logger recording replay outcomes and dead rows.</param>
public class CoreOutboxReplayJob(IServiceScopeFactory scopeFactory, ILogger<CoreOutboxReplayJob> logger)
    : OutboxReplayJob<CoreDbContext>(scopeFactory, logger);
