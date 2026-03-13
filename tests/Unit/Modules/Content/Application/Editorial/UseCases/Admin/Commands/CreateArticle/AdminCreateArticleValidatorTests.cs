using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle;

/// <summary>
/// Unit tests for <see cref="AdminCreateArticleValidator"/>.
/// </summary>
public class AdminCreateArticleValidatorTests
{
    private readonly AdminCreateArticleValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreateArticleCommand(
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Content.Editorial.Article.ValidTitle,
            Slug: TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithBothCustomerIdAndOrderItemId_ShouldNotHaveErrors()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();
        var command = new AdminCreateArticleCommand(
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Content.Editorial.Article.ValidTitle,
            Slug: TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: customerId,
            OrderItemId: orderItemId
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region CategoryId Validation Tests

    [Fact]
    public async Task Validate_WithEmptyCategoryId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateArticleCommand(
            CategoryId: Guid.Empty,
            Title: TestConstants.Content.Editorial.Article.ValidTitle,
            Slug: TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateArticleCommand.CategoryId)
                && e.ErrorMessage == "Category ID is required."
            );
    }

    #endregion

    #region Title Validation Tests

    [Fact]
    public async Task Validate_WithEmptyTitle_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateArticleCommand(
            CategoryId: Guid.NewGuid(),
            Title: string.Empty,
            Slug: TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateArticleCommand.Title)
                && e.ErrorMessage == "Article title is required."
            );
    }

    [Fact]
    public async Task Validate_WithTitleExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateArticleCommand(
            CategoryId: Guid.NewGuid(),
            Title: new string('a', TestConstants.Content.Editorial.Article.TitleMaxLength + 1),
            Slug: TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateArticleCommand.Title)
                && e.ErrorMessage == "Article title must not exceed 100 characters."
            );
    }

    #endregion

    #region Slug Validation Tests

    [Fact]
    public async Task Validate_WithEmptySlug_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateArticleCommand(
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Content.Editorial.Article.ValidTitle,
            Slug: string.Empty,
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateArticleCommand.Slug)
                && e.ErrorMessage == "Article slug is required."
            );
    }

    [Fact]
    public async Task Validate_WithSlugExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateArticleCommand(
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Content.Editorial.Article.ValidTitle,
            Slug: new string('a', TestConstants.Content.Editorial.Article.SlugMaxLength + 1),
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateArticleCommand.Slug)
                && e.ErrorMessage == "Article slug must not exceed 220 characters."
            );
    }

    [Fact]
    public async Task Validate_WithUppercaseSlug_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateArticleCommand(
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Content.Editorial.Article.ValidTitle,
            Slug: "Invalid-Slug",
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateArticleCommand.Slug)
                && e.ErrorMessage == "Article slug must be lowercase and contain only letters, numbers, and hyphens."
            );
    }

    #endregion

    #region Conditional OrderItemId/CustomerId Validation Tests

    [Fact]
    public async Task Validate_WithCustomerIdButNoOrderItemId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateArticleCommand(
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Content.Editorial.Article.ValidTitle,
            Slug: TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            OrderItemId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateArticleCommand.OrderItemId)
                && e.ErrorMessage == "Order item ID is required when customer ID is provided."
            );
    }

    [Fact]
    public async Task Validate_WithOrderItemIdButNoCustomerId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateArticleCommand(
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Content.Editorial.Article.ValidTitle,
            Slug: TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: Guid.NewGuid()
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateArticleCommand.CustomerId)
                && e.ErrorMessage == "Customer ID is required when order item ID is provided."
            );
    }

    #endregion
}
