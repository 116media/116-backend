using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags;

/// <summary>
/// Unit tests for <see cref="AdminUpdateArticleTagsValidator"/>.
/// </summary>
public class AdminUpdateArticleTagsValidatorTests
{
    private readonly AdminUpdateArticleTagsValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateArticleTagsCommand(
            ArticleId: Guid.NewGuid().ToString(),
            TagIds: new List<Guid> { Guid.NewGuid() }
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithEmptyTagIds_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateArticleTagsCommand(ArticleId: Guid.NewGuid().ToString(), TagIds: new List<Guid>());

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
        var command = new AdminUpdateArticleTagsCommand(ArticleId: string.Empty, TagIds: new List<Guid>());

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
        var command = new AdminUpdateArticleTagsCommand(ArticleId: "not-a-guid", TagIds: new List<Guid>());

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
}
