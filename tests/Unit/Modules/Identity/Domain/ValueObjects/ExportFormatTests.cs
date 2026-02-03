using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="ExportFormat"/> value object.
/// </summary>
public class ExportFormatTests
{
    #region Constructor Tests (Enum)

    [Fact]
    public void Constructor_WithValidEnumValue_ShouldCreateInstance()
    {
        // Arrange
        SessionExportFormat formatEnum = SessionExportFormat.Csv;

        // Act
        ExportFormat format = new(formatEnum);

        // Assert
        Assert.NotNull(format);
        Assert.Equal(SessionExportFormat.Csv, format.Value);
    }

    [Theory]
    [InlineData(SessionExportFormat.Csv)]
    [InlineData(SessionExportFormat.Xlsx)]
    public void Constructor_WithAllValidEnumValues_ShouldNotThrow(SessionExportFormat formatEnum)
    {
        // Act
        ExportFormat format = new(formatEnum);

        // Assert
        Assert.NotNull(format);
        Assert.Equal(formatEnum, format.Value);
    }

    [Fact]
    public void Constructor_WithInvalidEnumValue_ShouldThrowArgumentException()
    {
        // Arrange
        SessionExportFormat invalidEnum = (SessionExportFormat)999;

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new ExportFormat(invalidEnum));
        Assert.Contains("Invalid export format", exception.Message);
    }

    #endregion

    #region Constructor Tests (String)

    [Fact]
    public void Constructor_WithValidStringValue_ShouldCreateInstance()
    {
        // Arrange
        string formatString = "Csv";

        // Act
        ExportFormat format = new(formatString);

        // Assert
        Assert.NotNull(format);
        Assert.Equal(SessionExportFormat.Csv, format.Value);
    }

    [Theory]
    [InlineData("Csv", SessionExportFormat.Csv)]
    [InlineData("Xlsx", SessionExportFormat.Xlsx)]
    public void Constructor_WithValidStringValues_ShouldParseCorrectly(string input, SessionExportFormat expected)
    {
        // Act
        ExportFormat format = new(input);

        // Assert
        Assert.Equal(expected, format.Value);
    }

    [Theory]
    [InlineData("csv")]
    [InlineData("CSV")]
    [InlineData("CsV")]
    public void Constructor_WithCaseInsensitiveString_ShouldParseCorrectly(string input)
    {
        // Act
        ExportFormat format = new(input);

        // Assert
        Assert.Equal(SessionExportFormat.Csv, format.Value);
    }

    [Fact]
    public void Constructor_WithInvalidStringValue_ShouldThrowArgumentException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new ExportFormat("InvalidFormat"));
        Assert.Contains("Invalid export format", exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyString_ShouldThrowArgumentException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new ExportFormat(string.Empty));
        Assert.Contains("Invalid export format", exception.Message);
    }

    [Fact]
    public void Constructor_WithNullString_ShouldThrowArgumentException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new ExportFormat((string)null!));
        Assert.Contains("Invalid export format", exception.Message);
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversionToEnum_ShouldReturnValue()
    {
        // Arrange
        ExportFormat format = new(SessionExportFormat.Xlsx);

        // Act
        SessionExportFormat result = format;

        // Assert
        Assert.Equal(SessionExportFormat.Xlsx, result);
    }

    [Fact]
    public void ImplicitConversionToString_ShouldReturnEnumName()
    {
        // Arrange
        ExportFormat format = new(SessionExportFormat.Xlsx);

        // Act
        string result = format;

        // Assert
        Assert.Equal("Xlsx", result);
    }

    [Fact]
    public void ImplicitConversionFromEnum_ShouldCreateInstance()
    {
        // Arrange
        SessionExportFormat formatEnum = SessionExportFormat.Csv;

        // Act
        ExportFormat format = formatEnum;

        // Assert
        Assert.NotNull(format);
        Assert.Equal(SessionExportFormat.Csv, format.Value);
    }

    [Fact]
    public void ImplicitConversionFromString_ShouldCreateInstance()
    {
        // Arrange
        string formatString = "Xlsx";

        // Act
        ExportFormat format = formatString;

        // Assert
        Assert.NotNull(format);
        Assert.Equal(SessionExportFormat.Xlsx, format.Value);
    }

    [Fact]
    public void ImplicitConversionFromString_WithInvalidValue_ShouldThrowArgumentException()
    {
        // Arrange
        string invalidFormat = "InvalidFormat";

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            ExportFormat format = invalidFormat;
        });
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        ExportFormat format1 = new(SessionExportFormat.Csv);
        ExportFormat format2 = new(SessionExportFormat.Csv);

        // Act & Assert
        Assert.Equal(format1, format2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        ExportFormat format1 = new(SessionExportFormat.Csv);
        ExportFormat format2 = new(SessionExportFormat.Xlsx);

        // Act & Assert
        Assert.NotEqual(format1, format2);
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        ExportFormat format1 = new(SessionExportFormat.Csv);
        ExportFormat format2 = new(SessionExportFormat.Csv);

        // Act
        int hash1 = format1.GetHashCode();
        int hash2 = format2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    #endregion
}
