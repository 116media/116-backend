using _116.Identity.Application.Auth.Validators;
using AwesomeAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Validators;

/// <summary>
/// Unit tests for <see cref="SocialLoginValidation"/>.
/// </summary>
public class SocialLoginValidationTests
{
    private class TestAvatarUrlCommand
    {
        public string? AvatarUrl { get; set; }
    }

    private class TestAuthProviderCommand
    {
        public string Provider { get; set; } = string.Empty;
    }

    private class TestAvatarUrlCommandValidator : AbstractValidator<TestAvatarUrlCommand>
    {
        public TestAvatarUrlCommandValidator()
        {
            RuleFor(x => x.AvatarUrl).ValidAvatarUrl();
        }
    }

    private class TestAuthProviderCommandValidator : AbstractValidator<TestAuthProviderCommand>
    {
        public TestAuthProviderCommandValidator()
        {
            RuleFor(x => x.Provider).ValidAuthProvider();
        }
    }

    #region ValidAvatarUrl

    [Fact]
    public void ValidAvatarUrl_WithValidUrl_ShouldPass()
    {
        var validator = new TestAvatarUrlCommandValidator();
        var command = new TestAvatarUrlCommand { AvatarUrl = "https://avatar.url/image.jpg" };

        TestValidationResult<TestAvatarUrlCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.AvatarUrl);
    }

    [Fact]
    public void ValidAvatarUrl_WithNullUrl_ShouldPass()
    {
        var validator = new TestAvatarUrlCommandValidator();
        var command = new TestAvatarUrlCommand { AvatarUrl = null };

        TestValidationResult<TestAvatarUrlCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.AvatarUrl);
    }

    [Fact]
    public void ValidAvatarUrl_WithEmptyUrl_ShouldPass()
    {
        var validator = new TestAvatarUrlCommandValidator();
        var command = new TestAvatarUrlCommand { AvatarUrl = string.Empty };

        TestValidationResult<TestAvatarUrlCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.AvatarUrl);
    }

    [Fact]
    public void ValidAvatarUrl_WithInvalidUrl_ShouldFail()
    {
        var validator = new TestAvatarUrlCommandValidator();
        var command = new TestAvatarUrlCommand { AvatarUrl = "not-a-valid-url" };

        TestValidationResult<TestAvatarUrlCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.AvatarUrl)
            .WithErrorMessage("Avatar must be a valid URL when provided");
    }

    [Fact]
    public void ValidAvatarUrl_WithHttpUrl_ShouldPass()
    {
        var validator = new TestAvatarUrlCommandValidator();
        var command = new TestAvatarUrlCommand { AvatarUrl = "http://avatar.url/image.jpg" };

        TestValidationResult<TestAvatarUrlCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.AvatarUrl);
    }

    #endregion

    #region ValidAuthProvider

    [Fact]
    public void ValidAuthProvider_WithGoogle_ShouldPass()
    {
        var validator = new TestAuthProviderCommandValidator();
        var command = new TestAuthProviderCommand { Provider = "Google" };

        TestValidationResult<TestAuthProviderCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Provider);
    }

    [Fact]
    public void ValidAuthProvider_WithFacebook_ShouldPass()
    {
        var validator = new TestAuthProviderCommandValidator();
        var command = new TestAuthProviderCommand { Provider = "Facebook" };

        TestValidationResult<TestAuthProviderCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Provider);
    }

    [Fact]
    public void ValidAuthProvider_WithEmptyProvider_ShouldFail()
    {
        var validator = new TestAuthProviderCommandValidator();
        var command = new TestAuthProviderCommand { Provider = string.Empty };

        TestValidationResult<TestAuthProviderCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Provider).WithErrorMessage("Auth provider is required.");
    }

    [Fact]
    public void ValidAuthProvider_WithNullProvider_ShouldFail()
    {
        var validator = new TestAuthProviderCommandValidator();
        var command = new TestAuthProviderCommand { Provider = null! };

        TestValidationResult<TestAuthProviderCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Provider).WithErrorMessage("Auth provider is required.");
    }

    [Fact]
    public void ValidAuthProvider_WithEmptyProvider_ShouldStopCascading()
    {
        var validator = new TestAuthProviderCommandValidator();
        var command = new TestAuthProviderCommand { Provider = string.Empty };

        TestValidationResult<TestAuthProviderCommand> result = validator.TestValidate(command);

        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(TestAuthProviderCommand.Provider));
    }

    [Fact]
    public void ValidAuthProvider_WithInvalidProvider_ShouldFail()
    {
        var validator = new TestAuthProviderCommandValidator();
        var command = new TestAuthProviderCommand { Provider = "GitHub" };

        TestValidationResult<TestAuthProviderCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.Provider)
            .WithErrorMessage("Auth provider must be Facebook or Google");
    }

    [Fact]
    public void ValidAuthProvider_WithLocalProvider_ShouldPass()
    {
        var validator = new TestAuthProviderCommandValidator();
        var command = new TestAuthProviderCommand { Provider = "Local" };

        TestValidationResult<TestAuthProviderCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Provider);
    }

    [Fact]
    public void ValidAuthProvider_WithLowercaseProvider_ShouldFail()
    {
        var validator = new TestAuthProviderCommandValidator();
        var command = new TestAuthProviderCommand { Provider = "google" };

        TestValidationResult<TestAuthProviderCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.Provider)
            .WithErrorMessage("Auth provider must be Facebook or Google");
    }

    #endregion
}
