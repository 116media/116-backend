using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem;

/// <summary>
/// Unit tests for <see cref="AdminAddOrderItemValidator"/>.
/// </summary>
public class AdminAddOrderItemValidatorTests
{
    private readonly AdminAddOrderItemValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminAddOrderItemCommand(
            OrderId: Guid.NewGuid().ToString(),
            ContentKind: EnumCoreContentType.Article,
            CategoryId: Guid.NewGuid().ToString(),
            PromotionLevelId: null,
            SocialBoost: false,
            IsBonus: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region OrderId Validation Tests

    [Fact]
    public async Task Validate_WithInvalidGuidOrderId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminAddOrderItemCommand(
            OrderId: "not-a-guid",
            ContentKind: EnumCoreContentType.Article,
            CategoryId: Guid.NewGuid().ToString(),
            PromotionLevelId: null,
            SocialBoost: false,
            IsBonus: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAddOrderItemCommand.OrderId) && e.ErrorMessage == "Order ID is invalid."
            );
    }

    #endregion

    #region CategoryId Validation Tests

    [Fact]
    public async Task Validate_WithInvalidGuidCategoryId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminAddOrderItemCommand(
            OrderId: Guid.NewGuid().ToString(),
            ContentKind: EnumCoreContentType.Article,
            CategoryId: "not-a-guid",
            PromotionLevelId: null,
            SocialBoost: false,
            IsBonus: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAddOrderItemCommand.CategoryId)
                && e.ErrorMessage == "Category ID is invalid."
            );
    }

    #endregion

    #region ContentKind Validation Tests

    [Fact]
    public async Task Validate_WithInvalidContentKind_ShouldHaveError()
    {
        // Arrange
        var command = new AdminAddOrderItemCommand(
            OrderId: Guid.NewGuid().ToString(),
            ContentKind: (EnumCoreContentType)999,
            CategoryId: Guid.NewGuid().ToString(),
            PromotionLevelId: null,
            SocialBoost: false,
            IsBonus: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAddOrderItemCommand.ContentKind)
                && e.ErrorMessage == "Content kind must be Article or Video."
            );
    }

    #endregion
}
