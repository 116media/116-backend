using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Application.Templates.Messages;
using _116.Mailer.Contracts.Domain.Enums;

namespace _116.Mailer.Application.Templates;

/// <summary>
/// Renders email templates from the localized resource catalog by <c>{{token}}</c>
/// substitution. Token values are HTML-encoded into the HTML part and inserted
/// raw into the subject and text parts; any placeholder surviving substitution
/// fails the render, so a missing token or a resource typo can never reach a
/// recipient.
/// </summary>
/// <param name="messages">The localized template resource facade.</param>
public partial class EmailTemplateRenderer(EmailTemplateMessage messages) : IEmailTemplateRenderer
{
    /// <summary>
    /// The token the shared layout replaces with the rendered body.
    /// </summary>
    private const string ContentToken = "content";

    /// <inheritdoc />
    public RenderedEmail Render(string template, IReadOnlyDictionary<string, string> tokens, string locale)
    {
        CultureInfo previous = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = ResolveCulture(locale);

            string name = template;
            string subjectTemplate = messages.Subject(name);
            string htmlTemplate = messages.Html(name);
            string textTemplate = messages.Text(name);

            EnsureEveryPlaceholderHasAToken(template, subjectTemplate, tokens);
            EnsureEveryPlaceholderHasAToken(template, htmlTemplate, tokens);
            EnsureEveryPlaceholderHasAToken(template, textTemplate, tokens);

            string subject = Substitute(subjectTemplate, tokens, htmlEncode: false);
            string body = Substitute(htmlTemplate, tokens, htmlEncode: true);
            string htmlBody = messages.LayoutHtml().Replace($"{{{{{ContentToken}}}}}", body);
            string textBody = Substitute(textTemplate, tokens, htmlEncode: false);

            return new RenderedEmail(subject, htmlBody, textBody);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    /// <summary>
    /// Replaces every <c>{{token}}</c> occurrence in the source with its value.
    /// </summary>
    private static string Substitute(string source, IReadOnlyDictionary<string, string> tokens, bool htmlEncode)
    {
        string result = source;

        foreach ((string token, string value) in tokens)
        {
            string substituted = htmlEncode ? WebUtility.HtmlEncode(value) : value;
            result = result.Replace($"{{{{{token}}}}}", substituted);
        }

        return result;
    }

    /// <summary>
    /// Throws when the template declares a <c>{{placeholder}}</c> no token supplies. Checked
    /// against the template before substitution, so token values containing <c>{{text}}</c>
    /// are delivered literally rather than mistaken for an unresolved placeholder.
    /// </summary>
    private static void EnsureEveryPlaceholderHasAToken(
        string template,
        string source,
        IReadOnlyDictionary<string, string> tokens
    )
    {
        foreach (Match placeholder in PlaceholderRegex().Matches(source))
        {
            string name = placeholder.Value[2..^2];

            if (name != ContentToken && !tokens.ContainsKey(name))
            {
                throw new InvalidOperationException(
                    $"Template '{template}' declares placeholder '{placeholder.Value}' with no token."
                );
            }
        }
    }

    /// <summary>
    /// Maps a two-letter culture to a <see cref="CultureInfo" />, falling back
    /// to the invariant culture (neutral resources) for unknown values.
    /// </summary>
    private static CultureInfo ResolveCulture(string culture)
    {
        try
        {
            return CultureInfo.GetCultureInfo(culture);
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.InvariantCulture;
        }
    }

    [GeneratedRegex(@"\{\{[a-zA-Z0-9]+\}\}")]
    private static partial Regex PlaceholderRegex();
}
