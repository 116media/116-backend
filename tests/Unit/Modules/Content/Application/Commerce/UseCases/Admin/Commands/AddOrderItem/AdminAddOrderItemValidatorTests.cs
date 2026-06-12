using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem;

/// <summary>
/// Unit tests for <see cref="AdminAddOrderItemValidator"/>.
/// </summary>
public class AdminAddOrderItemValidatorTests
{
    private readonly ContentOrderErrorMessage _orderI18n = LocalizerFactory.CreateMessage<ContentOrderErrorMessage>();
    private readonly CategoryErrorMessage _categoryI18n = LocalizerFactory.CreateMessage<CategoryErrorMessage>();
    private readonly AdminAddOrderItemValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminAddOrderItemValidatorTests"/>.
    /// </summary>
    public AdminAddOrderItemValidatorTests()
    {
        _validator = new AdminAddOrderItemValidator(_orderI18n, _categoryI18n);
    }

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
                e.PropertyName == nameof(AdminAddOrderItemCommand.OrderId)
                && e.ErrorMessage == _orderI18n.Localizer["IdInvalid"].Value
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
                && e.ErrorMessage == _categoryI18n.Localizer["IdInvalid"].Value
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
                && e.ErrorMessage == _orderI18n.InvalidOrderItemContentKind()
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
        var orderI18n = LocalizerFactory.CreateMessage<ContentOrderErrorMessage>(culture);
        var categoryI18n = LocalizerFactory.CreateMessage<CategoryErrorMessage>(culture);
        var validator = new AdminAddOrderItemValidator(orderI18n, categoryI18n);
        var command = new AdminAddOrderItemCommand(
            OrderId: "not-a-guid",
            ContentKind: EnumCoreContentType.Article,
            CategoryId: Guid.NewGuid().ToString(),
            PromotionLevelId: null,
            SocialBoost: false,
            IsBonus: false
        );

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAddOrderItemCommand.OrderId)
                && e.ErrorMessage == orderI18n.Localizer["IdInvalid"].Value
            );
    }

    #endregion
}
