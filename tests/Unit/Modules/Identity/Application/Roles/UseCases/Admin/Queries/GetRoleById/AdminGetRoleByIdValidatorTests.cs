using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetRoleById;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Queries.GetRoleById;

/// <summary>
/// Unit tests for <see cref="AdminGetRoleByIdValidator"/>.
/// </summary>
public class AdminGetRoleByIdValidatorTests
{
    private readonly AdminGetRoleByIdValidator _validator = new();

    #region Valid Query Tests

    [Fact]
    public async Task Validate_WithValidGuid_ShouldNotHaveErrors()
    {
        // Arrange
        AdminGetRoleByIdQuery query = new(RoleId: Guid.NewGuid().ToString());

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
        AdminGetRoleByIdQuery query = new(RoleId: Guid.NewGuid().ToString().ToLowerInvariant());

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithUppercaseGuid_ShouldNotHaveErrors()
    {
        // Arrange
        AdminGetRoleByIdQuery query = new(RoleId: Guid.NewGuid().ToString().ToUpperInvariant());

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyGuid_ShouldNotHaveErrors()
    {
        // Arrange - Empty Guid is a valid Guid format
        AdminGetRoleByIdQuery query = new(RoleId: Guid.Empty.ToString());

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Required Validation Tests

    [Fact]
    public async Task Validate_WithNullRoleId_ShouldHaveError()
    {
        // Arrange
        AdminGetRoleByIdQuery query = new(RoleId: null!);

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminGetRoleByIdQuery.RoleId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.RoleIdRequired
            );
    }

    [Fact]
    public async Task Validate_WithEmptyRoleId_ShouldHaveError()
    {
        // Arrange
        AdminGetRoleByIdQuery query = new(RoleId: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminGetRoleByIdQuery.RoleId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.RoleIdRequired
            );
    }

    [Fact]
    public async Task Validate_WithWhitespaceRoleId_ShouldHaveError()
    {
        // Arrange
        AdminGetRoleByIdQuery query = new(RoleId: "   ");

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminGetRoleByIdQuery.RoleId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.RoleIdRequired
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
        AdminGetRoleByIdQuery query = new(RoleId: invalidGuid);

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminGetRoleByIdQuery.RoleId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.RoleIdInvalid
            );
    }

    #endregion
}
