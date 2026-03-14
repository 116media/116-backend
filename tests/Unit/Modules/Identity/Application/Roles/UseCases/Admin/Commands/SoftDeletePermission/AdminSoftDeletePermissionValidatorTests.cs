using _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission;

/// <summary>
/// Unit tests for <see cref="AdminSoftDeletePermissionValidator"/>.
/// </summary>
public class AdminSoftDeletePermissionValidatorTests
{
    private readonly AdminSoftDeletePermissionValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidGuid_ShouldNotHaveErrors()
    {
        AdminSoftDeletePermissionCommand command = new(PermissionId: Guid.NewGuid().ToString());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithLowercaseGuid_ShouldNotHaveErrors()
    {
        AdminSoftDeletePermissionCommand command = new(PermissionId: Guid.NewGuid().ToString().ToLowerInvariant());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithUppercaseGuid_ShouldNotHaveErrors()
    {
        AdminSoftDeletePermissionCommand command = new(PermissionId: Guid.NewGuid().ToString().ToUpperInvariant());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyGuid_ShouldNotHaveErrors()
    {
        AdminSoftDeletePermissionCommand command = new(PermissionId: Guid.Empty.ToString());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Required Validation Tests

    [Fact]
    public async Task Validate_WithNullPermissionId_ShouldHaveError()
    {
        AdminSoftDeletePermissionCommand command = new(PermissionId: null!);
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminSoftDeletePermissionCommand.PermissionId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.PermissionIdRequired
            );
    }

    [Fact]
    public async Task Validate_WithEmptyPermissionId_ShouldHaveError()
    {
        AdminSoftDeletePermissionCommand command = new(PermissionId: string.Empty);
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminSoftDeletePermissionCommand.PermissionId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.PermissionIdRequired
            );
    }

    [Fact]
    public async Task Validate_WithWhitespacePermissionId_ShouldHaveError()
    {
        AdminSoftDeletePermissionCommand command = new(PermissionId: "   ");
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminSoftDeletePermissionCommand.PermissionId)
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
        AdminSoftDeletePermissionCommand command = new(PermissionId: invalidGuid);
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminSoftDeletePermissionCommand.PermissionId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.PermissionIdInvalid
            );
    }

    #endregion
}
