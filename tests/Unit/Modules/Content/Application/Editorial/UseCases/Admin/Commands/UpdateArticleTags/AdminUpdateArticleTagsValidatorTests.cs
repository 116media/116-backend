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
    private readonly AdminUpdateArticleTagsValidator _validator = new(
        LocalizerFactory.CreateMessage<TagErrorMessage>()
    );

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
                && e.ErrorMessage == "Article ID is required."
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
                && e.ErrorMessage == "Article ID is invalid."
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
        result.Errors.Should().Contain(e => e.ErrorMessage == "Tag name is required.");
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
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Tag name must not exceed 50 characters"));
    }

    #endregion
}
