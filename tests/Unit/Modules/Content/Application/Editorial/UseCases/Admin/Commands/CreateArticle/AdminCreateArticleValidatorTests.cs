using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle;

/// <summary>
/// Unit tests for <see cref="AdminCreateArticleValidator"/>.
/// </summary>
public class AdminCreateArticleValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminCreateArticleValidator _validator;

    public AdminCreateArticleValidatorTests()
    {
        _validator = new AdminCreateArticleValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreateArticleCommand(
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Article.ValidTitle,
            Slug: TestConstants.Article.ValidSlug,
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
            Title: TestConstants.Article.ValidTitle,
            Slug: TestConstants.Article.ValidSlug,
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
            Title: TestConstants.Article.ValidTitle,
            Slug: TestConstants.Article.ValidSlug,
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
                && e.ErrorMessage == _i18n.Article.Msg.CategoryIdRequired()
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
            Slug: TestConstants.Article.ValidSlug,
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
                && e.ErrorMessage == _i18n.Article.Msg.TitleRequired()
            );
    }

    [Fact]
    public async Task Validate_WithTitleExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateArticleCommand(
            CategoryId: Guid.NewGuid(),
            Title: new string('a', TestConstants.Article.TitleMaxLength + 1),
            Slug: TestConstants.Article.ValidSlug,
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
                && e.ErrorMessage == _i18n.Article.Msg.TitleTooLong(TestConstants.Article.TitleMaxLength)
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
            Title: TestConstants.Article.ValidTitle,
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
                && e.ErrorMessage == _i18n.Article.Msg.SlugRequired()
            );
    }

    [Fact]
    public async Task Validate_WithSlugExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateArticleCommand(
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Article.ValidTitle,
            Slug: new string('a', TestConstants.Article.SlugMaxLength + 1),
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
                && e.ErrorMessage == _i18n.Article.Msg.SlugTooLong(TestConstants.Article.SlugMaxLength)
            );
    }

    [Fact]
    public async Task Validate_WithUppercaseSlug_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateArticleCommand(
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Article.ValidTitle,
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
                && e.ErrorMessage == _i18n.Article.Msg.SlugInvalidFormat()
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
            Title: TestConstants.Article.ValidTitle,
            Slug: TestConstants.Article.ValidSlug,
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
                && e.ErrorMessage == _i18n.ContentOrder.Msg.OrderItemIdRequired()
            );
    }

    [Fact]
    public async Task Validate_WithOrderItemIdButNoCustomerId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateArticleCommand(
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Article.ValidTitle,
            Slug: TestConstants.Article.ValidSlug,
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
                && e.ErrorMessage == _i18n.Customer.Msg.CustomerIdRequired()
            );
    }

    #endregion
}
