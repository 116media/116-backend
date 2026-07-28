using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Validators;

// The record must expose a "Slug" property so ValidationUtils.GetPropertyValue can find it.
internal record LyricsOptionalSlugInput(string? Slug);

internal class LyricsOptionalSlugValidator : AbstractValidator<LyricsOptionalSlugInput>
{
    public LyricsOptionalSlugValidator()
    {
        LyricsErrorMessage i18n = LocalizerFactory.CreateMessage<LyricsErrorMessage>();
        RuleFor(x => x.Slug).ValidLyricsSlug(i18n, isRequired: false);
    }
}

/// <summary>
/// Tests the isRequired=false branch of <see cref="EditorialValidation.ValidLyricsSlug{T}"/> —
/// the one editorial rule with an optional flavour, used by the public lyrics submission
/// validator where the slug may be omitted.
/// </summary>
public class EditorialValidatorsTests
{
    #region EditorialValidation — ValidLyricsSlug(isRequired: false)

    [Fact]
    public async Task ValidLyricsSlug_Optional_WithNull_ShouldNotHaveErrors()
    {
        var validator = new LyricsOptionalSlugValidator();
        ValidationResult result = await validator.ValidateAsync(new LyricsOptionalSlugInput(Slug: null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidLyricsSlug_Optional_WithWhitespace_ShouldNotHaveErrors()
    {
        var validator = new LyricsOptionalSlugValidator();
        ValidationResult result = await validator.ValidateAsync(new LyricsOptionalSlugInput(Slug: "   "));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidLyricsSlug_Optional_WithValidSlug_ShouldNotHaveErrors()
    {
        var validator = new LyricsOptionalSlugValidator();
        ValidationResult result = await validator.ValidateAsync(
            new LyricsOptionalSlugInput(Slug: "fally-ipupa-eloko-oyo-lyrics")
        );
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidLyricsSlug_Optional_WithTooLongSlug_ShouldHaveError()
    {
        var validator = new LyricsOptionalSlugValidator();
        string tooLong = new('a', ContentConstants.MaxSlugLength + 1);
        ValidationResult result = await validator.ValidateAsync(new LyricsOptionalSlugInput(Slug: tooLong));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LyricsOptionalSlugInput.Slug));
    }

    [Fact]
    public async Task ValidLyricsSlug_Optional_WithInvalidFormat_ShouldHaveError()
    {
        var validator = new LyricsOptionalSlugValidator();
        ValidationResult result = await validator.ValidateAsync(new LyricsOptionalSlugInput(Slug: "Invalid Slug"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LyricsOptionalSlugInput.Slug));
    }

    #endregion
}
