using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle;

/// <summary>
/// Unit tests for <see cref="AdminUpdateArticleValidator"/>.
/// </summary>
public class AdminUpdateArticleValidatorTests
{
    private readonly ArticleErrorMessage _articleI18n = LocalizerFactory.CreateMessage<ArticleErrorMessage>();
    private readonly ContentOrderErrorMessage _orderI18n = LocalizerFactory.CreateMessage<ContentOrderErrorMessage>();
    private readonly CustomerErrorMessage _customerI18n = LocalizerFactory.CreateMessage<CustomerErrorMessage>();

    private readonly AdminUpdateArticleValidator _validator;

    public AdminUpdateArticleValidatorTests()
    {
        _validator = new AdminUpdateArticleValidator(_articleI18n, _orderI18n, _customerI18n);
    }

    private static AdminUpdateArticleCommand ValidCommand() =>
        new(
            Id: Guid.NewGuid().ToString(),
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Content.Editorial.Article.ValidTitle,
            Slug: TestConstants.Content.Editorial.Article.ValidSlug,
            Headline: TestConstants.Content.Editorial.Article.ValidHeadline,
            Body: TestConstants.Content.Editorial.Article.ValidBody,
            CoverImageUrl: null,
            CustomerId: null,
            OrderItemId: null,
            SocialBoost: false,
            MetaTitle: null,
            MetaDescription: null
        );

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = ValidCommand();

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
        var command = ValidCommand() with
        {
            Id = string.Empty,
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleCommand.Id)
                && e.ErrorMessage == _articleI18n.Localizer["IdRequired"].Value
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidId_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            Id = "not-a-guid",
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleCommand.Id)
                && e.ErrorMessage == _articleI18n.Localizer["IdInvalid"].Value
            );
    }

    #endregion

    #region CategoryId Validation Tests

    [Fact]
    public async Task Validate_WithEmptyCategoryId_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            CategoryId = Guid.Empty,
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleCommand.CategoryId)
                && e.ErrorMessage == _articleI18n.CategoryIdRequired()
            );
    }

    #endregion

    #region Title Validation Tests

    [Fact]
    public async Task Validate_WithEmptyTitle_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            Title = string.Empty,
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleCommand.Title)
                && e.ErrorMessage == _articleI18n.TitleRequired()
            );
    }

    [Fact]
    public async Task Validate_WithTitleExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            Title = new string('a', TestConstants.Content.Editorial.Article.TitleMaxLength + 1),
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleCommand.Title)
                && e.ErrorMessage == _articleI18n.TitleTooLong(TestConstants.Content.Editorial.Article.TitleMaxLength)
            );
    }

    #endregion

    #region Slug Validation Tests

    [Fact]
    public async Task Validate_WithEmptySlug_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            Slug = string.Empty,
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleCommand.Slug)
                && e.ErrorMessage == _articleI18n.SlugRequired()
            );
    }

    [Fact]
    public async Task Validate_WithUppercaseSlug_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            Slug = "Invalid-Slug",
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleCommand.Slug)
                && e.ErrorMessage == _articleI18n.SlugInvalidFormat()
            );
    }

    #endregion

    #region Headline Validation Tests

    [Fact]
    public async Task Validate_WithEmptyHeadline_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            Headline = string.Empty,
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleCommand.Headline)
                && e.ErrorMessage == _articleI18n.HeadlineRequired()
            );
    }

    [Fact]
    public async Task Validate_WithHeadlineTooShort_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            Headline = new string('a', TestConstants.Content.Editorial.Article.HeadlineMinLength - 1),
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleCommand.Headline)
                && e.ErrorMessage
                    == _articleI18n.HeadlineTooShort(TestConstants.Content.Editorial.Article.HeadlineMinLength)
            );
    }

    [Fact]
    public async Task Validate_WithHeadlineExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            Headline = new string('a', TestConstants.Content.Editorial.Article.HeadlineMaxLength + 1),
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleCommand.Headline)
                && e.ErrorMessage
                    == _articleI18n.HeadlineTooLong(TestConstants.Content.Editorial.Article.HeadlineMaxLength)
            );
    }

    #endregion

    #region Body Validation Tests

    [Fact]
    public async Task Validate_WithEmptyBody_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            Body = string.Empty,
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleCommand.Body)
                && e.ErrorMessage == _articleI18n.BodyRequired()
            );
    }

    #endregion

    #region Conditional Validation Tests

    [Fact]
    public async Task Validate_WithCustomerIdButNoOrderItemId_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            CustomerId = Guid.NewGuid(),
            OrderItemId = null,
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleCommand.OrderItemId)
                && e.ErrorMessage == _orderI18n.OrderItemIdRequired()
            );
    }

    [Fact]
    public async Task Validate_WithOrderItemIdButNoCustomerId_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            CustomerId = null,
            OrderItemId = Guid.NewGuid(),
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleCommand.CustomerId)
                && e.ErrorMessage == _customerI18n.CustomerIdRequired()
            );
    }

    #endregion

    #region MetaTitle Validation Tests

    [Fact]
    public async Task Validate_WithMetaTitleTooShort_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            MetaTitle = new string('a', 5),
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleCommand.MetaTitle)
                && e.ErrorMessage == _articleI18n.MetaTitleTooShort(10)
            );
    }

    [Fact]
    public async Task Validate_WithMetaTitleExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            MetaTitle = new string('a', 71),
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleCommand.MetaTitle)
                && e.ErrorMessage == _articleI18n.MetaTitleTooLong(70)
            );
    }

    [Fact]
    public async Task Validate_WithNullMetaTitle_ShouldNotHaveErrors()
    {
        // Arrange
        var command = ValidCommand() with
        {
            MetaTitle = null,
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region MetaDescription Validation Tests

    [Fact]
    public async Task Validate_WithMetaDescriptionTooShort_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            MetaDescription = new string('a', 10),
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleCommand.MetaDescription)
                && e.ErrorMessage == _articleI18n.MetaDescriptionTooShort(50)
            );
    }

    [Fact]
    public async Task Validate_WithMetaDescriptionExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            MetaDescription = new string('a', 161),
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleCommand.MetaDescription)
                && e.ErrorMessage == _articleI18n.MetaDescriptionTooLong(160)
            );
    }

    [Fact]
    public async Task Validate_WithNullMetaDescription_ShouldNotHaveErrors()
    {
        // Arrange
        var command = ValidCommand() with
        {
            MetaDescription = null,
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
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
        var validator = new AdminUpdateArticleValidator(articleI18n, orderI18n, customerI18n);
        var command = ValidCommand() with { Title = string.Empty };

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleCommand.Title)
                && e.ErrorMessage == articleI18n.TitleRequired()
            );
    }

    #endregion
}
