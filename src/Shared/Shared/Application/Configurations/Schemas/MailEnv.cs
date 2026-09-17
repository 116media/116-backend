namespace _116.Shared.Application.Configurations.Schemas;

/// <summary>
/// Email delivery configuration. Provider-specific credentials are optional here and checked
/// where the provider is selected, so an SMTP deployment never has to set Resend's key.
/// </summary>
public static class MailEnv
{
    // An absent value keeps local development on the Mailer module's SMTP default (Mailpit).
    public static readonly EnvVar<string> Provider = EnvVar.Optional("EMAIL_PROVIDER", @default: "smtp");

    /// <summary>
    /// The address every outgoing email is sent from.
    /// </summary>
    public static readonly EnvVar<string> FromAddress = EnvVar.Required("EMAIL_FROM_ADDRESS");

    /// <summary>
    /// The display name every outgoing email is sent from.
    /// </summary>
    public static readonly EnvVar<string> FromName = EnvVar.Optional("EMAIL_FROM_NAME", @default: "116");

    /// <summary>
    /// The SMTP host; the default is the local Mailpit relay.
    /// </summary>
    public static readonly EnvVar<string> SmtpHost = EnvVar.Optional("SMTP_HOST", @default: "localhost");

    /// <summary>
    /// The SMTP port; the default is the local Mailpit relay.
    /// </summary>
    public static readonly EnvVar<int> SmtpPort = EnvVar.Int("SMTP_PORT", @default: 1025);

    /// <summary>
    /// The SMTP username; an empty value sends unauthenticated.
    /// </summary>
    public static readonly EnvVar<string> SmtpUsername = EnvVar.Optional("SMTP_USERNAME", @default: "");

    /// <summary>
    /// The SMTP password, used only when a username is set.
    /// </summary>
    public static readonly EnvVar<string> SmtpPassword = EnvVar.Optional("SMTP_PASSWORD", @default: "");

    /// <summary>
    /// Whether the SMTP connection upgrades to TLS.
    /// </summary>
    public static readonly EnvVar<bool> SmtpUseStartTls = EnvVar.Bool("SMTP_USE_STARTTLS", @default: false);

    /// <summary>
    /// The Resend API base URL.
    /// </summary>
    public static readonly EnvVar<string> ResendApiUrl = EnvVar.Optional(
        "RESEND_API_URL",
        @default: "https://api.resend.com"
    );

    /// <summary>
    /// The Resend API key; required only when Resend is the selected provider.
    /// </summary>
    public static readonly EnvVar<string?> ResendApiKey = EnvVar.Optional("RESEND_API_KEY");
}
