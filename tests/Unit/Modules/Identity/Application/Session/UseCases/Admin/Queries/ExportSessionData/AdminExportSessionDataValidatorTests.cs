using _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;

/// <summary>
/// Unit tests for <see cref="AdminExportSessionDataValidator"/>.
/// </summary>
public class AdminExportSessionDataValidatorTests
{
    private static readonly string[] ValidColumns = typeof(SessionExportDto)
        .GetProperties()
        .Select(p => p.Name)
        .ToArray();

    private readonly IdentityI18n _i18n = TestErrorsFactory.CreateIdentityI18n();
    private readonly AdminExportSessionDataValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminExportSessionDataValidatorTests"/>.
    /// </summary>
    public AdminExportSessionDataValidatorTests()
    {
        _validator = new(_i18n);
    }

    #region Valid Query Tests

    [Fact]
    public async Task Validate_WithValidQuery_ShouldNotHaveErrors()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(
            Status: "active",
            FromDate: DateTime.UtcNow.AddDays(-7),
            ToDate: DateTime.UtcNow,
            Format: "Csv",
            Columns: "Id,UserId,IpAddress"
        );

        // Act
        TestValidationResult<AdminExportSessionDataQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithAllNullValues_ShouldNotHaveErrors()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(
            Status: null,
            FromDate: null,
            ToDate: null,
            Format: null,
            Columns: null
        );

        // Act
        TestValidationResult<AdminExportSessionDataQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Format Validation Tests

    [Theory]
    [InlineData("Csv")]
    [InlineData("csv")]
    [InlineData("Xlsx")]
    [InlineData("xlsx")]
    [InlineData("XLSX")]
    public async Task Validate_WithValidFormat_ShouldNotHaveError(string format)
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Format: format);

        // Act
        TestValidationResult<AdminExportSessionDataQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("json")]
    [InlineData("xml")]
    [InlineData("pdf")]
    public async Task Validate_WithInvalidFormat_ShouldHaveError(string format)
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Format: format);

        // Act
        TestValidationResult<AdminExportSessionDataQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.Format)
            .WithErrorMessage(_i18n.User.Validation.ExportFormatInvalid());
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("")]
    public async Task Validate_WithBlankFormat_ShouldNotHaveError(string format)
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Format: format);

        // Act
        TestValidationResult<AdminExportSessionDataQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Status Validation Tests

    [Theory]
    [InlineData("active")]
    [InlineData("Active")]
    [InlineData("ACTIVE")]
    [InlineData("expired")]
    [InlineData("Expired")]
    [InlineData("EXPIRED")]
    public async Task Validate_WithValidStatus_ShouldNotHaveError(string status)
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Status: status);

        // Act
        TestValidationResult<AdminExportSessionDataQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("pending")]
    [InlineData("inactive")]
    [InlineData("invalid")]
    public async Task Validate_WithInvalidStatus_ShouldHaveError(string status)
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Status: status);

        // Act
        TestValidationResult<AdminExportSessionDataQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage(_i18n.User.Validation.ExportStatusInvalid());
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("")]
    public async Task Validate_WithBlankStatus_ShouldNotHaveError(string status)
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Status: status);

        // Act
        TestValidationResult<AdminExportSessionDataQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Columns Validation Tests

    [Theory]
    [InlineData("Id")]
    [InlineData("UserId")]
    [InlineData("Id,UserId")]
    [InlineData("Id,UserId,IpAddress")]
    [InlineData("id,userid,ipaddress")]
    [InlineData("Id, UserId, IpAddress")]
    public async Task Validate_WithValidColumns_ShouldNotHaveError(string columns)
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Columns: columns);

        // Act
        TestValidationResult<AdminExportSessionDataQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("InvalidColumn")]
    [InlineData("Id,InvalidColumn")]
    [InlineData("InvalidColumn1,InvalidColumn2")]
    public async Task Validate_WithInvalidColumns_ShouldHaveError(string columns)
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Columns: columns);

        // Act
        TestValidationResult<AdminExportSessionDataQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.Columns)
            .WithErrorMessage(_i18n.User.Validation.ExportColumnsInvalid(string.Join(", ", ValidColumns)));
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("")]
    public async Task Validate_WithBlankColumns_ShouldNotHaveError(string columns)
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Columns: columns);

        // Act
        TestValidationResult<AdminExportSessionDataQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Date Range Validation Tests

    [Fact]
    public async Task Validate_WithValidDateRange_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(FromDate: DateTime.UtcNow.AddDays(-7), ToDate: DateTime.UtcNow);

        // Act
        TestValidationResult<AdminExportSessionDataQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEqualFromAndToDates_ShouldNotHaveError()
    {
        // Arrange
        DateTime sameDate = DateTime.UtcNow;
        AdminExportSessionDataQuery query = new(FromDate: sameDate, ToDate: sameDate);

        // Act
        TestValidationResult<AdminExportSessionDataQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithToDateBeforeFromDate_ShouldHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(FromDate: DateTime.UtcNow, ToDate: DateTime.UtcNow.AddDays(-7));

        // Act
        TestValidationResult<AdminExportSessionDataQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.ToDate)
            .WithErrorMessage(_i18n.User.Validation.ExportDateRangeInvalid());
    }

    [Fact]
    public async Task Validate_WithOnlyFromDate_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(FromDate: DateTime.UtcNow.AddDays(-7), ToDate: null);

        // Act
        TestValidationResult<AdminExportSessionDataQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithOnlyToDate_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(FromDate: null, ToDate: DateTime.UtcNow);

        // Act
        TestValidationResult<AdminExportSessionDataQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithAllInvalidValues_ShouldHaveMultipleErrors()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(
            Status: "invalid",
            FromDate: DateTime.UtcNow,
            ToDate: DateTime.UtcNow.AddDays(-7),
            Format: "json",
            Columns: "InvalidColumn1,InvalidColumn2"
        );

        // Act
        TestValidationResult<AdminExportSessionDataQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(4);
    }

    #endregion
}
