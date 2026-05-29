using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateTag;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.CreateTag;

/// <summary>
/// Unit tests for <see cref="AdminCreateTagValidator"/>.
/// </summary>
public class AdminCreateTagValidatorTests
{
    private readonly TagErrorMessage _i18n = LocalizerFactory.CreateMessage<TagErrorMessage>();
    private readonly AdminCreateTagValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreateTagValidatorTests"/>.
    /// </summary>
    public AdminCreateTagValidatorTests()
    {
        _validator = new AdminCreateTagValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidNameAndSlug_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreateTagCommand(
            Name: TestConstants.Content.Tag.ValidName,
            Slug: TestConstants.Content.Tag.ValidSlug
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithSlugContainingNumbers_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreateTagCommand(Name: TestConstants.Content.Tag.ValidName, Slug: "fally-ipupa-123");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithMaxLengthNameAndSlug_ShouldNotHaveErrors()
    {
        // Arrange
        string maxLengthSlug = new('a', TestConstants.Content.Tag.SlugMaxLength);
        var command = new AdminCreateTagCommand(
            Name: new string('a', TestConstants.Content.Tag.NameMaxLength),
            Slug: maxLengthSlug
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Name Validation Tests

    [Fact]
    public async Task Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateTagCommand(Name: string.Empty, Slug: TestConstants.Content.Tag.ValidSlug);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreateTagCommand.Name) && e.ErrorMessage == _i18n.NameRequired()
            );
    }

    [Fact]
    public async Task Validate_WithNullName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateTagCommand(Name: null!, Slug: TestConstants.Content.Tag.ValidSlug);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateTagCommand.Name) && e.ErrorMessage == _i18n.NameRequired()
            );
    }

    [Fact]
    public async Task Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateTagCommand(
            Name: new string('a', TestConstants.Content.Tag.NameMaxLength + 1),
            Slug: TestConstants.Content.Tag.ValidSlug
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreateTagCommand.Name)
                && e.ErrorMessage == _i18n.NameTooLong(TestConstants.Content.Tag.NameMaxLength)
            );
    }

    #endregion

    #region Slug Validation Tests

    [Fact]
    public async Task Validate_WithEmptySlug_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateTagCommand(Name: TestConstants.Content.Tag.ValidName, Slug: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreateTagCommand.Slug) && e.ErrorMessage == _i18n.SlugRequired()
            );
    }

    [Fact]
    public async Task Validate_WithSlugExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        string longSlug = new('a', TestConstants.Content.Tag.SlugMaxLength + 1);
        var command = new AdminCreateTagCommand(Name: TestConstants.Content.Tag.ValidName, Slug: longSlug);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreateTagCommand.Slug)
                && e.ErrorMessage == _i18n.SlugTooLong(TestConstants.Content.Tag.SlugMaxLength)
            );
    }

    [Fact]
    public async Task Validate_WithUppercaseSlug_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateTagCommand(Name: TestConstants.Content.Tag.ValidName, Slug: "Fally-Ipupa");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreateTagCommand.Slug) && e.ErrorMessage == _i18n.SlugInvalidFormat()
            );
    }

    [Fact]
    public async Task Validate_WithSlugContainingSpaces_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateTagCommand(Name: TestConstants.Content.Tag.ValidName, Slug: "fally ipupa");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreateTagCommand.Slug) && e.ErrorMessage == _i18n.SlugInvalidFormat()
            );
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithAllInvalidValues_ShouldHaveMultipleErrors()
    {
        // Arrange
        var command = new AdminCreateTagCommand(Name: string.Empty, Slug: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminCreateTagCommand.Name));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminCreateTagCommand.Slug));
    }

    #endregion

    #region Culture Tests

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
    {
        // Arrange
        var i18n = LocalizerFactory.CreateMessage<TagErrorMessage>(culture);
        var validator = new AdminCreateTagValidator(i18n);
        var command = new AdminCreateTagCommand(Name: "", Slug: TestConstants.Content.Tag.ValidSlug);

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateTagCommand.Name) && e.ErrorMessage == i18n.NameRequired()
            );
    }

    #endregion
}
