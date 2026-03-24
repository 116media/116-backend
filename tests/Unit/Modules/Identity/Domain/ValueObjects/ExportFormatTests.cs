using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using AwesomeAssertions;
using AwesomeAssertions.Specialized;
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
        var formatEnum = EnumSessionExportFormat.Csv;

        // Act
        ExportFormat format = new(formatEnum);

        // Assert
        format.Should().NotBeNull();
        format.Value.Should().Be(EnumSessionExportFormat.Csv);
    }

    [Theory]
    [InlineData(EnumSessionExportFormat.Csv)]
    [InlineData(EnumSessionExportFormat.Xlsx)]
    public void Constructor_WithAllValidEnumValues_ShouldNotThrow(EnumSessionExportFormat formatEnum)
    {
        // Act
        ExportFormat format = new(formatEnum);

        // Assert
        format.Should().NotBeNull();
        format.Value.Should().Be(formatEnum);
    }

    [Fact]
    public void Constructor_WithInvalidEnumValue_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidEnum = (EnumSessionExportFormat)999;

        // Act & Assert
        Action act = () => new ExportFormat(invalidEnum);
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid export format");
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
        format.Should().NotBeNull();
        format.Value.Should().Be(EnumSessionExportFormat.Csv);
    }

    [Theory]
    [InlineData("Csv", EnumSessionExportFormat.Csv)]
    [InlineData("Xlsx", EnumSessionExportFormat.Xlsx)]
    public void Constructor_WithValidStringValues_ShouldParseCorrectly(string input, EnumSessionExportFormat expected)
    {
        // Act
        ExportFormat format = new(input);

        // Assert
        format.Value.Should().Be(expected);
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
        format.Value.Should().Be(EnumSessionExportFormat.Csv);
    }

    [Fact]
    public void Constructor_WithInvalidStringValue_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new ExportFormat("InvalidFormat");
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid export format");
    }

    [Fact]
    public void Constructor_WithEmptyString_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new ExportFormat(string.Empty);
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid export format");
    }

    [Fact]
    public void Constructor_WithNullString_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new ExportFormat((string)null!);
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid export format");
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversionToEnum_ShouldReturnValue()
    {
        // Arrange
        ExportFormat format = new(EnumSessionExportFormat.Xlsx);

        // Act
        EnumSessionExportFormat result = format;

        // Assert
        result.Should().Be(EnumSessionExportFormat.Xlsx);
    }

    [Fact]
    public void ImplicitConversionToString_ShouldReturnEnumName()
    {
        // Arrange
        ExportFormat format = new(EnumSessionExportFormat.Xlsx);

        // Act
        string result = format;

        // Assert
        result.Should().Be("Xlsx");
    }

    [Fact]
    public void ImplicitConversionFromEnum_ShouldCreateInstance()
    {
        // Arrange
        var formatEnum = EnumSessionExportFormat.Csv;

        // Act
        ExportFormat format = formatEnum;

        // Assert
        format.Should().NotBeNull();
        format.Value.Should().Be(EnumSessionExportFormat.Csv);
    }

    [Fact]
    public void ImplicitConversionFromString_ShouldCreateInstance()
    {
        // Arrange
        string formatString = "Xlsx";

        // Act
        ExportFormat format = formatString;

        // Assert
        format.Should().NotBeNull();
        format.Value.Should().Be(EnumSessionExportFormat.Xlsx);
    }

    [Fact]
    public void ImplicitConversionFromString_WithInvalidValue_ShouldThrowArgumentException()
    {
        // Arrange
        string invalidFormat = "InvalidFormat";

        // Act & Assert
        Action act = () =>
        {
            ExportFormat format = invalidFormat;
        };
        act.Should().ThrowExactly<ArgumentException>();
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        ExportFormat format1 = new(EnumSessionExportFormat.Csv);
        ExportFormat format2 = new(EnumSessionExportFormat.Csv);

        // Act & Assert
        format1.Should().Be(format2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        ExportFormat format1 = new(EnumSessionExportFormat.Csv);
        ExportFormat format2 = new(EnumSessionExportFormat.Xlsx);

        // Act & Assert
        format1.Should().NotBe(format2);
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        ExportFormat format1 = new(EnumSessionExportFormat.Csv);
        ExportFormat format2 = new(EnumSessionExportFormat.Csv);

        // Act
        int hash1 = format1.GetHashCode();
        int hash2 = format2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    #endregion
}
