using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectArticle;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RejectArticle;

/// <summary>
/// Unit tests for <see cref="AdminRejectArticleValidator"/>.
/// </summary>
public class AdminRejectArticleValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminRejectArticleValidator _validator;

    public AdminRejectArticleValidatorTests()
    {
        _validator = new AdminRejectArticleValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminRejectArticleCommand(
            Id: Guid.NewGuid().ToString(),
            Reason: TestConstants.Article.ValidRejectionReason
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
        var command = new AdminRejectArticleCommand(
            Id: string.Empty,
            Reason: TestConstants.Article.ValidRejectionReason
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminRejectArticleCommand.Id)
                && e.ErrorMessage == _i18n.Article.Msg.Localizer["IdRequired"].Value
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminRejectArticleCommand(
            Id: "not-a-guid",
            Reason: TestConstants.Article.ValidRejectionReason
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminRejectArticleCommand.Id)
                && e.ErrorMessage == _i18n.Article.Msg.Localizer["IdInvalid"].Value
            );
    }

    #endregion

    #region Reason Validation Tests

    [Fact]
    public async Task Validate_WithEmptyReason_ShouldHaveError()
    {
        // Arrange
        var command = new AdminRejectArticleCommand(Id: Guid.NewGuid().ToString(), Reason: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminRejectArticleCommand.Reason)
                && e.ErrorMessage == _i18n.Article.Msg.RejectionReasonRequired()
            );
    }

    [Fact]
    public async Task Validate_WithReasonExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminRejectArticleCommand(
            Id: Guid.NewGuid().ToString(),
            Reason: new string('a', TestConstants.Article.RejectionReasonMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminRejectArticleCommand.Reason)
                && e.ErrorMessage
                    == _i18n.Article.Msg.RejectionReasonTooLong(TestConstants.Article.RejectionReasonMaxLength)
            );
    }

    #endregion
}
