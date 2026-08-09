using _116.Content.Application.Catalog.UseCases.Admin.Commands.DeactivatePackage;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.DeactivatePackage;

/// <summary>
/// Unit tests for <see cref="AdminDeactivatePackageValidator"/>.
/// </summary>
public class AdminDeactivatePackageValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminDeactivatePackageValidator _validator;

    public AdminDeactivatePackageValidatorTests()
    {
        _validator = new AdminDeactivatePackageValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidId_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminDeactivatePackageCommand(Id: Guid.NewGuid().ToString());

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
        var command = new AdminDeactivatePackageCommand(Id: "");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminDeactivatePackageCommand.Id)
                && e.ErrorMessage == _i18n.Package.Msg.Localizer["IdRequired"].Value
            );
    }

    #endregion
}
