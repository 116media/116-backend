using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace _116.Identity.Infrastructure.BackgroundJobs;

/// <summary>
/// Replays the Identity module's undispatched domain events.
/// </summary>
/// <param name="scopeFactory">Factory creating the scope each replay batch runs in.</param>
/// <param name="logger">Logger recording replay outcomes and dead rows.</param>
public class IdentityOutboxReplayJob(IServiceScopeFactory scopeFactory, ILogger<IdentityOutboxReplayJob> logger)
    : OutboxReplayJob<IdentityDbContext>(scopeFactory, logger);
