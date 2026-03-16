using _116.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole;

/// <summary>
/// Unit tests for <see cref="AdminAssignPermissionToRoleValidator"/>.
/// </summary>
public class AdminAssignPermissionToRoleValidatorTests
{
    private readonly AdminAssignPermissionToRoleValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidGuid_ShouldNotHaveErrors()
    {
        AdminAssignPermissionToRoleCommand command = new(
            RoleId: Guid.NewGuid().ToString(),
            PermissionId: Guid.NewGuid()
        );
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithLowercaseGuid_ShouldNotHaveErrors()
    {
        AdminAssignPermissionToRoleCommand command = new(
            RoleId: Guid.NewGuid().ToString().ToLowerInvariant(),
            PermissionId: Guid.NewGuid()
        );
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithUppercaseGuid_ShouldNotHaveErrors()
    {
        AdminAssignPermissionToRoleCommand command = new(
            RoleId: Guid.NewGuid().ToString().ToUpperInvariant(),
            PermissionId: Guid.NewGuid()
        );
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyGuid_ShouldNotHaveErrors()
    {
        AdminAssignPermissionToRoleCommand command = new(RoleId: Guid.Empty.ToString(), PermissionId: Guid.NewGuid());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Required Validation Tests

    [Fact]
    public async Task Validate_WithNullRoleId_ShouldHaveError()
    {
        AdminAssignPermissionToRoleCommand command = new(RoleId: null!, PermissionId: Guid.NewGuid());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminAssignPermissionToRoleCommand.RoleId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.RoleIdRequired
            );
    }

    [Fact]
    public async Task Validate_WithEmptyRoleId_ShouldHaveError()
    {
        AdminAssignPermissionToRoleCommand command = new(RoleId: string.Empty, PermissionId: Guid.NewGuid());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminAssignPermissionToRoleCommand.RoleId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.RoleIdRequired
            );
    }

    [Fact]
    public async Task Validate_WithWhitespaceRoleId_ShouldHaveError()
    {
        AdminAssignPermissionToRoleCommand command = new(RoleId: "   ", PermissionId: Guid.NewGuid());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminAssignPermissionToRoleCommand.RoleId)
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
        AdminAssignPermissionToRoleCommand command = new(RoleId: invalidGuid, PermissionId: Guid.NewGuid());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminAssignPermissionToRoleCommand.RoleId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.RoleIdInvalid
            );
    }

    #endregion
}
