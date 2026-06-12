using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags;

/// <summary>
/// Unit tests for <see cref="AdminUpdateArticleTagsValidator"/>.
/// </summary>
public class AdminUpdateArticleTagsValidatorTests
{
    private readonly TagErrorMessage _tagI18n = LocalizerFactory.CreateMessage<TagErrorMessage>();
    private readonly ArticleErrorMessage _articleI18n = LocalizerFactory.CreateMessage<ArticleErrorMessage>();

    private readonly AdminUpdateArticleTagsValidator _validator;

    public AdminUpdateArticleTagsValidatorTests()
    {
        _validator = new AdminUpdateArticleTagsValidator(_tagI18n, _articleI18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidTagNames_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateArticleTagsCommand(
            ArticleId: Guid.NewGuid().ToString(),
            TagNames: new List<string> { "Fally Ipupa", "Kinshasa" }
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithEmptyTagNames_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateArticleTagsCommand(
            ArticleId: Guid.NewGuid().ToString(),
            TagNames: new List<string>()
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region ArticleId Validation Tests

    [Fact]
    public async Task Validate_WithEmptyArticleId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateArticleTagsCommand(ArticleId: string.Empty, TagNames: new List<string>());

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleTagsCommand.ArticleId)
                && e.ErrorMessage == _articleI18n.Localizer["IdRequired"].Value
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidArticleId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateArticleTagsCommand(ArticleId: "not-a-guid", TagNames: new List<string>());

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleTagsCommand.ArticleId)
                && e.ErrorMessage == _articleI18n.Localizer["IdInvalid"].Value
            );
    }

    #endregion

    #region TagNames Validation Tests

    [Fact]
    public async Task Validate_WithEmptyTagName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateArticleTagsCommand(
            ArticleId: Guid.NewGuid().ToString(),
            TagNames: new List<string> { string.Empty }
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == _tagI18n.NameRequired());
    }

    [Fact]
    public async Task Validate_WithTagNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateArticleTagsCommand(
            ArticleId: Guid.NewGuid().ToString(),
            TagNames: new List<string> { new string('a', 51) }
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains(_tagI18n.NameTooLong(50)));
    }

    #endregion

    #region Culture Tests

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
    {
        // Arrange
        var tagI18n = LocalizerFactory.CreateMessage<TagErrorMessage>(culture);
        var articleI18n = LocalizerFactory.CreateMessage<ArticleErrorMessage>(culture);
        var validator = new AdminUpdateArticleTagsValidator(tagI18n, articleI18n);
        var command = new AdminUpdateArticleTagsCommand(ArticleId: string.Empty, TagNames: new List<string>());

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleTagsCommand.ArticleId)
                && e.ErrorMessage == articleI18n.Localizer["IdRequired"].Value
            );
    }

    #endregion
}
