using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo;

/// <summary>
/// Unit tests for <see cref="AdminUpdateVideoSeoValidator"/>.
/// </summary>
public class AdminUpdateVideoSeoValidatorTests
{
    private readonly ArticleErrorMessage _i18n = LocalizerFactory.CreateMessage<ArticleErrorMessage>();

    private readonly AdminUpdateVideoSeoValidator _validator;

    public AdminUpdateVideoSeoValidatorTests()
    {
        _validator = new AdminUpdateVideoSeoValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateVideoSeoCommand(
            Id: Guid.NewGuid().ToString(),
            MetaTitle: null,
            MetaDescription: null
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
        var command = new AdminUpdateVideoSeoCommand(Id: string.Empty, MetaTitle: null, MetaDescription: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateVideoSeoCommand.Id)
                && e.ErrorMessage == _i18n.Localizer["VideoIdRequired"].Value
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateVideoSeoCommand(Id: "not-a-guid", MetaTitle: null, MetaDescription: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateVideoSeoCommand.Id)
                && e.ErrorMessage == _i18n.Localizer["VideoIdInvalid"].Value
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
        var i18n = LocalizerFactory.CreateMessage<ArticleErrorMessage>(culture);
        var validator = new AdminUpdateVideoSeoValidator(i18n);
        var command = new AdminUpdateVideoSeoCommand(Id: string.Empty, MetaTitle: null, MetaDescription: null);

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateVideoSeoCommand.Id)
                && e.ErrorMessage == i18n.Localizer["VideoIdRequired"].Value
            );
    }

    #endregion
}
