namespace _116.Shared.Application.Configurations.Schemas;

/// <summary>
/// Mail delivery variables.
/// </summary>
public static class MailEnv
{
    // An absent value keeps local development on the Mailer module's SMTP default (Mailpit).
    public static readonly EnvVar<string> Provider = EnvVar.Optional("EMAIL_PROVIDER", @default: "smtp");
}
