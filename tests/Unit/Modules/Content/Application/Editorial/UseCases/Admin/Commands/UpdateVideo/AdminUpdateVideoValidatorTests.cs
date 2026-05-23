using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo;

/// <summary>
/// Unit tests for <see cref="AdminUpdateVideoValidator"/>.
/// </summary>
public class AdminUpdateVideoValidatorTests
{
    private readonly AdminUpdateVideoValidator _validator = new(
        LocalizerFactory.CreateMessage<ArticleErrorMessage>(),
        LocalizerFactory.CreateMessage<VideoErrorMessage>(),
        LocalizerFactory.CreateMessage<ContentOrderErrorMessage>(),
        LocalizerFactory.CreateMessage<CustomerErrorMessage>()
    );

    private static AdminUpdateVideoCommand ValidCommand() =>
        new(
            Id: Guid.NewGuid().ToString(),
            CategoryId: Guid.NewGuid(),
            Title: TestConstants.Content.Editorial.Video.ValidTitle,
            Slug: TestConstants.Content.Editorial.Video.ValidSlug,
            Description: TestConstants.Content.Editorial.Video.ValidDescription,
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
                e.PropertyName == nameof(AdminUpdateVideoCommand.Id) && e.ErrorMessage == "Video ID is required."
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
                e.PropertyName == nameof(AdminUpdateVideoCommand.Id) && e.ErrorMessage == "Video ID is invalid."
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
                e.PropertyName == nameof(AdminUpdateVideoCommand.CategoryId)
                && e.ErrorMessage == "Article category ID is required."
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
                e.PropertyName == nameof(AdminUpdateVideoCommand.Title) && e.ErrorMessage == "Video title is required."
            );
    }

    [Fact]
    public async Task Validate_WithTitleExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            Title = new string('a', TestConstants.Content.Editorial.Video.TitleMaxLength + 1),
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateVideoCommand.Title)
                && e.ErrorMessage == "Video title must not exceed 100 characters."
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
                e.PropertyName == nameof(AdminUpdateVideoCommand.Slug) && e.ErrorMessage == "Video slug is required."
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
                e.PropertyName == nameof(AdminUpdateVideoCommand.Slug)
                && e.ErrorMessage == "Video slug must be lowercase and contain only letters, numbers, and hyphens."
            );
    }

    #endregion

    #region Description Validation Tests

    [Fact]
    public async Task Validate_WithEmptyDescription_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            Description = string.Empty,
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateVideoCommand.Description)
                && e.ErrorMessage == "Video description is required."
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
                e.PropertyName == nameof(AdminUpdateVideoCommand.OrderItemId)
                && e.ErrorMessage == "Order item ID is required."
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
                e.PropertyName == nameof(AdminUpdateVideoCommand.CustomerId)
                && e.ErrorMessage == "Customer ID is required."
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
                e.PropertyName == nameof(AdminUpdateVideoCommand.MetaTitle)
                && e.ErrorMessage == "Meta title must be at least 10 characters."
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
                e.PropertyName == nameof(AdminUpdateVideoCommand.MetaTitle)
                && e.ErrorMessage == "Meta title must not exceed 70 characters."
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
                e.PropertyName == nameof(AdminUpdateVideoCommand.MetaDescription)
                && e.ErrorMessage == "Meta description must be at least 50 characters."
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
                e.PropertyName == nameof(AdminUpdateVideoCommand.MetaDescription)
                && e.ErrorMessage == "Meta description must not exceed 160 characters."
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
}
