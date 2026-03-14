using _116.Identity.Application.User.UseCases.Admin.Queries.GetUserRoles;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Admin.Queries.GetUserRoles;

/// <summary>
/// Unit tests for <see cref="AdminGetUserRolesValidator"/>.
/// </summary>
public class AdminGetUserRolesValidatorTests
{
    private readonly AdminGetUserRolesValidator _validator = new();

    #region Valid Query Tests

    [Fact]
    public async Task Validate_WithValidGuid_ShouldNotHaveErrors()
    {
        AdminGetUserRolesQuery query = new(UserId: Guid.NewGuid().ToString());
        ValidationResult result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithLowercaseGuid_ShouldNotHaveErrors()
    {
        AdminGetUserRolesQuery query = new(UserId: Guid.NewGuid().ToString().ToLowerInvariant());
        ValidationResult result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithUppercaseGuid_ShouldNotHaveErrors()
    {
        AdminGetUserRolesQuery query = new(UserId: Guid.NewGuid().ToString().ToUpperInvariant());
        ValidationResult result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyGuid_ShouldNotHaveErrors()
    {
        AdminGetUserRolesQuery query = new(UserId: Guid.Empty.ToString());
        ValidationResult result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Required Validation Tests

    [Fact]
    public async Task Validate_WithNullUserId_ShouldHaveError()
    {
        AdminGetUserRolesQuery query = new(UserId: null!);
        ValidationResult result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminGetUserRolesQuery.UserId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.UserIdRequired
            );
    }

    [Fact]
    public async Task Validate_WithEmptyUserId_ShouldHaveError()
    {
        AdminGetUserRolesQuery query = new(UserId: string.Empty);
        ValidationResult result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminGetUserRolesQuery.UserId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.UserIdRequired
            );
    }

    [Fact]
    public async Task Validate_WithWhitespaceUserId_ShouldHaveError()
    {
        AdminGetUserRolesQuery query = new(UserId: "   ");
        ValidationResult result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminGetUserRolesQuery.UserId)
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
        AdminGetUserRolesQuery query = new(UserId: invalidGuid);
        ValidationResult result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminGetUserRolesQuery.UserId)
                && e.ErrorMessage == TestConstants.ValidationMessages.Guid.UserIdInvalid
            );
    }

    #endregion
}
