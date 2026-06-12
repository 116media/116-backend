using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreatePackage;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.CreatePackage;

/// <summary>
/// Unit tests for <see cref="AdminCreatePackageValidator"/>.
/// </summary>
public class AdminCreatePackageValidatorTests
{
    private readonly PackageErrorMessage _i18n = LocalizerFactory.CreateMessage<PackageErrorMessage>();
    private readonly AdminCreatePackageValidator _validator;

    public AdminCreatePackageValidatorTests()
    {
        _validator = new AdminCreatePackageValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreatePackageCommand(
            Name: TestConstants.Content.Package.ValidName,
            Description: TestConstants.Content.Package.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Name Validation Tests

    [Fact]
    public async Task Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePackageCommand(
            Name: string.Empty,
            Description: TestConstants.Content.Package.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreatePackageCommand.Name) && e.ErrorMessage == _i18n.NameRequired()
            );
    }

    [Fact]
    public async Task Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePackageCommand(
            Name: new string('a', TestConstants.Content.Package.NameMaxLength + 1),
            Description: TestConstants.Content.Package.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreatePackageCommand.Name)
                && e.ErrorMessage == _i18n.NameTooLong(TestConstants.Content.Package.NameMaxLength)
            );
    }

    #endregion

    #region Description Validation Tests

    [Fact]
    public async Task Validate_WithDescriptionExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePackageCommand(
            Name: TestConstants.Content.Package.ValidName,
            Description: new string('d', TestConstants.Content.Package.DescriptionMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreatePackageCommand.Description)
                && e.ErrorMessage == _i18n.DescriptionTooLong(TestConstants.Content.Package.DescriptionMaxLength)
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
        var i18n = LocalizerFactory.CreateMessage<PackageErrorMessage>(culture);
        var validator = new AdminCreatePackageValidator(i18n);
        var command = new AdminCreatePackageCommand(
            Name: string.Empty,
            Description: TestConstants.Content.Package.ValidDescription
        );

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreatePackageCommand.Name) && e.ErrorMessage == i18n.NameRequired()
            );
    }

    #endregion
}
