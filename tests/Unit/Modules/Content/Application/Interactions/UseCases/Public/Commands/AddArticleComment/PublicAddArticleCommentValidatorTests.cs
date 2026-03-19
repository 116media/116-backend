using _116.Content.Application.Interactions.UseCases.Public.Commands.AddArticleComment;
using _116.Content.Domain.Constants;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.AddArticleComment;

/// <summary>
/// Unit tests for <see cref="PublicAddArticleCommentValidator"/>.
/// </summary>
public class PublicAddArticleCommentValidatorTests
{
    private readonly PublicAddArticleCommentValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WhenBodyIsValid_ShouldPass()
    {
        // Arrange
        var command = new PublicAddArticleCommentCommand(
            ArticleId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Body: TestConstants.Content.Interactions.ValidCommentBody
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Body Validation Tests

    [Fact]
    public async Task Validate_WhenBodyIsEmpty_ShouldFail()
    {
        // Arrange
        var command = new PublicAddArticleCommentCommand(ArticleId: Guid.NewGuid(), UserId: Guid.NewGuid(), Body: "");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PublicAddArticleCommentCommand.Body));
    }

    [Fact]
    public async Task Validate_WhenBodyIsWhiteSpace_ShouldFail()
    {
        // Arrange
        var command = new PublicAddArticleCommentCommand(
            ArticleId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Body: "   "
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_WhenBodyExceedsMaxLength_ShouldFail()
    {
        // Arrange
        var command = new PublicAddArticleCommentCommand(
            ArticleId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Body: new string('a', ContentConstants.MaxCommentBodyLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion
}
