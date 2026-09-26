using _116.Mailer.Application.Newsletter.Messages;
using _116.Mailer.Application.Newsletter.OutboundEmails;
using _116.Mailer.Application.Notifications;
using _116.Mailer.Application.Notifications.Messages;
using _116.Mailer.Application.Shared.Errors;
using _116.Mailer.Application.Shared.Errors.Messages;
using _116.Mailer.Application.Shared.Persistence;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Application.Templates;
using _116.Mailer.Application.Templates.Messages;
using _116.Mailer.Contracts.Application.OutboundEmails;
using _116.Mailer.Contracts.Application.Services;
using _116.Mailer.Domain.Constants;
using _116.Mailer.Infrastructure.BackgroundJobs;
using _116.Mailer.Infrastructure.Persistence;
using _116.Mailer.Infrastructure.Repositories;
using _116.Mailer.Infrastructure.Services;
using _116.Shared.Application.Configurations.Schemas;
using _116.Shared.Application.Extensions;
using _116.Shared.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace _116.Mailer;

/// <summary>
/// Provides extension methods to register and configure the Mailer module's services and middleware.
/// </summary>
public static class MailerModule
{
    /// <summary>
    /// Gets the shared module configuration options for the Mailer module.
    /// Migrations run in every environment except Testing; the module owns no seeders.
    /// </summary>
    /// <param name="environment">The host environment the options are derived from.</param>
    /// <returns>The module options for the supplied environment.</returns>
    private static ModuleOptions<MailerDbContext> GetModuleOptions() =>
        new() { ModuleName = MailerConstants.ModuleName, SchemaName = MailerConstants.SchemaName };

    /// <summary>
    /// Adds the Mailer module's services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="environment">The host environment deciding whether the module migrates at startup.</param>
    /// <returns>The updated <see cref="IServiceCollection" /> for chaining.</returns>
    public static IServiceCollection AddMailerModule(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddModuleDatabase(GetModuleOptions());

        services.AddScoped<NewsletterErrorMessage>();
        services.AddScoped<NewsletterPageMessage>();
        services.AddScoped<NewsletterErrors>();
        services.AddScoped<NotificationErrorMessage>();
        services.AddScoped<NotificationErrors>();
        services.AddScoped<EmailTemplateMessage>();
        services.AddScoped<IEmailTemplateRenderer, EmailTemplateRenderer>();
        services.AddScoped<NotificationMessage>();
        services.AddScoped<INotificationRenderer, NotificationRenderer>();

        services.AddScoped<IMailerUnitOfWork, MailerUnitOfWork>();
        services.AddScoped(typeof(IMailerRepository<>), typeof(MailerRepository<>));

        // Replay delivers events raised inside a transaction, not just retries failed dispatches.
        services.AddScheduledJob<MailerOutboxReplayJob>(cronExpression: "0 */1 * * * ?");
        services.AddScoped<IOutboxEmailRepository, OutboxEmailRepository>();
        services.AddScoped<INewsletterRepository, NewsletterRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IEmailService, OutboxEmailService>();
        services.AddScoped<IEmailDispatcher, EmailDispatcher>();
        services.AddScoped<INotificationService, NotificationService>();

        RegisterEmailSender(services);

        services.AddScheduledJob<OutboxEmailDispatcherJob>(cronExpression: MailerConstants.DispatchCron);

        return services;
    }

    /// <summary>
    /// Registers the <see cref="IEmailSenderService" /> adapter selected by the
    /// <c>EMAIL_PROVIDER</c> environment variable. Unknown values fail at boot:
    /// a misconfigured provider must be loud, never a silent no-send.
    /// </summary>
    private static void RegisterEmailSender(IServiceCollection services)
    {
        string provider = MailEnv.Provider.Value;

        switch (provider.ToLowerInvariant())
        {
            case MailerConstants.EmailProviders.Smtp:
                services.AddScoped<IEmailSenderService, SmtpEmailSenderService>();
                break;
            case MailerConstants.EmailProviders.Resend:
                // Checked here rather than in the env schema, so an SMTP deployment never has
                // to supply a Resend key; a Resend deployment still fails at boot without one.
                if (string.IsNullOrWhiteSpace(MailEnv.ResendApiKey.Value))
                {
                    throw new InvalidOperationException("RESEND_API_KEY is required when EMAIL_PROVIDER is 'resend'.");
                }

                services
                    .AddHttpClient<IEmailSenderService, ResendEmailSenderService>()
                    .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(10));
                break;
            default:
                throw new InvalidOperationException($"Unknown EMAIL_PROVIDER '{provider}'.");
        }
    }
}
