using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag;

/// <summary>
/// Unit tests for <see cref="AdminUpdateTagValidator"/>.
/// </summary>
public class AdminUpdateTagValidatorTests
{
    private readonly TagErrorMessage _i18n = LocalizerFactory.CreateMessage<TagErrorMessage>();
    private readonly AdminUpdateTagValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdateTagValidatorTests"/>.
    /// </summary>
    public AdminUpdateTagValidatorTests()
    {
        _validator = new AdminUpdateTagValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateTagCommand(
            Id: Guid.NewGuid().ToString(),
            Name: TestConstants.Content.Tag.ValidName,
            Slug: TestConstants.Content.Tag.ValidSlug
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Id Validation Tests

    [Fact]
    public async Task Validate_WithEmptyId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateTagCommand(
            Id: string.Empty,
            Name: TestConstants.Content.Tag.ValidName,
            Slug: TestConstants.Content.Tag.ValidSlug
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateTagCommand.Id));
    }

    [Fact]
    public async Task Validate_WithInvalidGuidId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateTagCommand(
            Id: "not-a-valid-guid",
            Name: TestConstants.Content.Tag.ValidName,
            Slug: TestConstants.Content.Tag.ValidSlug
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateTagCommand.Id));
    }

    #endregion

    #region Name Validation Tests

    [Fact]
    public async Task Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateTagCommand(
            Id: Guid.NewGuid().ToString(),
            Name: string.Empty,
            Slug: TestConstants.Content.Tag.ValidSlug
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminUpdateTagCommand.Name) && e.ErrorMessage == _i18n.NameRequired()
            );
    }

    #endregion

    #region Slug Validation Tests

    [Fact]
    public async Task Validate_WithEmptySlug_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateTagCommand(
            Id: Guid.NewGuid().ToString(),
            Name: TestConstants.Content.Tag.ValidName,
            Slug: string.Empty
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminUpdateTagCommand.Slug) && e.ErrorMessage == _i18n.SlugRequired()
            );
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithAllInvalidValues_ShouldHaveMultipleErrors()
    {
        // Arrange
        var command = new AdminUpdateTagCommand(Id: string.Empty, Name: string.Empty, Slug: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateTagCommand.Id));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateTagCommand.Name));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateTagCommand.Slug));
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
        var validator = new AdminUpdateTagValidator(i18n);
        var command = new AdminUpdateTagCommand(
            Id: Guid.NewGuid().ToString(),
            Name: "",
            Slug: TestConstants.Content.Tag.ValidSlug
        );

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateTagCommand.Name) && e.ErrorMessage == i18n.NameRequired()
            );
    }

    #endregion
}
