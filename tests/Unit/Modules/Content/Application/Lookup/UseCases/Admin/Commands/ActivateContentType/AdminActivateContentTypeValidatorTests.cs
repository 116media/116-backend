using _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivateContentType;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.ActivateContentType;

/// <summary>
/// Unit tests for <see cref="AdminActivateContentTypeValidator"/>.
/// </summary>
public class AdminActivateContentTypeValidatorTests
{
    private readonly ContentTypeErrorMessage _i18n = LocalizerFactory.CreateMessage<ContentTypeErrorMessage>();
    private readonly AdminActivateContentTypeValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminActivateContentTypeValidatorTests"/>.
    /// </summary>
    public AdminActivateContentTypeValidatorTests()
    {
        _validator = new AdminActivateContentTypeValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidGuid_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminActivateContentTypeCommand(Id: Guid.NewGuid().ToString());

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Id Validation Tests

    [Fact]
    public async Task Validate_WithEmptyGuid_ShouldHaveError()
    {
        // Arrange
        var command = new AdminActivateContentTypeCommand(Id: "");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminActivateContentTypeCommand.Id)
                && e.ErrorMessage == _i18n.Localizer["IdRequired"].Value
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
        var i18n = LocalizerFactory.CreateMessage<ContentTypeErrorMessage>(culture);
        var validator = new AdminActivateContentTypeValidator(i18n);
        var command = new AdminActivateContentTypeCommand(Id: "");

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminActivateContentTypeCommand.Id)
                && e.ErrorMessage == i18n.Localizer["IdRequired"].Value
            );
    }

    #endregion
}
