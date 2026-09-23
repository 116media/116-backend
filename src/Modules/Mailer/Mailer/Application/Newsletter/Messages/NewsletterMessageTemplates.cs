namespace _116.Mailer.Application.Newsletter.Messages;

/// <summary>
/// The message templates this module sends. Owning the names here keeps the module off a
/// shared cross-module enum.
/// </summary>
public static class NewsletterMessageTemplates
{
    /// <summary>
    /// The NewsletterConfirm template name, resolved from this module's resources.
    /// </summary>
    public const string NewsletterConfirm = "NewsletterConfirm";

    /// <summary>
    /// The NewsletterWelcome template name, resolved from this module's resources.
    /// </summary>
    public const string NewsletterWelcome = "NewsletterWelcome";
}
