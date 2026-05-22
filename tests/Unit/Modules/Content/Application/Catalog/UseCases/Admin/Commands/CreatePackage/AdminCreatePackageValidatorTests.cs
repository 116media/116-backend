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
    private readonly AdminCreatePackageValidator _validator = new(
        LocalizerFactory.CreateMessage<PackageErrorMessage>()
    );

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
                e.PropertyName == nameof(AdminCreatePackageCommand.Name)
                && e.ErrorMessage == "Package name is required."
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
                && e.ErrorMessage == "Package name must not exceed 100 characters."
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
                && e.ErrorMessage == "Package description must not exceed 500 characters."
            );
    }

    #endregion
}
