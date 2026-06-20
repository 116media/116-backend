using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Enums;

namespace _116.Integration.Tests.Modules.Identity.Infrastructure.Services;

/// <summary>
/// Integration tests for <see cref="ISessionExportService" /> verifying CSV
/// and XLSX export against the DI-resolved service instance.
/// </summary>
[Collection("Database")]
public class SessionExportServiceTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private static SessionExportDto CreateSampleSession()
    {
        return new SessionExportDto(
            Id: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            IpAddress: "192.168.1.1",
            UserAgent: "Mozilla/5.0",
            Browser: EnumBrowser.Chrome,
            Device: EnumDevice.Desktop,
            Platform: EnumPlatform.Windows,
            Client: EnumClient.WebApp,
            ExpiresAt: DateTime.UtcNow.AddHours(1),
            IsActive: true,
            DeletedAt: null
        );
    }

    [Fact]
    public void Export_CsvFormat_ReturnsNonEmptyByteArray()
    {
        var service = Resolve<ISessionExportService>();
        var sessions = new List<SessionExportDto> { CreateSampleSession() };

        var result = service.Export(sessions, EnumSessionExportFormat.Csv);

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Export_XlsxFormat_ReturnsNonEmptyByteArray()
    {
        var service = Resolve<ISessionExportService>();
        var sessions = new List<SessionExportDto> { CreateSampleSession() };

        var result = service.Export(sessions, EnumSessionExportFormat.Xlsx);

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Export_CsvFormat_WithEmptyList_ReturnsNonEmptyByteArray()
    {
        var service = Resolve<ISessionExportService>();

        var result = service.Export(new List<SessionExportDto>(), EnumSessionExportFormat.Csv);

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Export_XlsxFormat_WithEmptyList_ReturnsNonEmptyByteArray()
    {
        var service = Resolve<ISessionExportService>();

        var result = service.Export(new List<SessionExportDto>(), EnumSessionExportFormat.Xlsx);

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Export_CsvFormat_WithMultipleSessions_ReturnsLargerOutput()
    {
        var service = Resolve<ISessionExportService>();
        var singleSession = new List<SessionExportDto> { CreateSampleSession() };
        var multipleSessions = new List<SessionExportDto>
        {
            CreateSampleSession(),
            CreateSampleSession(),
            CreateSampleSession(),
        };

        var singleResult = service.Export(singleSession, EnumSessionExportFormat.Csv);
        var multipleResult = service.Export(multipleSessions, EnumSessionExportFormat.Csv);

        multipleResult.Length.Should().BeGreaterThan(singleResult.Length);
    }

    [Fact]
    public void GetContentType_CsvFormat_ReturnsCsvContentType()
    {
        var service = Resolve<ISessionExportService>();

        var contentType = service.GetContentType(EnumSessionExportFormat.Csv);

        contentType.Should().Be("text/csv");
    }

    [Fact]
    public void GetContentType_XlsxFormat_ReturnsXlsxContentType()
    {
        var service = Resolve<ISessionExportService>();

        var contentType = service.GetContentType(EnumSessionExportFormat.Xlsx);

        contentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    [Fact]
    public void GenerateFileName_CsvFormat_ReturnsFileNameWithCsvExtension()
    {
        var service = Resolve<ISessionExportService>();

        var fileName = service.GenerateFileName(EnumSessionExportFormat.Csv);

        fileName.Should().StartWith("sessions-export-");
        fileName.Should().EndWith(".csv");
    }

    [Fact]
    public void GenerateFileName_XlsxFormat_ReturnsFileNameWithXlsxExtension()
    {
        var service = Resolve<ISessionExportService>();

        var fileName = service.GenerateFileName(EnumSessionExportFormat.Xlsx);

        fileName.Should().StartWith("sessions-export-");
        fileName.Should().EndWith(".xlsx");
    }

    [Fact]
    public void GenerateFileName_WithCustomBaseName_UsesCustomBaseName()
    {
        var service = Resolve<ISessionExportService>();

        var fileName = service.GenerateFileName(EnumSessionExportFormat.Csv, "custom-export");

        fileName.Should().StartWith("custom-export-");
        fileName.Should().EndWith(".csv");
    }
}
