using _116.Mailer.Contracts.Application;

namespace _116.Mailer.Application.Shared.Services;

/// <summary>
/// The rendered parts of a templated email.
/// </summary>
/// <param name="Subject">The rendered subject line.</param>
/// <param name="HtmlBody">The rendered HTML body.</param>
/// <param name="TextBody">The rendered plain-text body.</param>
public record RenderedEmail(string Subject, string HtmlBody, string TextBody);

/// <summary>
/// Produces the fully rendered subject, html and text parts for a template in
/// a given culture.
/// </summary>
public interface IEmailTemplateRenderer
{
    /// <summary>
    /// Renders a template with the given tokens. Throws when a required token
    /// is missing or an unresolved placeholder survives substitution — a
    /// missing token is a programming error, never a runtime state.
    /// </summary>
    /// <param name="template">The template to render, from the catalog.</param>
    /// <param name="tokens">The dynamic values the template requires.</param>
    /// <param name="culture">The two-letter culture (e.g. "en", "fr").</param>
    /// <returns>The rendered subject and bodies.</returns>
    RenderedEmail Render(EnumEmailTemplate template, IReadOnlyDictionary<string, string> tokens, string culture);
}
