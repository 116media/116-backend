using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertSingleStreamingLink;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpsertSingleStreamingLink;

/// <summary>
/// Unit tests for <see cref="AdminUpsertSingleStreamingLinkValidator"/>.
/// </summary>
public class AdminUpsertSingleStreamingLinkValidatorTests
{
    private readonly AdminUpsertSingleStreamingLinkValidator _validator = new(TestErrorsFactory.CreateContentI18n());

    private static AdminUpsertSingleStreamingLinkCommand Command(string url)
    {
        return new AdminUpsertSingleStreamingLinkCommand(
            LyricsId: Guid.NewGuid(),
            Platform: EnumStreamingPlatform.Spotify,
            Url: url
        );
    }

    [Theory]
    [InlineData("https://open.spotify.com/track/1")]
    [InlineData("http://music.example.com/track/2")]
    public async Task Validate_WhenUrlIsAbsoluteHttpUrl_ShouldPass(string url)
    {
        ValidationResult result = await _validator.ValidateAsync(Command(url));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("open.spotify.com/track/1")]
    [InlineData("/track/1")]
    [InlineData("ftp://example.com/track")]
    [InlineData("javascript:alert(1)")]
    public async Task Validate_WhenUrlIsNotAnAbsoluteHttpUrl_ShouldFail(string url)
    {
        ValidationResult result = await _validator.ValidateAsync(Command(url));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpsertSingleStreamingLinkCommand.Url));
    }

    [Fact]
    public async Task Validate_WhenUrlExceedsTheColumnLength_ShouldFail()
    {
        string url = "https://open.spotify.com/track/" + new string('a', ContentConstants.MaxStreamingLinkUrlLength);

        ValidationResult result = await _validator.ValidateAsync(Command(url));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpsertSingleStreamingLinkCommand.Url));
    }
}
