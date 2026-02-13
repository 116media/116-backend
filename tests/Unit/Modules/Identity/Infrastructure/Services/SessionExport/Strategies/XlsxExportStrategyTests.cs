using _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Services.SessionExport.Strategies;
using AwesomeAssertions;
using ClosedXML.Excel;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Services.SessionExport.Strategies;

/// <summary>
/// Unit tests for <see cref="XlsxExportStrategy"/>.
/// </summary>
public class XlsxExportStrategyTests
{
    private readonly XlsxExportStrategy _sut = new();

    [Fact]
    public void Export_WithNullColumns_ShouldReturnValidXlsxBytes()
    {
        // Arrange
        var sessions = new List<SessionExportDto> { CreateSessionDto() };

        // Act
        byte[] result = _sut.Export(sessions, null);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BePositive();

        // Verify it's a valid XLSX file
        using var memoryStream = new MemoryStream(result);
        Func<XLWorkbook> act = () => new XLWorkbook(memoryStream);
        act.Should().NotThrow();
    }

    [Fact]
    public void Export_WithEmptyColumns_ShouldReturnValidXlsxBytes()
    {
        // Arrange
        var sessions = new List<SessionExportDto> { CreateSessionDto() };

        // Act
        byte[] result = _sut.Export(sessions, new List<string>());

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BePositive();
    }

    [Fact]
    public void Export_WithSelectedColumns_ShouldReturnValidXlsxBytes()
    {
        // Arrange
        var sessions = new List<SessionExportDto> { CreateSessionDto() };

        var columns = new List<string> { "Id", "IpAddress", "IsActive" };

        // Act
        byte[] result = _sut.Export(sessions, columns);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BePositive();
    }

    [Fact]
    public void Export_WithMultipleSessions_ShouldReturnValidXlsxBytes()
    {
        // Arrange
        var sessions = new List<SessionExportDto> { CreateSessionDto("192.168.1.1"), CreateSessionDto("192.168.1.2") };

        // Act
        byte[] result = _sut.Export(sessions, null);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BePositive();
    }

    [Fact]
    public void Export_WithEmptySessions_ShouldReturnValidXlsxBytes()
    {
        // Arrange
        var sessions = new List<SessionExportDto>();
        var columns = new List<string> { "Id", "IpAddress" };

        // Act
        byte[] result = _sut.Export(sessions, columns);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BePositive();
    }

    [Fact]
    public void Export_ShouldCreateWorksheetNamedSessions()
    {
        // Arrange
        var sessions = new List<SessionExportDto> { CreateSessionDto() };

        // Act
        byte[] result = _sut.Export(sessions, null);

        // Assert
        using var memoryStream = new MemoryStream(result);
        using var workbook = new XLWorkbook(memoryStream);

        workbook.Worksheets.Should().ContainSingle();
        workbook.Worksheets.First().Name.Should().Be("Sessions");
    }

    [Fact]
    public void Export_ShouldStyleHeaderRow()
    {
        // Arrange
        var sessions = new List<SessionExportDto> { CreateSessionDto() };

        // Act
        byte[] result = _sut.Export(sessions, null);

        // Assert
        using var memoryStream = new MemoryStream(result);
        using var workbook = new XLWorkbook(memoryStream);
        IXLWorksheet worksheet = workbook.Worksheet("Sessions");

        IXLRow headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold.Should().BeTrue();
        headerRow.Style.Fill.BackgroundColor.Should().Be(XLColor.LightGray);
    }

    private static SessionExportDto CreateSessionDto(string ipAddress = "192.168.1.1")
    {
        return new SessionExportDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            ipAddress,
            "Mozilla/5.0",
            EnumBrowser.Chrome,
            EnumDevice.Desktop,
            EnumPlatform.Windows,
            EnumClient.WebApp,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1),
            true,
            null
        );
    }
}
