using _116.BuildingBlocks.Infrastructure.Outbox;
using _116.Storage.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace _116.Storage.Infrastructure.BackgroundJobs;

/// <summary>
/// Replays the Storage module's undispatched domain events.
/// </summary>
/// <param name="scopeFactory">Factory creating the scope each replay batch runs in.</param>
/// <param name="logger">Logger recording replay outcomes and dead rows.</param>
public class StorageOutboxReplayJob(IServiceScopeFactory scopeFactory, ILogger<StorageOutboxReplayJob> logger)
    : OutboxReplayJob<StorageDbContext>(scopeFactory, logger);
