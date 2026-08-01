using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Application.Templates;
using _116.Mailer.Application.Templates.Messages;
using _116.Mailer.Contracts.Application;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Application.Templates;

/// <summary>
/// Unit tests for <see cref="EmailTemplateRenderer" /> against the real
/// resource catalog: every template renders fully in both cultures, missing
/// tokens fail loudly, and token values are HTML-encoded into the HTML part.
/// </summary>
public class EmailTemplateRendererTests
{
    /// <summary>
    /// The full token superset — supplying extras is legal, so one dictionary
    /// covers every template's requirements.
    /// </summary>
    private static readonly Dictionary<string, string> AllTokens = new()
    {
        ["userName"] = "Fally",
        ["otpCode"] = "123456",
        ["expiryMinutes"] = "60",
        ["loginTime"] = "2026-08-16 12:00:00Z",
        ["ipAddress"] = "127.0.0.1",
        ["deviceSummary"] = "Safari on macOS",
        ["confirmUrl"] = "https://app.example/confirm/x",
        ["unsubscribeUrl"] = "https://app.example/unsubscribe/x",
        ["changeTime"] = "2026-08-16 12:00:00Z",
        ["resetTime"] = "2026-08-16 12:00:00Z",
        ["newEmailMasked"] = "f***@example.com",
        ["time"] = "2026-08-16 12:00:00Z",
        ["roleName"] = "Admin",
        ["action"] = "granted",
        ["customerName"] = "Label BOMAYE",
        ["orderReference"] = "AB12CD34",
        ["amountUsd"] = "150.00",
        ["paymentMethods"] = "BankTransfer, MobileMoney, Cash",
        ["itemSummary"] = "Musique, Interview",
        ["receiptUrl"] = "https://res.cloudinary.com/receipt.png",
        ["paidAt"] = "2026-08-16 12:00:00Z",
        ["notes"] = "Reference number missing",
        ["contentTitle"] = "Eloko Oyo",
        ["reason"] = "Policy violation",
        ["removedAt"] = "2026-08-16 12:00:00Z",
        ["publicUrl"] = "https://app.example/articles/eloko-oyo",
        ["shootDate"] = "2026-09-01 09:00:00Z",
        ["replierName"] = "Aline",
        ["articleTitle"] = "Eloko Oyo review",
        ["replyExcerpt"] = "Totally agree with you…",
        ["articleUrl"] = "https://app.example/articles/eloko-oyo",
        ["songTitle"] = "Eloko Oyo",
        ["decision"] = "accepted",
        ["lyricsUrl"] = "https://app.example/lyrics/eloko-oyo",
        ["outcome"] = "approved",
        ["reviewNote"] = "Great transcription.",
        ["artistName"] = "Fally Ipupa",
        ["artistUrl"] = "https://app.example/artists/fally-ipupa",
    };

    private static readonly EmailTemplateRenderer Renderer = new(
        LocalizerFactory.CreateMessage<EmailTemplateMessage>()
    );

    public static TheoryData<EnumEmailTemplate, string> AllTemplateCultures()
    {
        var data = new TheoryData<EnumEmailTemplate, string>();

        foreach (EnumEmailTemplate template in Enum.GetValues<EnumEmailTemplate>())
        {
            data.Add(template, "en");
            data.Add(template, "fr");
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(AllTemplateCultures))]
    public void Render_EveryTemplateInEveryCulture_ShouldLeaveNoPlaceholder(EnumEmailTemplate template, string culture)
    {
        RenderedEmail rendered = Renderer.Render(template, AllTokens, culture);

        rendered.Subject.Should().NotContain("{{").And.NotBeNullOrWhiteSpace();
        rendered.HtmlBody.Should().NotContain("{{");
        rendered.TextBody.Should().NotContain("{{").And.NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Render_ShouldWrapTheHtmlBodyInTheSharedLayout()
    {
        RenderedEmail rendered = Renderer.Render(EnumEmailTemplate.Welcome, AllTokens, "en");

        rendered.HtmlBody.Should().Contain("max-width:560px");
    }

    [Fact]
    public void Render_WithMissingToken_ShouldThrow()
    {
        var incomplete = new Dictionary<string, string> { ["userName"] = "Fally" };

        Action act = () => Renderer.Render(EnumEmailTemplate.EmailVerificationOtp, incomplete, "en");

        act.Should().Throw<InvalidOperationException>().WithMessage("*unresolved placeholder*");
    }

    [Fact]
    public void Render_ShouldHtmlEncodeTokenValuesInTheHtmlPartOnly()
    {
        var tokens = new Dictionary<string, string>(AllTokens) { ["userName"] = "<script>alert(1)</script>" };

        RenderedEmail rendered = Renderer.Render(EnumEmailTemplate.Welcome, tokens, "en");

        rendered.HtmlBody.Should().NotContain("<script>").And.Contain("&lt;script&gt;");
        rendered.TextBody.Should().Contain("<script>alert(1)</script>");
    }

    [Fact]
    public void Render_UnknownCulture_ShouldFallBackToNeutral()
    {
        RenderedEmail neutral = Renderer.Render(EnumEmailTemplate.Welcome, AllTokens, "xx");
        RenderedEmail english = Renderer.Render(EnumEmailTemplate.Welcome, AllTokens, "en");

        neutral.Subject.Should().Be(english.Subject);
    }

    [Fact]
    public void Render_French_ShouldDifferFromEnglish()
    {
        RenderedEmail french = Renderer.Render(EnumEmailTemplate.Welcome, AllTokens, "fr");
        RenderedEmail english = Renderer.Render(EnumEmailTemplate.Welcome, AllTokens, "en");

        french.Subject.Should().NotBe(english.Subject);
    }
}
