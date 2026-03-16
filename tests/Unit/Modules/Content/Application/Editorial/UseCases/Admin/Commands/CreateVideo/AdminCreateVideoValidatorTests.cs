using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo;

/// <summary>
/// Unit tests for <see cref="AdminCreateVideoValidator"/>.
/// </summary>
public class AdminCreateVideoValidatorTests
{
    private readonly AdminCreateVideoValidator _validator = new();

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
            Description: null,
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
            Description: null,
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
            Description: null,
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
                && e.ErrorMessage == "Category ID is required."
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
            Description: null,
            ShootingScheduledAt: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateVideoCommand.Title) && e.ErrorMessage == "Video title is required."
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
            Description: null,
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
                && e.ErrorMessage == "Video title must not exceed 100 characters."
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
            Description: null,
            ShootingScheduledAt: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateVideoCommand.Slug) && e.ErrorMessage == "Video slug is required."
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
            Description: null,
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
                && e.ErrorMessage == "Video slug must be lowercase and contain only letters, numbers, and hyphens."
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
            Description: null,
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
                && e.ErrorMessage == "Order item ID is required when customer ID is provided."
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
            Description: null,
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
                && e.ErrorMessage == "Customer ID is required when order item ID is provided."
            );
    }

    #endregion
}
