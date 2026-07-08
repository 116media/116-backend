using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Validators;

// Records must expose "Title" and "Slug" properties so ValidationUtils.GetPropertyValue can find them.
internal record ArticleOptionalTitleInput(string? Title);

internal record ArticleOptionalSlugInput(string? Slug);

internal record VideoOptionalTitleInput(string? Title);

internal record VideoOptionalSlugInput(string? Slug);

internal class ArticleOptionalTitleValidator : AbstractValidator<ArticleOptionalTitleInput>
{
    public ArticleOptionalTitleValidator()
    {
        ArticleErrorMessage i18n = LocalizerFactory.CreateMessage<ArticleErrorMessage>();
        RuleFor(x => x.Title).ValidArticleTitle(i18n, isRequired: false);
    }
}

internal class ArticleOptionalSlugValidator : AbstractValidator<ArticleOptionalSlugInput>
{
    public ArticleOptionalSlugValidator()
    {
        ArticleErrorMessage i18n = LocalizerFactory.CreateMessage<ArticleErrorMessage>();
        RuleFor(x => x.Slug).ValidArticleSlug(i18n, isRequired: false);
    }
}

internal class VideoOptionalTitleValidator : AbstractValidator<VideoOptionalTitleInput>
{
    public VideoOptionalTitleValidator()
    {
        VideoErrorMessage i18n = LocalizerFactory.CreateMessage<VideoErrorMessage>();
        RuleFor(x => x.Title).ValidVideoTitle(i18n, isRequired: false);
    }
}

internal class VideoOptionalSlugValidator : AbstractValidator<VideoOptionalSlugInput>
{
    public VideoOptionalSlugValidator()
    {
        VideoErrorMessage i18n = LocalizerFactory.CreateMessage<VideoErrorMessage>();
        RuleFor(x => x.Slug).ValidVideoSlug(i18n, isRequired: false);
    }
}

/// <summary>
/// Tests the isRequired=false branches of editorial validation extension methods.
/// These branches are not exercised by the command-specific validators (which use isRequired=true).
/// </summary>
public class EditorialValidatorsTests
{
    #region EditorialValidation — ValidArticleTitle(isRequired: false)

    [Fact]
    public async Task ValidArticleTitle_Optional_WithNull_ShouldNotHaveErrors()
    {
        var validator = new ArticleOptionalTitleValidator();
        ValidationResult result = await validator.ValidateAsync(new ArticleOptionalTitleInput(Title: null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidArticleTitle_Optional_WithWhitespace_ShouldNotHaveErrors()
    {
        var validator = new ArticleOptionalTitleValidator();
        ValidationResult result = await validator.ValidateAsync(new ArticleOptionalTitleInput(Title: "   "));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidArticleTitle_Optional_WithValidTitle_ShouldNotHaveErrors()
    {
        var validator = new ArticleOptionalTitleValidator();
        ValidationResult result = await validator.ValidateAsync(
            new ArticleOptionalTitleInput(Title: "Valid Article Title")
        );
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidArticleTitle_Optional_WithTooLongTitle_ShouldHaveError()
    {
        var validator = new ArticleOptionalTitleValidator();
        string tooLong = new('a', ContentConstants.MaxTitleLength + 1);
        ValidationResult result = await validator.ValidateAsync(new ArticleOptionalTitleInput(Title: tooLong));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ArticleOptionalTitleInput.Title));
    }

    #endregion

    #region EditorialValidation — ValidArticleSlug(isRequired: false)

    [Fact]
    public async Task ValidArticleSlug_Optional_WithNull_ShouldNotHaveErrors()
    {
        var validator = new ArticleOptionalSlugValidator();
        ValidationResult result = await validator.ValidateAsync(new ArticleOptionalSlugInput(Slug: null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidArticleSlug_Optional_WithWhitespace_ShouldNotHaveErrors()
    {
        var validator = new ArticleOptionalSlugValidator();
        ValidationResult result = await validator.ValidateAsync(new ArticleOptionalSlugInput(Slug: "   "));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidArticleSlug_Optional_WithValidSlug_ShouldNotHaveErrors()
    {
        var validator = new ArticleOptionalSlugValidator();
        ValidationResult result = await validator.ValidateAsync(
            new ArticleOptionalSlugInput(Slug: "valid-article-slug")
        );
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidArticleSlug_Optional_WithTooLongSlug_ShouldHaveError()
    {
        var validator = new ArticleOptionalSlugValidator();
        string tooLong = new('a', ContentConstants.MaxSlugLength + 1);
        ValidationResult result = await validator.ValidateAsync(new ArticleOptionalSlugInput(Slug: tooLong));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ArticleOptionalSlugInput.Slug));
    }

    [Fact]
    public async Task ValidArticleSlug_Optional_WithInvalidFormat_ShouldHaveError()
    {
        var validator = new ArticleOptionalSlugValidator();
        ValidationResult result = await validator.ValidateAsync(new ArticleOptionalSlugInput(Slug: "Invalid Slug"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ArticleOptionalSlugInput.Slug));
    }

    #endregion

    #region EditorialValidation — ValidVideoTitle(isRequired: false)

    [Fact]
    public async Task ValidVideoTitle_Optional_WithNull_ShouldNotHaveErrors()
    {
        var validator = new VideoOptionalTitleValidator();
        ValidationResult result = await validator.ValidateAsync(new VideoOptionalTitleInput(Title: null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidVideoTitle_Optional_WithWhitespace_ShouldNotHaveErrors()
    {
        var validator = new VideoOptionalTitleValidator();
        ValidationResult result = await validator.ValidateAsync(new VideoOptionalTitleInput(Title: "   "));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidVideoTitle_Optional_WithValidTitle_ShouldNotHaveErrors()
    {
        var validator = new VideoOptionalTitleValidator();
        ValidationResult result = await validator.ValidateAsync(
            new VideoOptionalTitleInput(Title: "Valid Video Title")
        );
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidVideoTitle_Optional_WithTooLongTitle_ShouldHaveError()
    {
        var validator = new VideoOptionalTitleValidator();
        string tooLong = new('a', ContentConstants.MaxTitleLength + 1);
        ValidationResult result = await validator.ValidateAsync(new VideoOptionalTitleInput(Title: tooLong));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(VideoOptionalTitleInput.Title));
    }

    #endregion

    #region EditorialValidation — ValidVideoSlug(isRequired: false)

    [Fact]
    public async Task ValidVideoSlug_Optional_WithNull_ShouldNotHaveErrors()
    {
        var validator = new VideoOptionalSlugValidator();
        ValidationResult result = await validator.ValidateAsync(new VideoOptionalSlugInput(Slug: null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidVideoSlug_Optional_WithWhitespace_ShouldNotHaveErrors()
    {
        var validator = new VideoOptionalSlugValidator();
        ValidationResult result = await validator.ValidateAsync(new VideoOptionalSlugInput(Slug: "   "));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidVideoSlug_Optional_WithValidSlug_ShouldNotHaveErrors()
    {
        var validator = new VideoOptionalSlugValidator();
        ValidationResult result = await validator.ValidateAsync(new VideoOptionalSlugInput(Slug: "valid-video-slug"));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidVideoSlug_Optional_WithTooLongSlug_ShouldHaveError()
    {
        var validator = new VideoOptionalSlugValidator();
        string tooLong = new('a', ContentConstants.MaxSlugLength + 1);
        ValidationResult result = await validator.ValidateAsync(new VideoOptionalSlugInput(Slug: tooLong));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(VideoOptionalSlugInput.Slug));
    }

    [Fact]
    public async Task ValidVideoSlug_Optional_WithInvalidFormat_ShouldHaveError()
    {
        var validator = new VideoOptionalSlugValidator();
        ValidationResult result = await validator.ValidateAsync(new VideoOptionalSlugInput(Slug: "Invalid Slug"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(VideoOptionalSlugInput.Slug));
    }

    #endregion
}
