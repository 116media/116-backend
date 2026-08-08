using _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivatePackage;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.ActivatePackage;

/// <summary>
/// Unit tests for <see cref="AdminActivatePackageValidator"/>.
/// </summary>
public class AdminActivatePackageValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminActivatePackageValidator _validator;

    public AdminActivatePackageValidatorTests()
    {
        _validator = new AdminActivatePackageValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidId_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminActivatePackageCommand(Id: Guid.NewGuid().ToString());

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
        var command = new AdminActivatePackageCommand(Id: "");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminActivatePackageCommand.Id)
                && e.ErrorMessage == _i18n.Package.Msg.Localizer["IdRequired"].Value
            );
    }

    #endregion
}
