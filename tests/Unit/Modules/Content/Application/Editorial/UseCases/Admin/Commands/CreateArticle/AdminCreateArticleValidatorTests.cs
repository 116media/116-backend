using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle;
using _116.Content.Application.Shared.Errors.Messages;
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
    private readonly ArticleErrorMessage _articleI18n = LocalizerFactory.CreateMessage<ArticleErrorMessage>();
    private readonly ContentOrderErrorMessage _orderI18n = LocalizerFactory.CreateMessage<ContentOrderErrorMessage>();
    private readonly CustomerErrorMessage _customerI18n = LocalizerFactory.CreateMessage<CustomerErrorMessage>();

    private readonly AdminCreateArticleValidator _validator;

    public AdminCreateArticleValidatorTests()
    {
        _validator = new AdminCreateArticleValidator(_articleI18n, _orderI18n, _customerI18n);
    }

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
                && e.ErrorMessage == _articleI18n.CategoryIdRequired()
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
                && e.ErrorMessage == _articleI18n.TitleRequired()
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
                && e.ErrorMessage == _articleI18n.TitleTooLong(TestConstants.Content.Editorial.Article.TitleMaxLength)
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
                && e.ErrorMessage == _articleI18n.SlugRequired()
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
                && e.ErrorMessage == _articleI18n.SlugTooLong(TestConstants.Content.Editorial.Article.SlugMaxLength)
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
                && e.ErrorMessage == _articleI18n.SlugInvalidFormat()
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
                && e.ErrorMessage == _orderI18n.OrderItemIdRequired()
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
                && e.ErrorMessage == _customerI18n.CustomerIdRequired()
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
        var articleI18n = LocalizerFactory.CreateMessage<ArticleErrorMessage>(culture);
        var orderI18n = LocalizerFactory.CreateMessage<ContentOrderErrorMessage>(culture);
        var customerI18n = LocalizerFactory.CreateMessage<CustomerErrorMessage>(culture);
        var validator = new AdminCreateArticleValidator(articleI18n, orderI18n, customerI18n);
        var command = new AdminCreateArticleCommand(
            CategoryId: Guid.NewGuid(),
            Title: string.Empty,
            Slug: TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null
        );

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateArticleCommand.Title)
                && e.ErrorMessage == articleI18n.TitleRequired()
            );
    }

    #endregion
}
