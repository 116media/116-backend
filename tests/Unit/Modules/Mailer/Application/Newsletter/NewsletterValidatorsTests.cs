using _116.Mailer.Application.Newsletter.UseCases.Public.Commands.ConfirmNewsletter;
using _116.Mailer.Application.Newsletter.UseCases.Public.Commands.SubscribeNewsletter;
using _116.Mailer.Application.Newsletter.UseCases.Public.Commands.UnsubscribeNewsletter;
using _116.Mailer.Application.Shared.Errors;
using _116.Mailer.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Application.Newsletter;

/// <summary>
/// Unit tests for the newsletter command validators.
/// </summary>
public class NewsletterValidatorsTests
{
    private static readonly NewsletterErrors Errors = new(LocalizerFactory.CreateMessage<NewsletterErrorMessage>());

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("missing@tld@double")]
    public async Task Subscribe_InvalidEmail_ShouldFail(string email)
    {
        var validator = new PublicSubscribeNewsletterValidator(Errors);

        ValidationResult result = await validator.ValidateAsync(new PublicSubscribeNewsletterCommand(email));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Subscribe_OverlongEmail_ShouldFail()
    {
        var validator = new PublicSubscribeNewsletterValidator(Errors);
        string overlong = $"{new string('a', 320)}@example.com";

        ValidationResult result = await validator.ValidateAsync(new PublicSubscribeNewsletterCommand(overlong));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Subscribe_ValidEmail_ShouldPass()
    {
        var validator = new PublicSubscribeNewsletterValidator(Errors);

        ValidationResult result = await validator.ValidateAsync(
            new PublicSubscribeNewsletterCommand("fan@example.com")
        );

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Confirm_EmptyToken_ShouldFail()
    {
        var validator = new PublicConfirmNewsletterValidator(Errors);

        ValidationResult result = await validator.ValidateAsync(new PublicConfirmNewsletterCommand(""));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Unsubscribe_EmptyToken_ShouldFail()
    {
        var validator = new PublicUnsubscribeNewsletterValidator(Errors);

        ValidationResult result = await validator.ValidateAsync(new PublicUnsubscribeNewsletterCommand(""));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Confirm_PresentToken_ShouldPass()
    {
        var validator = new PublicConfirmNewsletterValidator(Errors);

        ValidationResult result = await validator.ValidateAsync(new PublicConfirmNewsletterCommand("token-x"));

        result.IsValid.Should().BeTrue();
    }
}
