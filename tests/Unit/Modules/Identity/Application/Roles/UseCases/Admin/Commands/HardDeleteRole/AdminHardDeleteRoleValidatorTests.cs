using _116.Identity.Application.Roles.UseCases.Admin.Commands.HardDeleteRole;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.HardDeleteRole;

/// <summary>
/// Unit tests for <see cref="AdminHardDeleteRoleValidator"/>.
/// </summary>
public class AdminHardDeleteRoleValidatorTests
{
    private readonly AdminHardDeleteRoleValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidGuid_ShouldNotHaveErrors()
    {
        AdminHardDeleteRoleCommand command = new(RoleId: Guid.NewGuid().ToString());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithLowercaseGuid_ShouldNotHaveErrors()
    {
        AdminHardDeleteRoleCommand command = new(RoleId: Guid.NewGuid().ToString().ToLowerInvariant());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithUppercaseGuid_ShouldNotHaveErrors()
    {
        AdminHardDeleteRoleCommand command = new(RoleId: Guid.NewGuid().ToString().ToUpperInvariant());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyGuid_ShouldNotHaveErrors()
    {
        AdminHardDeleteRoleCommand command = new(RoleId: Guid.Empty.ToString());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Required Validation Tests

    [Fact]
    public async Task Validate_WithNullRoleId_ShouldHaveError()
    {
        AdminHardDeleteRoleCommand command = new(RoleId: null!);
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminHardDeleteRoleCommand.RoleId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.RoleIdRequired
            );
    }

    [Fact]
    public async Task Validate_WithEmptyRoleId_ShouldHaveError()
    {
        AdminHardDeleteRoleCommand command = new(RoleId: string.Empty);
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminHardDeleteRoleCommand.RoleId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.RoleIdRequired
            );
    }

    [Fact]
    public async Task Validate_WithWhitespaceRoleId_ShouldHaveError()
    {
        AdminHardDeleteRoleCommand command = new(RoleId: "   ");
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminHardDeleteRoleCommand.RoleId)
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
        AdminHardDeleteRoleCommand command = new(RoleId: invalidGuid);
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminHardDeleteRoleCommand.RoleId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.RoleIdInvalid
            );
    }

    #endregion
}
