using _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser;

/// <summary>
/// Unit tests for <see cref="AdminRemoveRoleFromUserValidator"/>.
/// </summary>
public class AdminRemoveRoleFromUserValidatorTests
{
    private readonly AdminRemoveRoleFromUserValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidGuid_ShouldNotHaveErrors()
    {
        AdminRemoveRoleFromUserCommand command = new(UserId: Guid.NewGuid().ToString(), RoleId: Guid.NewGuid());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithLowercaseGuid_ShouldNotHaveErrors()
    {
        AdminRemoveRoleFromUserCommand command = new(
            UserId: Guid.NewGuid().ToString().ToLowerInvariant(),
            RoleId: Guid.NewGuid()
        );
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithUppercaseGuid_ShouldNotHaveErrors()
    {
        AdminRemoveRoleFromUserCommand command = new(
            UserId: Guid.NewGuid().ToString().ToUpperInvariant(),
            RoleId: Guid.NewGuid()
        );
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyGuid_ShouldNotHaveErrors()
    {
        AdminRemoveRoleFromUserCommand command = new(UserId: Guid.Empty.ToString(), RoleId: Guid.NewGuid());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Required Validation Tests

    [Fact]
    public async Task Validate_WithNullUserId_ShouldHaveError()
    {
        AdminRemoveRoleFromUserCommand command = new(UserId: null!, RoleId: Guid.NewGuid());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminRemoveRoleFromUserCommand.UserId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.UserIdRequired
            );
    }

    [Fact]
    public async Task Validate_WithEmptyUserId_ShouldHaveError()
    {
        AdminRemoveRoleFromUserCommand command = new(UserId: string.Empty, RoleId: Guid.NewGuid());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminRemoveRoleFromUserCommand.UserId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.UserIdRequired
            );
    }

    [Fact]
    public async Task Validate_WithWhitespaceUserId_ShouldHaveError()
    {
        AdminRemoveRoleFromUserCommand command = new(UserId: "   ", RoleId: Guid.NewGuid());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminRemoveRoleFromUserCommand.UserId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.UserIdRequired
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
        AdminRemoveRoleFromUserCommand command = new(UserId: invalidGuid, RoleId: Guid.NewGuid());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminRemoveRoleFromUserCommand.UserId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.UserIdInvalid
            );
    }

    #endregion
}
