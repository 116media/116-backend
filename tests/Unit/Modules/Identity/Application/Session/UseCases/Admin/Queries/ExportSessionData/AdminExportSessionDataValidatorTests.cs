using _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;

/// <summary>
/// Unit tests for <see cref="AdminExportSessionDataValidator"/>.
/// </summary>
public class AdminExportSessionDataValidatorTests
{
    private readonly AdminExportSessionDataValidator _validator = new();

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
        var result = await _validator.TestValidateAsync(query);

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
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Format Validation Tests

    [Fact]
    public async Task Validate_WithValidCsvFormat_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Format: "Csv");

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithValidXlsxFormat_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Format: "Xlsx");

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithInvalidFormat_ShouldHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Format: "json");

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Format).WithErrorMessage("Format must be one of: Csv, Xlsx.");
    }

    [Fact]
    public async Task Validate_WithFormatCaseInsensitive_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Format: "csv");

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Status Validation Tests

    [Fact]
    public async Task Validate_WithValidActiveStatus_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Status: "active");

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithValidExpiredStatus_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Status: "expired");

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithInvalidStatus_ShouldHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Status: "pending");

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Status must be either 'active' or 'expired'.");
    }

    [Fact]
    public async Task Validate_WithStatusCaseInsensitive_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Status: "ACTIVE");

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Columns Validation Tests

    [Fact]
    public async Task Validate_WithValidSingleColumn_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Columns: "Id");

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithValidMultipleColumns_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Columns: "Id,UserId,IpAddress,UserAgent,Browser");

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithInvalidColumnName_ShouldHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Columns: "Id,InvalidColumn");

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Columns);
    }

    [Fact]
    public async Task Validate_WithColumnsContainingSpaces_ShouldNotHaveError()
    {
        // Arrange - spaces should be trimmed
        AdminExportSessionDataQuery query = new(Columns: "Id, UserId, IpAddress");

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithAllValidColumns_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(
            Columns: "Id,UserId,IpAddress,UserAgent,Browser,Device,Platform,Client,CreatedAt,ExpiresAt,IsActive,DeletedAt"
        );

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Date Validation Tests

    [Fact]
    public async Task Validate_WithValidDateRange_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(FromDate: DateTime.UtcNow.AddDays(-7), ToDate: DateTime.UtcNow);

        // Act
        var result = await _validator.TestValidateAsync(query);

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
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithToDateBeforeFromDate_ShouldHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(FromDate: DateTime.UtcNow, ToDate: DateTime.UtcNow.AddDays(-7));

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.ToDate)
            .WithErrorMessage("ToDate must be greater than or equal to FromDate.");
    }

    [Fact]
    public async Task Validate_WithOnlyFromDate_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(FromDate: DateTime.UtcNow.AddDays(-7), ToDate: null);

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithOnlyToDate_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(FromDate: null, ToDate: DateTime.UtcNow);

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task Validate_WithWhitespaceFormat_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Format: "   ");

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyStringFormat_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Format: "");

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithWhitespaceStatus_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Status: "   ");

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyStringStatus_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Status: "");

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithWhitespaceColumns_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Columns: "   ");

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyStringColumns_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Columns: "");

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithColumnsCaseInsensitive_ShouldNotHaveError()
    {
        // Arrange
        AdminExportSessionDataQuery query = new(Columns: "id,userid,ipaddress");

        // Act
        var result = await _validator.TestValidateAsync(query);

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
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterThanOrEqualTo(4);
    }

    #endregion

    #region Private Method Direct Tests (via Reflection)

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BeValidExportFormat_WithNullOrWhitespace_ShouldReturnFalse(string? format)
    {
        // Arrange
        var method = typeof(AdminExportSessionDataValidator).GetMethod(
            "BeValidExportFormat",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        // Act
        bool result = (bool)method!.Invoke(null, new object?[] { format })!;

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("Csv")]
    [InlineData("csv")]
    [InlineData("Xlsx")]
    [InlineData("xlsx")]
    [InlineData("XLSX")]
    public void BeValidExportFormat_WithValidFormat_ShouldReturnTrue(string format)
    {
        // Arrange
        var method = typeof(AdminExportSessionDataValidator).GetMethod(
            "BeValidExportFormat",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        // Act
        bool result = (bool)method!.Invoke(null, new object[] { format })!;

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("json")]
    [InlineData("xml")]
    [InlineData("pdf")]
    public void BeValidExportFormat_WithInvalidFormat_ShouldReturnFalse(string format)
    {
        // Arrange
        var method = typeof(AdminExportSessionDataValidator).GetMethod(
            "BeValidExportFormat",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        // Act
        bool result = (bool)method!.Invoke(null, new object[] { format })!;

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BeValidSessionStatus_WithNullOrWhitespace_ShouldReturnFalse(string? status)
    {
        // Arrange
        var method = typeof(AdminExportSessionDataValidator).GetMethod(
            "BeValidSessionStatus",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        // Act
        bool result = (bool)method!.Invoke(null, new object?[] { status })!;

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("Active")]
    [InlineData("active")]
    [InlineData("ACTIVE")]
    [InlineData("Expired")]
    [InlineData("expired")]
    [InlineData("EXPIRED")]
    public void BeValidSessionStatus_WithValidStatus_ShouldReturnTrue(string status)
    {
        // Arrange
        var method = typeof(AdminExportSessionDataValidator).GetMethod(
            "BeValidSessionStatus",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        // Act
        bool result = (bool)method!.Invoke(null, new object[] { status })!;

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("pending")]
    [InlineData("inactive")]
    [InlineData("invalid")]
    public void BeValidSessionStatus_WithInvalidStatus_ShouldReturnFalse(string status)
    {
        // Arrange
        var method = typeof(AdminExportSessionDataValidator).GetMethod(
            "BeValidSessionStatus",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        // Act
        bool result = (bool)method!.Invoke(null, new object[] { status })!;

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BeValidColumns_WithNullOrWhitespace_ShouldReturnTrue(string? columns)
    {
        // Arrange
        var method = typeof(AdminExportSessionDataValidator).GetMethod(
            "BeValidColumns",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        // Act
        bool result = (bool)method!.Invoke(null, new object?[] { columns })!;

        // Assert
        result.Should().BeTrue("null or whitespace columns should be considered valid");
    }

    [Theory]
    [InlineData("Id")]
    [InlineData("UserId")]
    [InlineData("Id,UserId")]
    [InlineData("Id,UserId,IpAddress")]
    [InlineData("id,userid,ipaddress")] // Case insensitive
    public void BeValidColumns_WithValidColumns_ShouldReturnTrue(string columns)
    {
        // Arrange
        var method = typeof(AdminExportSessionDataValidator).GetMethod(
            "BeValidColumns",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        // Act
        bool result = (bool)method!.Invoke(null, new object[] { columns })!;

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("InvalidColumn")]
    [InlineData("Id,InvalidColumn")]
    [InlineData("InvalidColumn1,InvalidColumn2")]
    public void BeValidColumns_WithInvalidColumns_ShouldReturnFalse(string columns)
    {
        // Arrange
        var method = typeof(AdminExportSessionDataValidator).GetMethod(
            "BeValidColumns",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        // Act
        bool result = (bool)method!.Invoke(null, new object[] { columns })!;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void BeValidColumns_WithColumnsContainingSpaces_ShouldTrimAndValidate()
    {
        // Arrange
        var method = typeof(AdminExportSessionDataValidator).GetMethod(
            "BeValidColumns",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        // Act
        bool result = (bool)method!.Invoke(null, new object[] { "Id, UserId, IpAddress" })!;

        // Assert
        result.Should().BeTrue("spaces should be trimmed from column names");
    }

    #endregion
}
