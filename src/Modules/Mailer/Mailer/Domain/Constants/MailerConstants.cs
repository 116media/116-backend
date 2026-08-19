namespace _116.Mailer.Domain.Constants;

/// <summary>
/// Constants for the Mailer module: module identity, provider names, and the
/// outbox delivery policy.
/// </summary>
public static class MailerConstants
{
    /// <summary>
    /// The module name used for registration and diagnostics.
    /// </summary>
    public const string ModuleName = "Mailer";

    /// <summary>
    /// The PostgreSQL schema owned by the Mailer module.
    /// </summary>
    public const string SchemaName = "mailer";

    /// <summary>
    /// Route scope segment for public endpoints.
    /// </summary>
    public const string Public = "public";

    /// <summary>
    /// Route scope segment for admin endpoints.
    /// </summary>
    public const string Admin = "admin";

    /// <summary>
    /// Route resource segment for newsletter endpoints.
    /// </summary>
    public const string NewsletterRoute = "newsletter";

    /// <summary>
    /// Route resource segment for in-app notification endpoints.
    /// </summary>
    public const string NotificationsRoute = "notifications";

    /// <summary>
    /// The accepted values of the <c>EMAIL_PROVIDER</c> environment variable.
    /// Adding a provider means adding a constant here and a registration branch
    /// in <c>MailerModule</c> — nothing else.
    /// </summary>
    public static class EmailProviders
    {
        /// <summary>
        /// SMTP relay delivery (Mailpit in development, any relay in production).
        /// </summary>
        public const string Smtp = "smtp";

        /// <summary>
        /// Resend HTTP API delivery.
        /// </summary>
        public const string Resend = "resend";
    }

    /// <summary>
    /// Backoff schedule between delivery attempts. The attempt count indexes
    /// into this array; when it runs past the end the email is marked failed.
    /// </summary>
    public static readonly TimeSpan[] RetrySchedule =
    [
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(30),
        TimeSpan.FromHours(2),
        TimeSpan.FromHours(12),
    ];

    /// <summary>
    /// Maximum delivery attempts before an outbox email is marked failed.
    /// </summary>
    public static int MaxAttempts => RetrySchedule.Length;

    /// <summary>
    /// Maximum outbox emails claimed per dispatcher run.
    /// </summary>
    public const int DispatchBatchSize = 20;

    /// <summary>
    /// Quartz cron expression for the outbox dispatcher: every 15 seconds.
    /// </summary>
    public const string DispatchCron = "0/15 * * * * ?";

    /// <summary>
    /// Maximum length persisted for a provider error message on an outbox row.
    /// </summary>
    public const int MaxLastErrorLength = 1000;

    /// <summary>
    /// Byte length of newsletter confirmation and unsubscribe tokens before
    /// url-safe encoding.
    /// </summary>
    public const int NewsletterTokenBytes = 32;
}
