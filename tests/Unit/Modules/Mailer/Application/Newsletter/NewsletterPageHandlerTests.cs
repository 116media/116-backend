using System.Net;
using _116.Mailer.Application.Newsletter.Messages;
using _116.Mailer.Application.Newsletter.UseCases.Public.Queries.GetNewsletterConfirmPage;
using _116.Mailer.Application.Newsletter.UseCases.Public.Queries.GetNewsletterUnsubscribePage;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Application.Newsletter;

/// <summary>
/// Unit tests for the two newsletter page handlers against the real resource catalog: the
/// rendered document carries the localized copy, posts the token back, and leaves no
/// placeholder or unencoded user input behind.
/// </summary>
public class NewsletterPageHandlerTests
{
    private const string Action = "/api/v1/public/newsletter/confirm/tok-123";

    private static PublicGetNewsletterConfirmPageHandler ConfirmHandler =>
        new(LocalizerFactory.CreateMessage<NewsletterPageMessage>());

    private static PublicGetNewsletterUnsubscribePageHandler UnsubscribeHandler =>
        new(LocalizerFactory.CreateMessage<NewsletterPageMessage>());

    [Fact]
    public async Task ConfirmHandler_ShouldRenderAFormPostingBackToTheAction()
    {
        PublicGetNewsletterConfirmPageResult result = await ConfirmHandler.Handle(
            new PublicGetNewsletterConfirmPageQuery(Action),
            CancellationToken.None
        );

        result.Html.Should().Contain($"<form method=\"post\" action=\"{Action}\">");
        result.Html.Should().StartWith("<!doctype html>").And.EndWith("</html>");
    }

    [Fact]
    public async Task ConfirmHandler_ShouldRenderTheLocalizedCopy()
    {
        var page = LocalizerFactory.CreateMessage<NewsletterPageMessage>();

        PublicGetNewsletterConfirmPageResult result = await ConfirmHandler.Handle(
            new PublicGetNewsletterConfirmPageQuery(Action),
            CancellationToken.None
        );

        result.Html.Should().Contain(WebUtility.HtmlEncode(page.ConfirmTitle()));
        result.Html.Should().Contain(WebUtility.HtmlEncode(page.ConfirmPrompt()));
        result.Html.Should().Contain(WebUtility.HtmlEncode(page.ConfirmButton()));
    }

    [Fact]
    public async Task UnsubscribeHandler_ShouldRenderTheLocalizedCopy()
    {
        var page = LocalizerFactory.CreateMessage<NewsletterPageMessage>();

        PublicGetNewsletterUnsubscribePageResult result = await UnsubscribeHandler.Handle(
            new PublicGetNewsletterUnsubscribePageQuery(Action),
            CancellationToken.None
        );

        result.Html.Should().Contain(WebUtility.HtmlEncode(page.UnsubscribeTitle()));
        result.Html.Should().Contain(WebUtility.HtmlEncode(page.UnsubscribeButton()));
    }

    [Fact]
    public async Task ConfirmAndUnsubscribePages_ShouldNotShareTheirCopy()
    {
        PublicGetNewsletterConfirmPageResult confirm = await ConfirmHandler.Handle(
            new PublicGetNewsletterConfirmPageQuery(Action),
            CancellationToken.None
        );
        PublicGetNewsletterUnsubscribePageResult unsubscribe = await UnsubscribeHandler.Handle(
            new PublicGetNewsletterUnsubscribePageQuery(Action),
            CancellationToken.None
        );

        confirm.Html.Should().NotBe(unsubscribe.Html);
    }

    [Fact]
    public async Task ConfirmHandler_ShouldEncodeAnActionCarryingMarkup()
    {
        // The token reaches the page straight off the route, so a crafted one must not break out.
        const string hostile = "/newsletter/confirm/\"><script>alert(1)</script>";

        PublicGetNewsletterConfirmPageResult result = await ConfirmHandler.Handle(
            new PublicGetNewsletterConfirmPageQuery(hostile),
            CancellationToken.None
        );

        result.Html.Should().NotContain("<script>").And.Contain("&lt;script&gt;");
    }

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    public async Task ConfirmHandler_ShouldRenderInEveryCulture(string culture)
    {
        using var cultureScope = new CultureScope(culture);

        PublicGetNewsletterConfirmPageResult result = await ConfirmHandler.Handle(
            new PublicGetNewsletterConfirmPageQuery(Action),
            CancellationToken.None
        );

        result.Html.Should().NotContain("{{").And.NotContain("ConfirmTitle");
    }
}
