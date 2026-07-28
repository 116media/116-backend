using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertArtistSocialLink;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Helpers;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpsertArtistSocialLink;

/// <summary>
/// Unit tests for <see cref="AdminUpsertArtistSocialLinkValidator"/>. The URL becomes an
/// href on the public page, so the scheme lock is a stored-XSS guard, not pedantry.
/// </summary>
public class AdminUpsertArtistSocialLinkValidatorTests
{
    private readonly AdminUpsertArtistSocialLinkValidator _validator = new(TestErrorsFactory.CreateContentI18n());

    private static AdminUpsertArtistSocialLinkCommand Command(string url) =>
        new(Guid.NewGuid(), EnumSocialPlatform.Instagram, url);

    [Fact]
    public void Validate_WithHttpsUrl_ShouldPass()
    {
        _validator.TestValidate(Command("https://instagram.com/fallyipupa01")).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUrl_ShouldFail()
    {
        _validator.TestValidate(Command("")).ShouldHaveValidationErrorFor(x => x.Url);
    }

    [Fact]
    public void Validate_WithOverlongUrl_ShouldFail()
    {
        string url = "https://example.com/" + new string('a', 500);
        _validator.TestValidate(Command(url)).ShouldHaveValidationErrorFor(x => x.Url);
    }

    [Theory]
    [InlineData("http://instagram.com/someone")]
    [InlineData("javascript:alert(1)")]
    [InlineData("ftp://example.com/file")]
    [InlineData("/relative/path")]
    [InlineData("not a url")]
    public void Validate_WithNonHttpsUrl_ShouldFail(string url)
    {
        _validator.TestValidate(Command(url)).ShouldHaveValidationErrorFor(x => x.Url);
    }

    [Fact]
    public void Validate_WithUndeclaredPlatform_ShouldFail()
    {
        var command = new AdminUpsertArtistSocialLinkCommand(
            Guid.NewGuid(),
            (EnumSocialPlatform)999,
            "https://example.com"
        );

        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Platform);
    }
}
