using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreatePackage;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.CreatePackage;

/// <summary>
/// Unit tests for <see cref="CreatePackageValidator"/>.
/// </summary>
public class CreatePackageValidatorTests
{
    private readonly CreatePackageValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreatePackageCommand(
            Name: TestConstants.Content.Package.ValidName,
            Description: TestConstants.Content.Package.ValidDescription,
            FlatPriceUsd: TestConstants.Content.Package.ValidFlatPriceUsd
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithZeroPrice_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreatePackageCommand(
            Name: TestConstants.Content.Package.ValidName,
            Description: null,
            FlatPriceUsd: TestConstants.Content.Package.ZeroFlatPriceUsd
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
        var command = new CreatePackageCommand(
            Name: string.Empty,
            Description: null,
            FlatPriceUsd: TestConstants.Content.Package.ValidFlatPriceUsd
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(CreatePackageCommand.Name) && e.ErrorMessage == "Package name is required."
            );
    }

    [Fact]
    public async Task Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new CreatePackageCommand(
            Name: new string('a', TestConstants.Content.Package.NameMaxLength + 1),
            Description: null,
            FlatPriceUsd: TestConstants.Content.Package.ValidFlatPriceUsd
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(CreatePackageCommand.Name)
                && e.ErrorMessage == "Package name must not exceed 100 characters."
            );
    }

    #endregion

    #region Description Validation Tests

    [Fact]
    public async Task Validate_WithDescriptionExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new CreatePackageCommand(
            Name: TestConstants.Content.Package.ValidName,
            Description: new string('d', TestConstants.Content.Package.DescriptionMaxLength + 1),
            FlatPriceUsd: TestConstants.Content.Package.ValidFlatPriceUsd
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(CreatePackageCommand.Description)
                && e.ErrorMessage == "Package description must not exceed 500 characters."
            );
    }

    #endregion

    #region FlatPriceUsd Validation Tests

    [Fact]
    public async Task Validate_WithNegativePrice_ShouldHaveError()
    {
        // Arrange
        var command = new CreatePackageCommand(
            Name: TestConstants.Content.Package.ValidName,
            Description: null,
            FlatPriceUsd: -1m
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(CreatePackageCommand.FlatPriceUsd)
                && e.ErrorMessage == "Package price must be zero or greater."
            );
    }

    #endregion
}
