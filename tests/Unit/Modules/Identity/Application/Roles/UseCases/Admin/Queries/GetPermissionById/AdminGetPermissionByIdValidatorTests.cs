using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetPermissionById;
using _116.Unit.Tests.Common.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Queries.GetPermissionById;

/// <summary>
/// Unit tests for <see cref="AdminGetPermissionByIdValidator"/>.
/// </summary>
public class AdminGetPermissionByIdValidatorTests
{
    private readonly AdminGetPermissionByIdValidator _validator = new();

    #region Valid Query Tests

    [Fact]
    public async Task Validate_WithValidGuid_ShouldNotHaveErrors()
    {
        // Arrange
        AdminGetPermissionByIdQuery query = new(PermissionId: Guid.NewGuid().ToString());

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithLowercaseGuid_ShouldNotHaveErrors()
    {
        // Arrange
        AdminGetPermissionByIdQuery query = new(PermissionId: Guid.NewGuid().ToString().ToLowerInvariant());

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithUppercaseGuid_ShouldNotHaveErrors()
    {
        // Arrange
        AdminGetPermissionByIdQuery query = new(PermissionId: Guid.NewGuid().ToString().ToUpperInvariant());

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Required Validation Tests

    [Fact]
    public async Task Validate_WithNullPermissionId_ShouldHaveError()
    {
        // Arrange
        AdminGetPermissionByIdQuery query = new(PermissionId: null!);

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminGetPermissionByIdQuery.PermissionId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.PermissionIdRequired
            );
    }

    [Fact]
    public async Task Validate_WithEmptyPermissionId_ShouldHaveError()
    {
        // Arrange
        AdminGetPermissionByIdQuery query = new(PermissionId: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminGetPermissionByIdQuery.PermissionId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.PermissionIdRequired
            );
    }

    [Fact]
    public async Task Validate_WithWhitespacePermissionId_ShouldHaveError()
    {
        // Arrange
        AdminGetPermissionByIdQuery query = new(PermissionId: "   ");

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminGetPermissionByIdQuery.PermissionId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.PermissionIdRequired
            );
    }

    #endregion

    #region Format Validation Tests

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("12345")]
    [InlineData("abc-def-ghi")]
    [InlineData("00000000-0000-0000-0000-00000000000g")]
    public async Task Validate_WithInvalidGuidFormat_ShouldHaveError(string invalidGuid)
    {
        // Arrange
        AdminGetPermissionByIdQuery query = new(PermissionId: invalidGuid);

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminGetPermissionByIdQuery.PermissionId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.PermissionIdInvalid
            );
    }

    [Fact]
    public async Task Validate_WithEmptyGuid_ShouldNotHaveErrors()
    {
        // Arrange - Empty Guid is a valid Guid format
        AdminGetPermissionByIdQuery query = new(PermissionId: Guid.Empty.ToString());

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
}
