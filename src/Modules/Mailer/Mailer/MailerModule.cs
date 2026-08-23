using _116.Mailer.Application.Notifications;
using _116.Mailer.Application.Notifications.Messages;
using _116.Mailer.Application.Shared.Errors;
using _116.Mailer.Application.Shared.Errors.Messages;
using _116.Mailer.Application.Shared.Persistence;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Application.Templates;
using _116.Mailer.Application.Templates.Messages;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Domain.Constants;
using _116.Mailer.Infrastructure.BackgroundJobs;
using _116.Mailer.Infrastructure.Persistence;
using _116.Mailer.Infrastructure.Repositories;
using _116.Mailer.Infrastructure.Services;
using _116.Shared.Application.Configurations;
using _116.Shared.Application.Extensions;
using _116.Shared.Infrastructure;
using Microsoft.AspNetCore.Builder;
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
    private static ModuleOptions<MailerDbContext> GetModuleOptions(IHostEnvironment environment)
    {
        bool enableMigrations = !environment.IsEnvironment("Testing");

        return new ModuleOptions<MailerDbContext>
        {
            ModuleName = MailerConstants.ModuleName,
            SchemaName = MailerConstants.SchemaName,
            EnableMigrations = enableMigrations,
            EnableSeeding = false,
        };
    }

    /// <summary>
    /// Adds the Mailer module's services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="environment">The host environment deciding whether the module migrates at startup.</param>
    /// <returns>The updated <see cref="IServiceCollection" /> for chaining.</returns>
    public static IServiceCollection AddMailerModule(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddModuleDatabase(GetModuleOptions(environment));

        services.AddScoped<NewsletterErrorMessage>();
        services.AddScoped<NewsletterErrors>();
        services.AddScoped<NotificationErrorMessage>();
        services.AddScoped<NotificationErrors>();
        services.AddScoped<EmailTemplateMessage>();
        services.AddScoped<IEmailTemplateRenderer, EmailTemplateRenderer>();
        services.AddScoped<NotificationMessage>();
        services.AddScoped<INotificationRenderer, NotificationRenderer>();

        services.AddScoped<IMailerUnitOfWork, MailerUnitOfWork>();
        services.AddScoped<IOutboxEmailRepository, OutboxEmailRepository>();
        services.AddScoped<INewsletterRepository, NewsletterRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IMailer, OutboxMailer>();
        services.AddScoped<INotifier, Notifier>();

        RegisterEmailSender(services);

        services.AddScheduledJob<OutboxEmailDispatcherJob>(cronExpression: MailerConstants.DispatchCron);

        return services;
    }

    /// <summary>
    /// Registers the <see cref="IEmailSender" /> adapter selected by the
    /// <c>EMAIL_PROVIDER</c> environment variable. Unknown values fail at boot:
    /// a misconfigured provider must be loud, never a silent no-send.
    /// </summary>
    private static void RegisterEmailSender(IServiceCollection services)
    {
        string provider = AppEnvironment.EmailProvider() ?? MailerConstants.EmailProviders.Smtp;

        switch (provider.ToLowerInvariant())
        {
            case MailerConstants.EmailProviders.Smtp:
                services.AddScoped<IEmailSender, SmtpEmailSender>();
                break;
            case MailerConstants.EmailProviders.Resend:
                services
                    .AddHttpClient<IEmailSender, ResendEmailSender>()
                    .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(10));
                break;
            default:
                throw new InvalidOperationException($"Unknown EMAIL_PROVIDER '{provider}'.");
        }
    }

    /// <summary>
    /// Configures the Mailer module's middleware in the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The updated <see cref="IApplicationBuilder" /> for chaining.</returns>
    public static IApplicationBuilder UseMailerModule(this IApplicationBuilder app)
    {
        IHostEnvironment environment = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
        app.UseModuleDatabase(GetModuleOptions(environment));

        return app;
    }
}
