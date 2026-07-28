using _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks;
using _116.Tests.Fixtures.Helpers;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks;

/// <summary>
/// Unit tests for <see cref="AdminResolveAlbumStreamingLinksValidator"/>.
/// </summary>
public class AdminResolveAlbumStreamingLinksValidatorTests
{
    private readonly AdminResolveAlbumStreamingLinksValidator _validator = new(TestErrorsFactory.CreateContentI18n());

    private static AdminResolveAlbumStreamingLinksCommand Command(string sourceUrl) => new(Guid.NewGuid(), sourceUrl);

    [Fact]
    public void Validate_WithHttpsSourceUrl_ShouldPass()
    {
        _validator.TestValidate(Command("https://open.spotify.com/album/abc123")).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptySourceUrl_ShouldFail()
    {
        _validator.TestValidate(Command("")).ShouldHaveValidationErrorFor(x => x.SourceUrl);
    }

    [Fact]
    public void Validate_WithOverlongSourceUrl_ShouldFail()
    {
        string url = "https://open.spotify.com/album/" + new string('a', 500);
        _validator.TestValidate(Command(url)).ShouldHaveValidationErrorFor(x => x.SourceUrl);
    }

    [Theory]
    [InlineData("http://open.spotify.com/album/abc")]
    [InlineData("javascript:alert(1)")]
    [InlineData("/relative/path")]
    [InlineData("not a url")]
    public void Validate_WithNonHttpsSourceUrl_ShouldFail(string url)
    {
        _validator.TestValidate(Command(url)).ShouldHaveValidationErrorFor(x => x.SourceUrl);
    }
}
