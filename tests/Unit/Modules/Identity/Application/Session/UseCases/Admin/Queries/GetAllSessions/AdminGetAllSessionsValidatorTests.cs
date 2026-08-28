using _116.Identity.Application.Session.UseCases.Admin.Queries.GetAllSessions;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Admin.Queries.GetAllSessions;

/// <summary>
/// Unit tests for <see cref="AdminGetAllSessionsValidator"/>.
/// </summary>
public class AdminGetAllSessionsValidatorTests
{
    private readonly IdentityI18n _i18n = TestErrorsFactory.CreateIdentityI18n();
    private readonly AdminGetAllSessionsValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminGetAllSessionsValidatorTests"/>.
    /// </summary>
    public AdminGetAllSessionsValidatorTests()
    {
        _validator = new(_i18n);
    }

    #region Valid Query Tests

    [Fact]
    public async Task Validate_WithValidQuery_ShouldNotHaveErrors()
    {
        // Arrange
        AdminGetAllSessionsQuery query = new(PaginatedRequest: new PaginatedRequest(0, 10), UserId: null);

        // Act
        TestValidationResult<AdminGetAllSessionsQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithValidUserIdFilter_ShouldNotHaveErrors()
    {
        // Arrange
        AdminGetAllSessionsQuery query = new(
            PaginatedRequest: new PaginatedRequest(0, 10),
            UserId: Guid.NewGuid().ToString()
        );

        // Act
        TestValidationResult<AdminGetAllSessionsQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region UserId Validation Tests

    [Fact]
    public async Task Validate_WithInvalidUserIdFormat_ShouldHaveError()
    {
        // Arrange
        AdminGetAllSessionsQuery query = new(PaginatedRequest: new PaginatedRequest(0, 10), UserId: "not-a-guid");

        // Act
        TestValidationResult<AdminGetAllSessionsQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(_i18n.User.Validation.Localizer["UserIdInvalid"].Value);
    }

    [Fact]
    public async Task Validate_WithNullUserId_ShouldNotHaveError()
    {
        // Arrange
        AdminGetAllSessionsQuery query = new(PaginatedRequest: new PaginatedRequest(0, 10), UserId: null);

        // Act
        TestValidationResult<AdminGetAllSessionsQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithEmptyUserId_ShouldNotHaveError()
    {
        // Arrange
        AdminGetAllSessionsQuery query = new(PaginatedRequest: new PaginatedRequest(0, 10), UserId: string.Empty);

        // Act
        TestValidationResult<AdminGetAllSessionsQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion
}
