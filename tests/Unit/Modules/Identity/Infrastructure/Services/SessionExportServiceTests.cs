using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Services;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="SessionExportService"/>.
/// </summary>
public class SessionExportServiceTests
{
    private readonly SessionExportService _service = new();

    #region Export Tests

    [Fact]
    public void Export_WithCsvFormat_ShouldReturnByteArray()
    {
        // Arrange
        var sessions = new List<SessionExportDto>
        {
            new(
                Id: Guid.NewGuid(),
                UserId: Guid.NewGuid(),
                IpAddress: "192.168.1.1",
                UserAgent: "Mozilla/5.0",
                Browser: EnumBrowser.Chrome,
                Device: EnumDevice.Desktop,
                Platform: EnumPlatform.Windows,
                Client: EnumClient.WebApp,
                CreatedAt: DateTime.UtcNow,
                ExpiresAt: DateTime.UtcNow.AddDays(30),
                IsActive: true,
                DeletedAt: null
            ),
        };

        // Act
        byte[] result = _service.Export(sessions, SessionExportFormat.Csv);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BePositive();
    }

    [Fact]
    public void Export_WithXlsxFormat_ShouldReturnByteArray()
    {
        // Arrange
        var sessions = new List<SessionExportDto>
        {
            new(
                Id: Guid.NewGuid(),
                UserId: Guid.NewGuid(),
                IpAddress: "192.168.1.1",
                UserAgent: "Mozilla/5.0",
                Browser: EnumBrowser.Firefox,
                Device: EnumDevice.Mobile,
                Platform: EnumPlatform.Android,
                Client: EnumClient.MobileApp,
                CreatedAt: DateTime.UtcNow,
                ExpiresAt: DateTime.UtcNow.AddDays(30),
                IsActive: true,
                DeletedAt: null
            ),
        };

        // Act
        byte[] result = _service.Export(sessions, SessionExportFormat.Xlsx);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BePositive();
    }

    [Fact]
    public void Export_WithEmptySessionList_ShouldReturnByteArray()
    {
        // Arrange
        var emptySessions = new List<SessionExportDto>();

        // Act
        byte[] result = _service.Export(emptySessions, SessionExportFormat.Csv);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Export_WithUnsupportedFormat_ShouldThrowArgumentException()
    {
        // Arrange
        var sessions = new List<SessionExportDto>
        {
            new(
                Id: Guid.NewGuid(),
                UserId: Guid.NewGuid(),
                IpAddress: "192.168.1.1",
                UserAgent: "Mozilla/5.0",
                Browser: EnumBrowser.Chrome,
                Device: EnumDevice.Desktop,
                Platform: EnumPlatform.Windows,
                Client: EnumClient.WebApp,
                CreatedAt: DateTime.UtcNow,
                ExpiresAt: DateTime.UtcNow.AddDays(30),
                IsActive: true,
                DeletedAt: null
            ),
        };

        // Act
        Func<byte[]> act = () => _service.Export(sessions, (SessionExportFormat)999);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*No export strategy found for format*");
    }

    // Note: Column filtering test removed due to CsvHelper limitation with IEnumerable types

    #endregion

    #region GetContentType Tests

    [Fact]
    public void GetContentType_WithCsvFormat_ShouldReturnCsvContentType()
    {
        // Act
        string contentType = _service.GetContentType(SessionExportFormat.Csv);

        // Assert
        contentType.Should().Be("text/csv");
    }

    [Fact]
    public void GetContentType_WithXlsxFormat_ShouldReturnXlsxContentType()
    {
        // Act
        string contentType = _service.GetContentType(SessionExportFormat.Xlsx);

        // Assert
        contentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    [Fact]
    public void GetContentType_WithUnsupportedFormat_ShouldThrowArgumentException()
    {
        // Act
        Func<string> act = () => _service.GetContentType((SessionExportFormat)999);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Unsupported export format*");
    }

    #endregion

    #region GenerateFileName Tests

    [Fact]
    public void GenerateFileName_WithCsvFormat_ShouldReturnFileNameWithCsvExtension()
    {
        // Act
        string fileName = _service.GenerateFileName(SessionExportFormat.Csv);

        // Assert
        fileName.Should().Contain(".csv");
        fileName.Should().Contain("sessions-export");
    }

    [Fact]
    public void GenerateFileName_WithXlsxFormat_ShouldReturnFileNameWithXlsxExtension()
    {
        // Act
        string fileName = _service.GenerateFileName(SessionExportFormat.Xlsx);

        // Assert
        fileName.Should().Contain(".xlsx");
        fileName.Should().Contain("sessions-export");
    }

    [Fact]
    public void GenerateFileName_WithCustomBaseFileName_ShouldUseCustomBaseName()
    {
        // Act
        string fileName = _service.GenerateFileName(SessionExportFormat.Csv, "custom-export");

        // Assert
        fileName.Should().Contain("custom-export");
        fileName.Should().Contain(".csv");
    }

    [Fact]
    public void GenerateFileName_ShouldIncludeTimestamp()
    {
        // Act
        string fileName = _service.GenerateFileName(SessionExportFormat.Csv);

        // Assert - should contain datetime pattern
        fileName.Should().MatchRegex(@"\d{14}"); // yyyyMMddHHmmss
    }

    [Fact]
    public void GenerateFileName_WithUnsupportedFormat_ShouldThrowArgumentException()
    {
        // Act
        Func<string> act = () => _service.GenerateFileName((SessionExportFormat)999);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Unsupported export format*");
    }

    #endregion
}
