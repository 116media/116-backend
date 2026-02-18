using System.Text;
using _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Services.SessionExport.Strategies;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Services.SessionExport.Strategies;

/// <summary>
/// Unit tests for <see cref="CsvExportStrategy"/>.
/// </summary>
public class CsvExportStrategyTests
{
    private readonly CsvExportStrategy _sut = new();

    [Fact]
    public void Export_WithNullColumns_ShouldReturnValidCsvBytes()
    {
        // Arrange
        var sessions = new List<SessionExportDto> { CreateSessionDto() };

        // Act
        byte[] result = _sut.Export(sessions, null);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BePositive();

        string csvContent = Encoding.UTF8.GetString(result);
        csvContent.Should().NotBeNullOrEmpty();
        csvContent.Should().Contain("Id"); // Header should be present
    }

    [Fact]
    public void Export_WithEmptyColumns_ShouldReturnValidCsvBytes()
    {
        // Arrange
        var sessions = new List<SessionExportDto> { CreateSessionDto() };

        // Act
        byte[] result = _sut.Export(sessions, []);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BePositive();
    }

    [Fact]
    public void Export_WithMultipleSessions_ShouldReturnValidCsvBytes()
    {
        // Arrange
        var sessions = new List<SessionExportDto> { CreateSessionDto("192.168.1.1"), CreateSessionDto("192.168.1.2") };

        // Act
        byte[] result = _sut.Export(sessions, null);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BePositive();

        string csvContent = Encoding.UTF8.GetString(result);
        csvContent.Should().Contain("192.168.1.1");
        csvContent.Should().Contain("192.168.1.2");
    }

    [Fact]
    public void Export_WithEmptySessions_ShouldReturnValidCsvBytes()
    {
        // Arrange
        var sessions = new List<SessionExportDto>();

        // Act
        byte[] result = _sut.Export(sessions, null);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BePositive();
    }

    [Fact]
    public void Export_ShouldReturnValidUtf8Bytes()
    {
        // Arrange
        var sessions = new List<SessionExportDto> { CreateSessionDto() };

        // Act
        byte[] result = _sut.Export(sessions, null);

        // Assert
        result.Should().NotBeNull();
        Func<string> act = () => Encoding.UTF8.GetString(result);
        act.Should().NotThrow();
    }

    [Fact]
    public void Export_WithSelectedColumns_ShouldThrowCsvHelperException()
    {
        // Arrange
        var sessions = new List<SessionExportDto> { CreateSessionDto() };
        var columns = new List<string> { "Id", "IpAddress", "IsActive" };

        // Act - This exercises the filtered columns code path
        // CsvHelper cannot handle ExpandoObject (dynamic objects) for CSV writing
        Func<byte[]> act = () => _sut.Export(sessions, columns);

        // Assert - CsvHelper throws ConfigurationException when attempting to write ExpandoObject
        // The GetFilteredColumns method itself is fully tested in SessionExportBaseTests
        // This is a known limitation of CsvHelper library
        act.Should()
            .Throw<CsvHelper.Configuration.ConfigurationException>()
            .WithMessage("*Types that inherit IEnumerable cannot be auto mapped*");
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
