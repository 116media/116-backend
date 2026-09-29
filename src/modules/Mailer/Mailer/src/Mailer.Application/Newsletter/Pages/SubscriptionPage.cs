using System.Net;
using System.Text;

namespace _116.Mailer.Application.Newsletter.Pages;

/// <summary>
/// Renders the minimal confirmation pages the newsletter emails link to. The page exists so the
/// state change happens on a POST: link scanners and mail previewers follow GETs, not forms.
/// </summary>
public static class SubscriptionPage
{
    /// <summary>
    /// Builds a page whose single button posts the signed token back to the same route.
    /// </summary>
    /// <param name="title">Page heading.</param>
    /// <param name="prompt">Sentence explaining what the button does.</param>
    /// <param name="buttonLabel">Label on the submit button.</param>
    /// <param name="action">The route the form posts to.</param>
    /// <returns>A self-contained HTML document.</returns>
    public static string Render(string title, string prompt, string buttonLabel, string action)
    {
        var html = new StringBuilder();

        html.Append("<!doctype html><html><head><meta charset=\"utf-8\">");
        html.Append("<meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">");
        html.Append("<meta name=\"robots\" content=\"noindex,nofollow\">");
        html.Append($"<title>{WebUtility.HtmlEncode(title)}</title></head><body>");
        html.Append($"<h1>{WebUtility.HtmlEncode(title)}</h1>");
        html.Append($"<p>{WebUtility.HtmlEncode(prompt)}</p>");
        html.Append($"<form method=\"post\" action=\"{WebUtility.HtmlEncode(action)}\">");
        html.Append($"<button type=\"submit\">{WebUtility.HtmlEncode(buttonLabel)}</button>");
        html.Append("</form></body></html>");

        return html.ToString();
    }
}
