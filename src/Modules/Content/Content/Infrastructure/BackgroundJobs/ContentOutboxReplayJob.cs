using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace _116.Content.Infrastructure.BackgroundJobs;

/// <summary>
/// Replays the Content module's undispatched domain events.
/// </summary>
/// <param name="scopeFactory">Factory creating the scope each replay batch runs in.</param>
/// <param name="logger">Logger recording replay outcomes and dead rows.</param>
public class ContentOutboxReplayJob(IServiceScopeFactory scopeFactory, ILogger<ContentOutboxReplayJob> logger)
    : OutboxReplayJob<ContentDbContext>(scopeFactory, logger);
