using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo;

/// <summary>
/// Unit tests for <see cref="AdminCreateVideoValidator"/>.
/// </summary>
public class AdminCreateVideoValidatorTests
{
    private readonly ArticleErrorMessage _articleI18n = LocalizerFactory.CreateMessage<ArticleErrorMessage>();
    private readonly VideoErrorMessage _videoI18n = LocalizerFactory.CreateMessage<VideoErrorMessage>();
    private readonly ContentOrderErrorMessage _orderI18n = LocalizerFactory.CreateMessage<ContentOrderErrorMessage>();
    private readonly CustomerErrorMessage _customerI18n = LocalizerFactory.CreateMessage<CustomerErrorMessage>();

    private readonly AdminCreateVideoValidator _validator;

    public AdminCreateVideoValidatorTests()
    {
        _validator = new AdminCreateVideoValidator(_articleI18n, _videoI18n, _orderI18n, _customerI18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreateVideoCommand(
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Content.Editorial.Video.ValidTitle,
            Slug: TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null,
            Description: TestConstants.Content.Editorial.Video.ValidDescription,
            ShootingScheduledAt: null
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
        var command = new AdminCreateVideoCommand(
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Content.Editorial.Video.ValidTitle,
            Slug: TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            OrderItemId: Guid.NewGuid(),
            Description: TestConstants.Content.Editorial.Video.ValidDescription,
            ShootingScheduledAt: null
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
        var command = new AdminCreateVideoCommand(
            CategoryId: Guid.Empty,
            Title: TestConstants.Content.Editorial.Video.ValidTitle,
            Slug: TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null,
            Description: TestConstants.Content.Editorial.Video.ValidDescription,
            ShootingScheduledAt: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateVideoCommand.CategoryId)
                && e.ErrorMessage == _articleI18n.CategoryIdRequired()
            );
    }

    #endregion

    #region Title Validation Tests

    [Fact]
    public async Task Validate_WithEmptyTitle_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateVideoCommand(
            CategoryId: Guid.NewGuid(),
            Title: string.Empty,
            Slug: TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null,
            Description: TestConstants.Content.Editorial.Video.ValidDescription,
            ShootingScheduledAt: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateVideoCommand.Title) && e.ErrorMessage == _videoI18n.TitleRequired()
            );
    }

    [Fact]
    public async Task Validate_WithTitleExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateVideoCommand(
            CategoryId: Guid.NewGuid(),
            Title: new string('a', TestConstants.Content.Editorial.Video.TitleMaxLength + 1),
            Slug: TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null,
            Description: TestConstants.Content.Editorial.Video.ValidDescription,
            ShootingScheduledAt: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateVideoCommand.Title)
                && e.ErrorMessage == _videoI18n.TitleTooLong(TestConstants.Content.Editorial.Video.TitleMaxLength)
            );
    }

    #endregion

    #region Slug Validation Tests

    [Fact]
    public async Task Validate_WithEmptySlug_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateVideoCommand(
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Content.Editorial.Video.ValidTitle,
            Slug: string.Empty,
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null,
            Description: TestConstants.Content.Editorial.Video.ValidDescription,
            ShootingScheduledAt: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateVideoCommand.Slug) && e.ErrorMessage == _videoI18n.SlugRequired()
            );
    }

    [Fact]
    public async Task Validate_WithUppercaseSlug_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateVideoCommand(
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Content.Editorial.Video.ValidTitle,
            Slug: "Invalid-Slug",
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null,
            Description: TestConstants.Content.Editorial.Video.ValidDescription,
            ShootingScheduledAt: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateVideoCommand.Slug)
                && e.ErrorMessage == _videoI18n.SlugInvalidFormat()
            );
    }

    #endregion

    #region Conditional Validation Tests

    [Fact]
    public async Task Validate_WithCustomerIdButNoOrderItemId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateVideoCommand(
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Content.Editorial.Video.ValidTitle,
            Slug: TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            OrderItemId: null,
            Description: TestConstants.Content.Editorial.Video.ValidDescription,
            ShootingScheduledAt: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateVideoCommand.OrderItemId)
                && e.ErrorMessage == _orderI18n.OrderItemIdRequired()
            );
    }

    [Fact]
    public async Task Validate_WithOrderItemIdButNoCustomerId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateVideoCommand(
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Content.Editorial.Video.ValidTitle,
            Slug: TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: Guid.NewGuid(),
            Description: TestConstants.Content.Editorial.Video.ValidDescription,
            ShootingScheduledAt: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateVideoCommand.CustomerId)
                && e.ErrorMessage == _customerI18n.CustomerIdRequired()
            );
    }

    #endregion

    #region Description Validation Tests

    [Fact]
    public async Task Validate_WithEmptyDescription_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateVideoCommand(
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Content.Editorial.Video.ValidTitle,
            Slug: TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null,
            Description: string.Empty,
            ShootingScheduledAt: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateVideoCommand.Description)
                && e.ErrorMessage == _videoI18n.DescriptionRequired()
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
        var videoI18n = LocalizerFactory.CreateMessage<VideoErrorMessage>(culture);
        var orderI18n = LocalizerFactory.CreateMessage<ContentOrderErrorMessage>(culture);
        var customerI18n = LocalizerFactory.CreateMessage<CustomerErrorMessage>(culture);
        var validator = new AdminCreateVideoValidator(articleI18n, videoI18n, orderI18n, customerI18n);
        var command = new AdminCreateVideoCommand(
            CategoryId: Guid.NewGuid(),
            Title: string.Empty,
            Slug: TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null,
            Description: TestConstants.Content.Editorial.Video.ValidDescription,
            ShootingScheduledAt: null
        );

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateVideoCommand.Title) && e.ErrorMessage == videoI18n.TitleRequired()
            );
    }

    #endregion
}
