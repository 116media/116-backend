using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Validators;

internal record ContentTypeOptionalNameInput(string? Name);

internal record PricingTierOptionalInput(string? Name, string? Description);

internal record PromotionLevelOptionalNameInput(string? Name);

internal record TagOptionalInput(string? Name, string? Slug);

internal record CategoryOptionalInput(string? Name, string? Slug);

internal class ContentTypeOptionalNameValidator : AbstractValidator<ContentTypeOptionalNameInput>
{
    public ContentTypeOptionalNameValidator()
    {
        ContentTypeErrorMessage msg = LocalizerFactory.CreateMessage<ContentTypeErrorMessage>();
        RuleFor(x => x.Name)
            .ValidContentTypeName(
                nameRequired: msg.NameRequired(),
                nameTooLong: msg.NameTooLong(ContentConstants.MaxContentTypeNameLength),
                isRequired: false
            );
    }
}

internal class PricingTierOptionalNameValidator : AbstractValidator<PricingTierOptionalInput>
{
    public PricingTierOptionalNameValidator()
    {
        PricingTierErrorMessage msg = LocalizerFactory.CreateMessage<PricingTierErrorMessage>();
        RuleFor(x => x.Name)
            .ValidPricingTierName(
                nameRequired: msg.NameRequired(),
                nameTooLong: msg.NameTooLong(ContentConstants.MaxPricingTierNameLength),
                isRequired: false
            );
    }
}

internal class PricingTierRequiredDescriptionValidator : AbstractValidator<PricingTierOptionalInput>
{
    public PricingTierRequiredDescriptionValidator()
    {
        PricingTierErrorMessage msg = LocalizerFactory.CreateMessage<PricingTierErrorMessage>();
        RuleFor(x => x.Description)
            .ValidPricingTierDescription(
                descriptionRequired: msg.DescriptionRequired(),
                descriptionTooLong: msg.DescriptionTooLong(ContentConstants.MaxPricingTierDescriptionLength)
            );
    }
}

internal class PromotionLevelOptionalNameValidator : AbstractValidator<PromotionLevelOptionalNameInput>
{
    public PromotionLevelOptionalNameValidator()
    {
        PromotionLevelErrorMessage msg = LocalizerFactory.CreateMessage<PromotionLevelErrorMessage>();
        RuleFor(x => x.Name)
            .ValidPromotionLevelName(
                nameRequired: msg.NameRequired(),
                nameTooLong: msg.NameTooLong(ContentConstants.MaxPromotionLevelNameLength),
                isRequired: false
            );
    }
}

internal class TagOptionalNameValidator : AbstractValidator<TagOptionalInput>
{
    public TagOptionalNameValidator()
    {
        TagErrorMessage msg = LocalizerFactory.CreateMessage<TagErrorMessage>();
        RuleFor(x => x.Name)
            .ValidTagName(
                nameRequired: msg.NameRequired(),
                nameTooLong: msg.NameTooLong(ContentConstants.MaxTagNameLength),
                isRequired: false
            );
    }
}

internal class TagOptionalSlugValidator : AbstractValidator<TagOptionalInput>
{
    public TagOptionalSlugValidator()
    {
        TagErrorMessage msg = LocalizerFactory.CreateMessage<TagErrorMessage>();
        RuleFor(x => x.Slug)
            .ValidTagSlug(
                slugRequired: msg.SlugRequired(),
                slugTooLong: msg.SlugTooLong(ContentConstants.MaxTagSlugLength),
                slugInvalidFormat: msg.SlugInvalidFormat(),
                isRequired: false
            );
    }
}

internal class CategoryOptionalNameValidator : AbstractValidator<CategoryOptionalInput>
{
    public CategoryOptionalNameValidator()
    {
        CategoryErrorMessage msg = LocalizerFactory.CreateMessage<CategoryErrorMessage>();
        RuleFor(x => x.Name)
            .ValidCategoryName(
                nameRequired: msg.NameRequired(),
                nameTooLong: msg.NameTooLong(ContentConstants.MaxCategoryNameLength),
                isRequired: false
            );
    }
}

internal class CategoryOptionalSlugValidator : AbstractValidator<CategoryOptionalInput>
{
    public CategoryOptionalSlugValidator()
    {
        CategoryErrorMessage msg = LocalizerFactory.CreateMessage<CategoryErrorMessage>();
        RuleFor(x => x.Slug)
            .ValidCategorySlug(
                slugRequired: msg.SlugRequired(),
                slugTooLong: msg.SlugTooLong(ContentConstants.MaxCategorySlugLength),
                slugInvalidFormat: msg.SlugInvalidFormat(),
                isRequired: false
            );
    }
}

/// <summary>
/// Tests the isRequired=false branches of shared validation extension methods.
/// These branches are not exercised by the command-specific validators (which use isRequired=true).
/// </summary>
public class SharedValidatorsTests
{
    #region ContentTypeValidation — ValidContentTypeName(isRequired: false)

    [Fact]
    public async Task ValidContentTypeName_Optional_WithNull_ShouldNotHaveErrors()
    {
        var validator = new ContentTypeOptionalNameValidator();
        ValidationResult result = await validator.ValidateAsync(new ContentTypeOptionalNameInput(Name: null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidContentTypeName_Optional_WithWhitespace_ShouldNotHaveErrors()
    {
        var validator = new ContentTypeOptionalNameValidator();
        ValidationResult result = await validator.ValidateAsync(new ContentTypeOptionalNameInput(Name: "   "));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidContentTypeName_Optional_WithValidName_ShouldNotHaveErrors()
    {
        var validator = new ContentTypeOptionalNameValidator();
        ValidationResult result = await validator.ValidateAsync(new ContentTypeOptionalNameInput(Name: "Article"));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidContentTypeName_Optional_WithTooLongName_ShouldHaveError()
    {
        var validator = new ContentTypeOptionalNameValidator();
        string tooLong = new('a', ContentConstants.MaxContentTypeNameLength + 1);
        ValidationResult result = await validator.ValidateAsync(new ContentTypeOptionalNameInput(Name: tooLong));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ContentTypeOptionalNameInput.Name));
    }

    #endregion

    #region PricingTierValidation — ValidPricingTierName(isRequired: false)

    [Fact]
    public async Task ValidPricingTierName_Optional_WithNull_ShouldNotHaveErrors()
    {
        var validator = new PricingTierOptionalNameValidator();
        ValidationResult result = await validator.ValidateAsync(
            new PricingTierOptionalInput(Name: null, Description: null)
        );
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidPricingTierName_Optional_WithValidName_ShouldNotHaveErrors()
    {
        var validator = new PricingTierOptionalNameValidator();
        ValidationResult result = await validator.ValidateAsync(
            new PricingTierOptionalInput(Name: "Standard", Description: null)
        );
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidPricingTierName_Optional_WithTooLongName_ShouldHaveError()
    {
        var validator = new PricingTierOptionalNameValidator();
        string tooLong = new('a', ContentConstants.MaxPricingTierNameLength + 1);
        ValidationResult result = await validator.ValidateAsync(
            new PricingTierOptionalInput(Name: tooLong, Description: null)
        );
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PricingTierOptionalInput.Name));
    }

    #endregion

    #region PricingTierValidation — ValidPricingTierDescription

    [Fact]
    public async Task ValidPricingTierDescription_Required_WithValidDescription_ShouldNotHaveErrors()
    {
        var validator = new PricingTierRequiredDescriptionValidator();
        ValidationResult result = await validator.ValidateAsync(
            new PricingTierOptionalInput(Name: null, Description: "A valid description")
        );
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidPricingTierDescription_Required_WithEmpty_ShouldHaveError()
    {
        var validator = new PricingTierRequiredDescriptionValidator();
        ValidationResult result = await validator.ValidateAsync(
            new PricingTierOptionalInput(Name: null, Description: string.Empty)
        );
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(PricingTierOptionalInput.Description)
                && e.ErrorMessage == "Pricing tier description is required."
            );
    }

    [Fact]
    public async Task ValidPricingTierDescription_Required_WithTooLong_ShouldHaveError()
    {
        var validator = new PricingTierRequiredDescriptionValidator();
        string tooLong = new('a', ContentConstants.MaxPricingTierDescriptionLength + 1);
        ValidationResult result = await validator.ValidateAsync(
            new PricingTierOptionalInput(Name: null, Description: tooLong)
        );
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PricingTierOptionalInput.Description));
    }

    #endregion

    #region PromotionLevelValidation — ValidPromotionLevelName(isRequired: false)

    [Fact]
    public async Task ValidPromotionLevelName_Optional_WithNull_ShouldNotHaveErrors()
    {
        var validator = new PromotionLevelOptionalNameValidator();
        ValidationResult result = await validator.ValidateAsync(new PromotionLevelOptionalNameInput(Name: null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidPromotionLevelName_Optional_WithValidName_ShouldNotHaveErrors()
    {
        var validator = new PromotionLevelOptionalNameValidator();
        ValidationResult result = await validator.ValidateAsync(new PromotionLevelOptionalNameInput(Name: "Gold"));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidPromotionLevelName_Optional_WithTooLongName_ShouldHaveError()
    {
        var validator = new PromotionLevelOptionalNameValidator();
        string tooLong = new('a', ContentConstants.MaxPromotionLevelNameLength + 1);
        ValidationResult result = await validator.ValidateAsync(new PromotionLevelOptionalNameInput(Name: tooLong));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PromotionLevelOptionalNameInput.Name));
    }

    #endregion

    #region TagValidation — ValidTagName(isRequired: false)

    [Fact]
    public async Task ValidTagName_Optional_WithNull_ShouldNotHaveErrors()
    {
        var validator = new TagOptionalNameValidator();
        ValidationResult result = await validator.ValidateAsync(new TagOptionalInput(Name: null, Slug: null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidTagName_Optional_WithValidName_ShouldNotHaveErrors()
    {
        var validator = new TagOptionalNameValidator();
        ValidationResult result = await validator.ValidateAsync(new TagOptionalInput(Name: "Hip Hop", Slug: null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidTagName_Optional_WithTooLongName_ShouldHaveError()
    {
        var validator = new TagOptionalNameValidator();
        string tooLong = new('a', ContentConstants.MaxTagNameLength + 1);
        ValidationResult result = await validator.ValidateAsync(new TagOptionalInput(Name: tooLong, Slug: null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(TagOptionalInput.Name));
    }

    #endregion

    #region TagValidation — ValidTagSlug(isRequired: false)

    [Fact]
    public async Task ValidTagSlug_Optional_WithNull_ShouldNotHaveErrors()
    {
        var validator = new TagOptionalSlugValidator();
        ValidationResult result = await validator.ValidateAsync(new TagOptionalInput(Name: null, Slug: null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidTagSlug_Optional_WithValidSlug_ShouldNotHaveErrors()
    {
        var validator = new TagOptionalSlugValidator();
        ValidationResult result = await validator.ValidateAsync(new TagOptionalInput(Name: null, Slug: "hip-hop"));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidTagSlug_Optional_WithUppercaseSlug_ShouldHaveError()
    {
        var validator = new TagOptionalSlugValidator();
        ValidationResult result = await validator.ValidateAsync(new TagOptionalInput(Name: null, Slug: "Hip-Hop"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(TagOptionalInput.Slug));
    }

    [Fact]
    public async Task ValidTagSlug_Optional_WithTooLongSlug_ShouldHaveError()
    {
        var validator = new TagOptionalSlugValidator();
        string tooLong = new('a', ContentConstants.MaxTagSlugLength + 1);
        ValidationResult result = await validator.ValidateAsync(new TagOptionalInput(Name: null, Slug: tooLong));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(TagOptionalInput.Slug));
    }

    #endregion

    #region CategoryValidation — ValidCategoryName(isRequired: false)

    [Fact]
    public async Task ValidCategoryName_Optional_WithNull_ShouldNotHaveErrors()
    {
        var validator = new CategoryOptionalNameValidator();
        ValidationResult result = await validator.ValidateAsync(new CategoryOptionalInput(Name: null, Slug: null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidCategoryName_Optional_WithValidName_ShouldNotHaveErrors()
    {
        var validator = new CategoryOptionalNameValidator();
        ValidationResult result = await validator.ValidateAsync(
            new CategoryOptionalInput(Name: "Artist Profile", Slug: null)
        );
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidCategoryName_Optional_WithTooLongName_ShouldHaveError()
    {
        var validator = new CategoryOptionalNameValidator();
        string tooLong = new('a', ContentConstants.MaxCategoryNameLength + 1);
        ValidationResult result = await validator.ValidateAsync(new CategoryOptionalInput(Name: tooLong, Slug: null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CategoryOptionalInput.Name));
    }

    #endregion

    #region CategoryValidation — ValidCategorySlug(isRequired: false)

    [Fact]
    public async Task ValidCategorySlug_Optional_WithNull_ShouldNotHaveErrors()
    {
        var validator = new CategoryOptionalSlugValidator();
        ValidationResult result = await validator.ValidateAsync(new CategoryOptionalInput(Name: null, Slug: null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidCategorySlug_Optional_WithValidSlug_ShouldNotHaveErrors()
    {
        var validator = new CategoryOptionalSlugValidator();
        ValidationResult result = await validator.ValidateAsync(
            new CategoryOptionalInput(Name: null, Slug: "artist-profile")
        );
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidCategorySlug_Optional_WithUppercaseSlug_ShouldHaveError()
    {
        var validator = new CategoryOptionalSlugValidator();
        ValidationResult result = await validator.ValidateAsync(
            new CategoryOptionalInput(Name: null, Slug: "Artist-Profile")
        );
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CategoryOptionalInput.Slug));
    }

    [Fact]
    public async Task ValidCategorySlug_Optional_WithTooLongSlug_ShouldHaveError()
    {
        var validator = new CategoryOptionalSlugValidator();
        string tooLong = new('a', ContentConstants.MaxCategorySlugLength + 1);
        ValidationResult result = await validator.ValidateAsync(new CategoryOptionalInput(Name: null, Slug: tooLong));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CategoryOptionalInput.Slug));
    }

    #endregion
}
