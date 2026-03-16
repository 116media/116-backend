using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
using AwesomeAssertions;
using FluentValidation;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Validators;

// Records for ValidArticleId / ValidVideoId / ValidLyricsId
internal record ArticleIdInput(Guid ArticleId);

internal record VideoIdInput(Guid VideoId);

internal record LyricsIdInput(Guid LyricsId);

internal class ArticleIdValidator : AbstractValidator<ArticleIdInput>
{
    public ArticleIdValidator() => RuleFor(x => x.ArticleId).ValidArticleId();
}

internal class VideoIdValidator : AbstractValidator<VideoIdInput>
{
    public VideoIdValidator() => RuleFor(x => x.VideoId).ValidVideoId();
}

internal class LyricsIdValidator : AbstractValidator<LyricsIdInput>
{
    public LyricsIdValidator() => RuleFor(x => x.LyricsId).ValidLyricsId();
}

// Records must expose "Title" and "Slug" properties so ValidationUtils.GetPropertyValue can find them.
internal record ArticleOptionalTitleInput(string? Title);

internal record ArticleOptionalSlugInput(string? Slug);

internal record VideoOptionalTitleInput(string? Title);

internal record VideoOptionalSlugInput(string? Slug);

internal class ArticleOptionalTitleValidator : AbstractValidator<ArticleOptionalTitleInput>
{
    public ArticleOptionalTitleValidator() => RuleFor(x => x.Title).ValidArticleTitle(isRequired: false);
}

internal class ArticleOptionalSlugValidator : AbstractValidator<ArticleOptionalSlugInput>
{
    public ArticleOptionalSlugValidator() => RuleFor(x => x.Slug).ValidArticleSlug(isRequired: false);
}

internal class VideoOptionalTitleValidator : AbstractValidator<VideoOptionalTitleInput>
{
    public VideoOptionalTitleValidator() => RuleFor(x => x.Title).ValidVideoTitle(isRequired: false);
}

internal class VideoOptionalSlugValidator : AbstractValidator<VideoOptionalSlugInput>
{
    public VideoOptionalSlugValidator() => RuleFor(x => x.Slug).ValidVideoSlug(isRequired: false);
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

    #region EditorialValidation — ValidArticleId / ValidVideoId / ValidLyricsId

    [Fact]
    public async Task ValidArticleId_WithValidGuid_ShouldNotHaveErrors()
    {
        var validator = new ArticleIdValidator();
        ValidationResult result = await validator.ValidateAsync(new ArticleIdInput(Guid.NewGuid()));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidArticleId_WithEmptyGuid_ShouldHaveError()
    {
        var validator = new ArticleIdValidator();
        ValidationResult result = await validator.ValidateAsync(new ArticleIdInput(Guid.Empty));
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(ArticleIdInput.ArticleId) && e.ErrorMessage == "Article ID is required."
            );
    }

    [Fact]
    public async Task ValidVideoId_WithValidGuid_ShouldNotHaveErrors()
    {
        var validator = new VideoIdValidator();
        ValidationResult result = await validator.ValidateAsync(new VideoIdInput(Guid.NewGuid()));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidVideoId_WithEmptyGuid_ShouldHaveError()
    {
        var validator = new VideoIdValidator();
        ValidationResult result = await validator.ValidateAsync(new VideoIdInput(Guid.Empty));
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e => e.PropertyName == nameof(VideoIdInput.VideoId) && e.ErrorMessage == "Video ID is required.");
    }

    [Fact]
    public async Task ValidLyricsId_WithValidGuid_ShouldNotHaveErrors()
    {
        var validator = new LyricsIdValidator();
        ValidationResult result = await validator.ValidateAsync(new LyricsIdInput(Guid.NewGuid()));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidLyricsId_WithEmptyGuid_ShouldHaveError()
    {
        var validator = new LyricsIdValidator();
        ValidationResult result = await validator.ValidateAsync(new LyricsIdInput(Guid.Empty));
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(LyricsIdInput.LyricsId) && e.ErrorMessage == "Lyrics ID is required."
            );
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
