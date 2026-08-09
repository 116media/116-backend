using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo;
using _116.Content.Application.Shared.Errors.Facade;
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
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminCreateVideoValidator _validator;

    public AdminCreateVideoValidatorTests()
    {
        _validator = new AdminCreateVideoValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreateVideoCommand(
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Video.ValidTitle,
            Slug: TestConstants.Video.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null,
            Description: TestConstants.Video.ValidDescription,
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
            Title: TestConstants.Video.ValidTitle,
            Slug: TestConstants.Video.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            OrderItemId: Guid.NewGuid(),
            Description: TestConstants.Video.ValidDescription,
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
            Title: TestConstants.Video.ValidTitle,
            Slug: TestConstants.Video.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null,
            Description: TestConstants.Video.ValidDescription,
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
                && e.ErrorMessage == _i18n.Article.Msg.CategoryIdRequired()
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
            Slug: TestConstants.Video.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null,
            Description: TestConstants.Video.ValidDescription,
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
                && e.ErrorMessage == _i18n.Video.Msg.TitleRequired()
            );
    }

    [Fact]
    public async Task Validate_WithTitleExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateVideoCommand(
            CategoryId: Guid.NewGuid(),
            Title: new string('a', TestConstants.Video.TitleMaxLength + 1),
            Slug: TestConstants.Video.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null,
            Description: TestConstants.Video.ValidDescription,
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
                && e.ErrorMessage == _i18n.Video.Msg.TitleTooLong(TestConstants.Video.TitleMaxLength)
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
            Title: TestConstants.Video.ValidTitle,
            Slug: string.Empty,
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null,
            Description: TestConstants.Video.ValidDescription,
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
                && e.ErrorMessage == _i18n.Video.Msg.SlugRequired()
            );
    }

    [Fact]
    public async Task Validate_WithUppercaseSlug_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateVideoCommand(
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Video.ValidTitle,
            Slug: "Invalid-Slug",
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: null,
            Description: TestConstants.Video.ValidDescription,
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
                && e.ErrorMessage == _i18n.Video.Msg.SlugInvalidFormat()
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
            Title: TestConstants.Video.ValidTitle,
            Slug: TestConstants.Video.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            OrderItemId: null,
            Description: TestConstants.Video.ValidDescription,
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
                && e.ErrorMessage == _i18n.ContentOrder.Msg.OrderItemIdRequired()
            );
    }

    [Fact]
    public async Task Validate_WithOrderItemIdButNoCustomerId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateVideoCommand(
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Video.ValidTitle,
            Slug: TestConstants.Video.ValidSlug,
            AuthorId: Guid.NewGuid(),
            CustomerId: null,
            OrderItemId: Guid.NewGuid(),
            Description: TestConstants.Video.ValidDescription,
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
                && e.ErrorMessage == _i18n.Customer.Msg.CustomerIdRequired()
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
            Title: TestConstants.Video.ValidTitle,
            Slug: TestConstants.Video.ValidSlug,
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
                && e.ErrorMessage == _i18n.Video.Msg.DescriptionRequired()
            );
    }

    #endregion
}
