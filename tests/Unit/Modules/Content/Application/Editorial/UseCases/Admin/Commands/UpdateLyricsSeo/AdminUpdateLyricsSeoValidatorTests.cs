using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsSeo;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsSeo;

/// <summary>
/// Unit tests for <see cref="AdminUpdateLyricsSeoValidator"/>.
/// </summary>
public class AdminUpdateLyricsSeoValidatorTests
{
    private readonly LyricsErrorMessage _i18n = LocalizerFactory.CreateMessage<LyricsErrorMessage>();

    private readonly AdminUpdateLyricsSeoValidator _validator;

    public AdminUpdateLyricsSeoValidatorTests()
    {
        _validator = new AdminUpdateLyricsSeoValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateLyricsSeoCommand(
            Id: Guid.NewGuid().ToString(),
            MetaTitle: null,
            MetaDescription: null,
            StructuredData: null
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
        var command = new AdminUpdateLyricsSeoCommand(
            Id: string.Empty,
            MetaTitle: null,
            MetaDescription: null,
            StructuredData: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsSeoCommand.Id)
                && e.ErrorMessage == _i18n.Localizer["IdRequired"].Value
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateLyricsSeoCommand(
            Id: "not-a-guid",
            MetaTitle: null,
            MetaDescription: null,
            StructuredData: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsSeoCommand.Id)
                && e.ErrorMessage == _i18n.Localizer["IdInvalid"].Value
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
        var i18n = LocalizerFactory.CreateMessage<LyricsErrorMessage>(culture);
        var validator = new AdminUpdateLyricsSeoValidator(i18n);
        var command = new AdminUpdateLyricsSeoCommand(
            Id: string.Empty,
            MetaTitle: null,
            MetaDescription: null,
            StructuredData: null
        );

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsSeoCommand.Id)
                && e.ErrorMessage == i18n.Localizer["IdRequired"].Value
            );
    }

    #endregion
}
