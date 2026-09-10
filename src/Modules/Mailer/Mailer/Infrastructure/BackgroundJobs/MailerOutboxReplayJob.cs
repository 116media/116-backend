using _116.Mailer.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace _116.Mailer.Infrastructure.BackgroundJobs;

/// <summary>
/// Replays the Mailer module's undispatched domain events.
/// </summary>
/// <param name="scopeFactory">Factory creating the scope each replay batch runs in.</param>
/// <param name="logger">Logger recording replay outcomes and dead rows.</param>
public class MailerOutboxReplayJob(IServiceScopeFactory scopeFactory, ILogger<MailerOutboxReplayJob> logger)
    : OutboxReplayJob<MailerDbContext>(scopeFactory, logger);
