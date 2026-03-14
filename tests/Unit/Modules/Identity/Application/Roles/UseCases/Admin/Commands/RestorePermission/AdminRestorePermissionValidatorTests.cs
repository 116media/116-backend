using _116.Identity.Application.Roles.UseCases.Admin.Commands.RestorePermission;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.RestorePermission;

/// <summary>
/// Unit tests for <see cref="AdminRestorePermissionValidator"/>.
/// </summary>
public class AdminRestorePermissionValidatorTests
{
    private readonly AdminRestorePermissionValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidGuid_ShouldNotHaveErrors()
    {
        AdminRestorePermissionCommand command = new(PermissionId: Guid.NewGuid().ToString());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithLowercaseGuid_ShouldNotHaveErrors()
    {
        AdminRestorePermissionCommand command = new(PermissionId: Guid.NewGuid().ToString().ToLowerInvariant());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithUppercaseGuid_ShouldNotHaveErrors()
    {
        AdminRestorePermissionCommand command = new(PermissionId: Guid.NewGuid().ToString().ToUpperInvariant());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyGuid_ShouldNotHaveErrors()
    {
        AdminRestorePermissionCommand command = new(PermissionId: Guid.Empty.ToString());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Required Validation Tests

    [Fact]
    public async Task Validate_WithNullPermissionId_ShouldHaveError()
    {
        AdminRestorePermissionCommand command = new(PermissionId: null!);
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminRestorePermissionCommand.PermissionId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.PermissionIdRequired
            );
    }

    [Fact]
    public async Task Validate_WithEmptyPermissionId_ShouldHaveError()
    {
        AdminRestorePermissionCommand command = new(PermissionId: string.Empty);
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminRestorePermissionCommand.PermissionId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.PermissionIdRequired
            );
    }

    [Fact]
    public async Task Validate_WithWhitespacePermissionId_ShouldHaveError()
    {
        AdminRestorePermissionCommand command = new(PermissionId: "   ");
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminRestorePermissionCommand.PermissionId)
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
        AdminRestorePermissionCommand command = new(PermissionId: invalidGuid);
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminRestorePermissionCommand.PermissionId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.PermissionIdInvalid
            );
    }

    #endregion
}
