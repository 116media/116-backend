using _116.Identity.Application.Roles.UseCases.Admin.Commands.ActivateRole;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.ActivateRole;

/// <summary>
/// Unit tests for <see cref="AdminActivateRoleValidator"/>.
/// </summary>
public class AdminActivateRoleValidatorTests
{
    private readonly AdminActivateRoleValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidGuid_ShouldNotHaveErrors()
    {
        AdminActivateRoleCommand command = new(RoleId: Guid.NewGuid().ToString());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithLowercaseGuid_ShouldNotHaveErrors()
    {
        AdminActivateRoleCommand command = new(RoleId: Guid.NewGuid().ToString().ToLowerInvariant());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithUppercaseGuid_ShouldNotHaveErrors()
    {
        AdminActivateRoleCommand command = new(RoleId: Guid.NewGuid().ToString().ToUpperInvariant());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyGuid_ShouldNotHaveErrors()
    {
        AdminActivateRoleCommand command = new(RoleId: Guid.Empty.ToString());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Required Validation Tests

    [Fact]
    public async Task Validate_WithNullRoleId_ShouldHaveError()
    {
        AdminActivateRoleCommand command = new(RoleId: null!);
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminActivateRoleCommand.RoleId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.RoleIdRequired
            );
    }

    [Fact]
    public async Task Validate_WithEmptyRoleId_ShouldHaveError()
    {
        AdminActivateRoleCommand command = new(RoleId: string.Empty);
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminActivateRoleCommand.RoleId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.RoleIdRequired
            );
    }

    [Fact]
    public async Task Validate_WithWhitespaceRoleId_ShouldHaveError()
    {
        AdminActivateRoleCommand command = new(RoleId: "   ");
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminActivateRoleCommand.RoleId)
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
        AdminActivateRoleCommand command = new(RoleId: invalidGuid);
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminActivateRoleCommand.RoleId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.RoleIdInvalid
            );
    }

    #endregion
}
