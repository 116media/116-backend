using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle;

/// <summary>
/// Unit tests for <see cref="AdminForceUnpromoteArticleValidator"/>.
/// </summary>
public class AdminForceUnpromoteArticleValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminForceUnpromoteArticleValidator _validator;

    public AdminForceUnpromoteArticleValidatorTests()
    {
        _validator = new AdminForceUnpromoteArticleValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminForceUnpromoteArticleCommand(
            Slug: "my-article-slug",
            Reason: "Government takedown request."
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Slug Validation Tests

    [Fact]
    public async Task Validate_WithEmptySlug_ShouldHaveError()
    {
        // Arrange
        var command = new AdminForceUnpromoteArticleCommand(Slug: string.Empty, Reason: "Government takedown request.");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminForceUnpromoteArticleCommand.Slug)
                && e.ErrorMessage == _i18n.Article.Msg.SlugRequired()
            );
    }

    #endregion

    #region Reason Validation Tests

    [Fact]
    public async Task Validate_WithEmptyReason_ShouldHaveError()
    {
        // Arrange
        var command = new AdminForceUnpromoteArticleCommand(Slug: "my-article-slug", Reason: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminForceUnpromoteArticleCommand.Reason)
                && e.ErrorMessage == _i18n.Article.Msg.RejectionReasonRequired()
            );
    }

    [Fact]
    public async Task Validate_WithReasonExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminForceUnpromoteArticleCommand(Slug: "my-article-slug", Reason: new string('a', 501));

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminForceUnpromoteArticleCommand.Reason)
                && e.ErrorMessage == _i18n.Article.Msg.RejectionReasonTooLong(500)
            );
    }

    [Fact]
    public async Task Validate_WithReasonAtMaxLength_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminForceUnpromoteArticleCommand(Slug: "my-article-slug", Reason: new string('a', 500));

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion
}
