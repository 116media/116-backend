using System.Globalization;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType;

/// <summary>
/// Unit tests for <see cref="AdminCreateContentTypeValidator"/>.
/// </summary>
public class AdminCreateContentTypeValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminCreateContentTypeValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreateContentTypeValidatorTests"/>.
    /// </summary>
    public AdminCreateContentTypeValidatorTests()
    {
        _validator = new AdminCreateContentTypeValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidName_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreateContentTypeCommand(Name: TestConstants.Content.ContentType.ValidName);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithMaxLengthName_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreateContentTypeCommand(
            Name: new string('a', TestConstants.Content.ContentType.NameMaxLength)
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
        var command = new AdminCreateContentTypeCommand(Name: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreateContentTypeCommand.Name)
                && e.ErrorMessage == _i18n.ContentType.Msg.NameRequired()
            );
    }

    [Fact]
    public async Task Validate_WithNullName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateContentTypeCommand(Name: null!);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateContentTypeCommand.Name)
                && e.ErrorMessage == _i18n.ContentType.Msg.NameRequired()
            );
    }

    [Fact]
    public async Task Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateContentTypeCommand(
            Name: new string('a', TestConstants.Content.ContentType.NameMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreateContentTypeCommand.Name)
                && e.ErrorMessage == _i18n.ContentType.Msg.NameTooLong(TestConstants.Content.ContentType.NameMaxLength)
            );
    }

    #endregion

    #region Culture Tests

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
    {
        // Arrange
        Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
        var validator = new AdminCreateContentTypeValidator(_i18n);
        var command = new AdminCreateContentTypeCommand(Name: "");

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateContentTypeCommand.Name)
                && e.ErrorMessage == _i18n.ContentType.Msg.NameRequired()
            );
    }

    #endregion
}
