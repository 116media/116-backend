using _116.Mailer.Application.Notifications;
using _116.Mailer.Application.Notifications.Messages;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Contracts.Application;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Application.Notifications;

/// <summary>
/// Unit tests for <see cref="NotificationRenderer" /> against the real
/// resource catalog: every active type renders fully in both cultures,
/// missing tokens fail loudly, and reserved catalog members without copy
/// fail loudly instead of rendering their raw key name.
/// </summary>
public class NotificationRendererTests
{
    /// <summary>
    /// The full token superset — supplying extras is legal, so one dictionary
    /// covers every type's requirements.
    /// </summary>
    private static readonly Dictionary<string, string> AllTokens = new()
    {
        ["newEmailMasked"] = "f***@example.com",
        ["roleName"] = "Admin",
        ["action"] = "granted",
        ["replierName"] = "Aline",
        ["articleTitle"] = "Eloko Oyo review",
        ["linkPath"] = "/articles/eloko-oyo",
        ["songTitle"] = "Eloko Oyo",
        ["decision"] = "accepted",
        ["outcome"] = "approved",
        ["artistName"] = "Fally Ipupa",
    };

    /// <summary>
    /// Every catalog member has copy since the community wave landed, so the
    /// whole enum renders in both cultures.
    /// </summary>
    private static readonly EnumNotificationType[] ActiveTypes = Enum.GetValues<EnumNotificationType>();

    private static readonly NotificationRenderer Renderer = new(LocalizerFactory.CreateMessage<NotificationMessage>());

    public static TheoryData<EnumNotificationType, string> ActiveTypeCultures()
    {
        var data = new TheoryData<EnumNotificationType, string>();

        foreach (EnumNotificationType type in ActiveTypes)
        {
            data.Add(type, "en");
            data.Add(type, "fr");
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(ActiveTypeCultures))]
    public void Render_EveryActiveTypeInEveryCulture_ShouldLeaveNoPlaceholder(EnumNotificationType type, string culture)
    {
        RenderedNotification rendered = Renderer.Render(type, AllTokens, culture);

        rendered.Title.Should().NotContain("{{").And.NotBeNullOrWhiteSpace();
        rendered.Body.Should().NotContain("{{").And.NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Render_ShouldLocalizeByCulture()
    {
        RenderedNotification english = Renderer.Render(EnumNotificationType.PasswordChanged, AllTokens, "en");
        RenderedNotification french = Renderer.Render(EnumNotificationType.PasswordChanged, AllTokens, "fr");

        english.Title.Should().Be("Password changed");
        french.Title.Should().Be("Mot de passe modifié");
    }

    [Fact]
    public void Render_WithAnUnknownCulture_ShouldFallBackToNeutralResources()
    {
        RenderedNotification rendered = Renderer.Render(EnumNotificationType.PasswordChanged, AllTokens, "zz");

        rendered.Title.Should().Be("Password changed");
    }

    /// <summary>
    /// Verifies that a malformed culture name — one no culture can be built
    /// from, unlike an unassigned two-letter code — falls back to the neutral
    /// resources instead of failing the render.
    /// </summary>
    [Fact]
    public void Render_WithAMalformedCultureName_ShouldFallBackToNeutralResources()
    {
        RenderedNotification rendered = Renderer.Render(
            EnumNotificationType.PasswordChanged,
            AllTokens,
            "!! not a culture !!"
        );

        rendered.Title.Should().Be("Password changed");
    }

    [Fact]
    public void Render_ShouldSubstituteTokensIntoTheBody()
    {
        RenderedNotification rendered = Renderer.Render(EnumNotificationType.CommentReply, AllTokens, "en");

        rendered.Body.Should().Be("Aline replied to your comment on Eloko Oyo review.");
    }

    [Fact]
    public void Render_WithMissingToken_ShouldThrow()
    {
        var incomplete = new Dictionary<string, string>();

        Action act = () => Renderer.Render(EnumNotificationType.EmailChanged, incomplete, "en");

        act.Should().Throw<InvalidOperationException>().WithMessage("*unresolved placeholder*");
    }

    [Fact]
    public void Render_ATypeWithoutResources_ShouldThrow()
    {
        // No reserved catalog member remains, so an out-of-range value stands
        // in for a member whose resources were never added.
        const EnumNotificationType unknown = (EnumNotificationType)999;

        Action act = () => Renderer.Render(unknown, AllTokens, "en");

        act.Should().Throw<InvalidOperationException>().WithMessage("*missing*");
    }
}
