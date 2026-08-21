using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Application.Templates.Messages;
using _116.Mailer.Contracts.Domain;

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
    public RenderedEmail Render(EnumEmailTemplate template, IReadOnlyDictionary<string, string> tokens, string culture)
    {
        CultureInfo previous = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = ResolveCulture(culture);

            string name = template.ToString();
            string subject = Substitute(messages.Subject(name), tokens, htmlEncode: false);
            string body = Substitute(messages.Html(name), tokens, htmlEncode: true);
            string htmlBody = messages.LayoutHtml().Replace($"{{{{{ContentToken}}}}}", body);
            string textBody = Substitute(messages.Text(name), tokens, htmlEncode: false);

            EnsureFullyResolved(template, subject);
            EnsureFullyResolved(template, htmlBody);
            EnsureFullyResolved(template, textBody);

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
    /// Throws when any <c>{{placeholder}}</c> survived substitution — a missing
    /// token or a resource typo, both programming errors.
    /// </summary>
    private static void EnsureFullyResolved(EnumEmailTemplate template, string rendered)
    {
        Match leftover = PlaceholderRegex().Match(rendered);

        if (leftover.Success)
        {
            throw new InvalidOperationException(
                $"Template '{template}' rendered with unresolved placeholder '{leftover.Value}'."
            );
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
