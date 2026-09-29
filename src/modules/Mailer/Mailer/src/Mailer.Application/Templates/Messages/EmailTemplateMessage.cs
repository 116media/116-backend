using Microsoft.Extensions.Localization;

namespace _116.Mailer.Application.Templates.Messages;

/// <summary>
/// Localizer facade for email template resources. Keys follow the
/// <c>&lt;Template&gt;Subject</c> / <c>&lt;Template&gt;Html</c> /
/// <c>&lt;Template&gt;Text</c> convention, plus the shared
/// <c>LayoutHtml</c> wrapper; the renderer addresses them by template name
/// rather than through per-template methods, because the catalog is uniform
/// by construction.
/// </summary>
/// <param name="localizer">The string localizer bound to this resource set.</param>
public class EmailTemplateMessage(IStringLocalizer<EmailTemplateMessage> localizer)
{
    /// <summary>
    /// Gets the localized subject line source for a template.
    /// </summary>
    /// <param name="template">The template catalog name.</param>
    /// <returns>The subject resource with unresolved tokens.</returns>
    public string Subject(string template)
    {
        return localizer[$"{template}Subject"];
    }

    /// <summary>
    /// Gets the localized HTML body source for a template.
    /// </summary>
    /// <param name="template">The template catalog name.</param>
    /// <returns>The HTML body resource with unresolved tokens.</returns>
    public string Html(string template)
    {
        return localizer[$"{template}Html"];
    }

    /// <summary>
    /// Gets the localized plain-text body source for a template.
    /// </summary>
    /// <param name="template">The template catalog name.</param>
    /// <returns>The text body resource with unresolved tokens.</returns>
    public string Text(string template)
    {
        return localizer[$"{template}Text"];
    }

    /// <summary>
    /// Gets the shared HTML layout that wraps every rendered HTML body via its
    /// <c>{{content}}</c> token.
    /// </summary>
    /// <returns>The layout resource.</returns>
    public string LayoutHtml()
    {
        return localizer["LayoutHtml"];
    }
}
