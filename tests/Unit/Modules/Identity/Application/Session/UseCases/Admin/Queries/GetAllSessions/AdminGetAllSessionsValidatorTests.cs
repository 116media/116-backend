using _116.Identity.Application.Session.UseCases.Admin.Queries.GetAllSessions;
using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Admin.Queries.GetAllSessions;

/// <summary>
/// Unit tests for <see cref="AdminGetAllSessionsValidator"/>.
/// </summary>
public class AdminGetAllSessionsValidatorTests
{
    private readonly AdminGetAllSessionsValidator _validator = new();

    #region Valid Query Tests

    [Fact]
    public async Task Validate_WithValidQuery_ShouldNotHaveErrors()
    {
        // Arrange
        AdminGetAllSessionsQuery query = new(
            PaginatedRequest: new PaginatedRequest { PageIndex = 0, PageSize = 10 },
            UserId: null
        );

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
            PaginatedRequest: new PaginatedRequest { PageIndex = 0, PageSize = 10 },
            UserId: Guid.NewGuid().ToString()
        );

        // Act
        TestValidationResult<AdminGetAllSessionsQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithMaxPageSize_ShouldNotHaveErrors()
    {
        // Arrange
        AdminGetAllSessionsQuery query = new(
            PaginatedRequest: new PaginatedRequest { PageIndex = 0, PageSize = 100 },
            UserId: null
        );

        // Act
        TestValidationResult<AdminGetAllSessionsQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region PageIndex Validation Tests

    [Fact]
    public async Task Validate_WithNegativePageIndex_ShouldHaveError()
    {
        // Arrange
        AdminGetAllSessionsQuery query = new(
            PaginatedRequest: new PaginatedRequest { PageIndex = -1, PageSize = 10 },
            UserId: null
        );

        // Act
        TestValidationResult<AdminGetAllSessionsQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.PaginatedRequest.PageIndex)
            .WithErrorMessage("Page index must be greater than or equal to 0.");
    }

    #endregion

    #region PageSize Validation Tests

    [Fact]
    public async Task Validate_WithZeroPageSize_ShouldHaveError()
    {
        // Arrange
        AdminGetAllSessionsQuery query = new(
            PaginatedRequest: new PaginatedRequest { PageIndex = 0, PageSize = 0 },
            UserId: null
        );

        // Act
        TestValidationResult<AdminGetAllSessionsQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.PaginatedRequest.PageSize)
            .WithErrorMessage("Page size must be greater than 0.");
    }

    [Fact]
    public async Task Validate_WithNegativePageSize_ShouldHaveError()
    {
        // Arrange
        AdminGetAllSessionsQuery query = new(
            PaginatedRequest: new PaginatedRequest { PageIndex = 0, PageSize = -1 },
            UserId: null
        );

        // Act
        TestValidationResult<AdminGetAllSessionsQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.PaginatedRequest.PageSize)
            .WithErrorMessage("Page size must be greater than 0.");
    }

    [Fact]
    public async Task Validate_WithPageSizeExceedingMaximum_ShouldHaveError()
    {
        // Arrange
        AdminGetAllSessionsQuery query = new(
            PaginatedRequest: new PaginatedRequest { PageIndex = 0, PageSize = 101 },
            UserId: null
        );

        // Act
        TestValidationResult<AdminGetAllSessionsQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.PaginatedRequest.PageSize)
            .WithErrorMessage("Page size must not exceed 100.");
    }

    #endregion

    #region UserId Validation Tests

    [Fact]
    public async Task Validate_WithInvalidUserIdFormat_ShouldHaveError()
    {
        // Arrange
        AdminGetAllSessionsQuery query = new(
            PaginatedRequest: new PaginatedRequest { PageIndex = 0, PageSize = 10 },
            UserId: "not-a-guid"
        );

        // Act
        TestValidationResult<AdminGetAllSessionsQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserId).WithErrorMessage("User ID is invalid.");
    }

    [Fact]
    public async Task Validate_WithNullUserId_ShouldNotHaveError()
    {
        // Arrange
        AdminGetAllSessionsQuery query = new(
            PaginatedRequest: new PaginatedRequest { PageIndex = 0, PageSize = 10 },
            UserId: null
        );

        // Act
        TestValidationResult<AdminGetAllSessionsQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithEmptyUserId_ShouldNotHaveError()
    {
        // Arrange - Empty UserId is optional, should not have error
        AdminGetAllSessionsQuery query = new(
            PaginatedRequest: new PaginatedRequest { PageIndex = 0, PageSize = 10 },
            UserId: string.Empty
        );

        // Act
        TestValidationResult<AdminGetAllSessionsQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithMultipleInvalidValues_ShouldHaveMultipleErrors()
    {
        // Arrange
        AdminGetAllSessionsQuery query = new(
            PaginatedRequest: new PaginatedRequest { PageIndex = -1, PageSize = 0 },
            UserId: "invalid-guid"
        );

        // Act
        TestValidationResult<AdminGetAllSessionsQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.ShouldHaveValidationErrorFor(x => x.PaginatedRequest.PageIndex);
        result.ShouldHaveValidationErrorFor(x => x.PaginatedRequest.PageSize);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    #endregion
}
