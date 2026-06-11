using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem;

/// <summary>
/// Unit tests for <see cref="AdminEditOrderItemValidator"/>.
/// </summary>
public class AdminEditOrderItemValidatorTests
{
    private readonly AdminEditOrderItemValidator _validator = new(
        LocalizerFactory.CreateMessage<_116.Content.Application.Shared.Errors.Messages.ContentOrderErrorMessage>("en")
    );

    #region Validate

    [Fact]
    public void Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminEditOrderItemCommand(
            OrderId: Guid.NewGuid().ToString(),
            ItemId: Guid.NewGuid().ToString(),
            ContentKind: EnumCoreContentType.Article,
            CategoryId: Guid.NewGuid().ToString(),
            PromotionLevelId: Guid.NewGuid(),
            SocialBoost: true,
            IsBonus: false
        );

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithInvalidGuidOrderId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminEditOrderItemCommand(
            OrderId: "not-a-guid",
            ItemId: Guid.NewGuid().ToString(),
            ContentKind: null,
            CategoryId: null,
            PromotionLevelId: null,
            SocialBoost: null,
            IsBonus: null
        );

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OrderId");
    }

    [Fact]
    public void Validate_WithInvalidGuidItemId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminEditOrderItemCommand(
            OrderId: Guid.NewGuid().ToString(),
            ItemId: "not-a-guid",
            ContentKind: null,
            CategoryId: null,
            PromotionLevelId: null,
            SocialBoost: null,
            IsBonus: null
        );

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ItemId");
    }

    [Fact]
    public void Validate_WithInvalidGuidCategoryId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminEditOrderItemCommand(
            OrderId: Guid.NewGuid().ToString(),
            ItemId: Guid.NewGuid().ToString(),
            ContentKind: null,
            CategoryId: "not-a-guid",
            PromotionLevelId: null,
            SocialBoost: null,
            IsBonus: null
        );

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CategoryId");
    }

    #endregion
}
