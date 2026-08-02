using System.Globalization;
using _116.Content.Application.Editorial.UseCases.Public.Commands.ProposeTranslationRevision;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Commands.ProposeTranslationRevision;

/// <summary>
/// Unit tests for <see cref="PublicProposeTranslationRevisionValidator"/>.
/// </summary>
public class PublicProposeTranslationRevisionValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly PublicProposeTranslationRevisionValidator _validator;

    public PublicProposeTranslationRevisionValidatorTests()
    {
        _validator = new PublicProposeTranslationRevisionValidator(_i18n);
    }

    private static PublicProposeTranslationRevisionCommand BuildValidCommand(
        string? proposedText = null,
        string? editSummary = null
    ) =>
        new(
            TranslationId: Guid.NewGuid(),
            ProposedText: proposedText ?? TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
            EditSummary: editSummary,
            UserId: Guid.NewGuid()
        );

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = BuildValidCommand();

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithEditSummaryProvided_ShouldNotHaveErrors()
    {
        // Arrange
        var command = BuildValidCommand(editSummary: "Corrected the chorus translation.");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region ProposedText Validation Tests

    [Fact]
    public async Task Validate_WithEmptyProposedText_ShouldHaveError()
    {
        // Arrange
        var command = BuildValidCommand(proposedText: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(PublicProposeTranslationRevisionCommand.ProposedText)
                && e.ErrorMessage == _i18n.Translation.Msg.ProposedTextRequired()
            );
    }

    [Fact]
    public async Task Validate_WithNullProposedText_ShouldHaveError()
    {
        // Arrange
        var command = new PublicProposeTranslationRevisionCommand(
            TranslationId: Guid.NewGuid(),
            ProposedText: null!,
            EditSummary: null,
            UserId: Guid.NewGuid()
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e => e.PropertyName == nameof(PublicProposeTranslationRevisionCommand.ProposedText));
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
        var validator = new PublicProposeTranslationRevisionValidator(_i18n);
        var command = BuildValidCommand(proposedText: string.Empty);

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(PublicProposeTranslationRevisionCommand.ProposedText)
                && e.ErrorMessage == _i18n.Translation.Msg.ProposedTextRequired()
            );
    }

    #endregion
}
