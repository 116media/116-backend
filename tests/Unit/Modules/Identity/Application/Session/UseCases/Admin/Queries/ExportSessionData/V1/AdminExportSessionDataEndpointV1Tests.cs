using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;
using _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData.V1;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Enums;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData.V1;

/// <summary>
/// Unit tests for <see cref="AdminExportSessionDataEndpointV1"/>.
/// </summary>
public class AdminExportSessionDataEndpointV1Tests
{
    private readonly Mock<ISessionExportService> _exportServiceMock;

    public AdminExportSessionDataEndpointV1Tests()
    {
        _exportServiceMock = new Mock<ISessionExportService>();
    }

    #region Response Tests

    [Fact]
    public void AdminExportSessionDataResponse_ShouldConstructCorrectly()
    {
        var data = new List<SessionExportDto>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "127.0.0.1",
                "Mozilla/5.0",
                default,
                default,
                default,
                default,
                DateTime.UtcNow.AddDays(1),
                true,
                null
            ),
        };

        var response = new AdminExportSessionDataResponse(SessionData: data);

        response.Should().NotBeNull();
        response.SessionData.Should().ContainSingle();
    }

    #endregion

    #region ExportFile Tests

    [Fact]
    public void ExportFile_WithCsvFormat_ShouldReturnFileResult()
    {
        // Arrange
        var sessions = new List<SessionExportDto>();
        var result = new AdminExportSessionDataResult(SessionData: sessions);
        byte[] expectedBytes = [0x01, 0x02, 0x03];

        _exportServiceMock
            .Setup(x =>
                x.Export(It.IsAny<List<SessionExportDto>>(), EnumSessionExportFormat.Csv, It.IsAny<List<string>?>())
            )
            .Returns(expectedBytes);
        _exportServiceMock.Setup(x => x.GetContentType(It.IsAny<EnumSessionExportFormat>())).Returns("text/csv");
        _exportServiceMock
            .Setup(x => x.GenerateFileName(It.IsAny<EnumSessionExportFormat>(), It.IsAny<string?>()))
            .Returns("sessions.csv");

        // Act
        IResult fileResult = AdminExportSessionDataEndpointV1.ExportFile(
            _exportServiceMock.Object,
            result,
            "csv",
            columns: null
        );

        // Assert
        fileResult.Should().NotBeNull();
        fileResult.Should().BeOfType<FileContentHttpResult>();
    }

    [Fact]
    public void ExportFile_WithXlsxFormat_ShouldReturnFileResult()
    {
        // Arrange
        var sessions = new List<SessionExportDto>();
        var result = new AdminExportSessionDataResult(SessionData: sessions);
        byte[] expectedBytes = [0x01, 0x02, 0x03];

        _exportServiceMock
            .Setup(x =>
                x.Export(It.IsAny<List<SessionExportDto>>(), EnumSessionExportFormat.Xlsx, It.IsAny<List<string>?>())
            )
            .Returns(expectedBytes);
        _exportServiceMock
            .Setup(x => x.GetContentType(It.IsAny<EnumSessionExportFormat>()))
            .Returns("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        _exportServiceMock
            .Setup(x => x.GenerateFileName(It.IsAny<EnumSessionExportFormat>(), It.IsAny<string?>()))
            .Returns("sessions.xlsx");

        // Act
        IResult fileResult = AdminExportSessionDataEndpointV1.ExportFile(
            _exportServiceMock.Object,
            result,
            "xlsx",
            columns: null
        );

        // Assert
        fileResult.Should().NotBeNull();
        fileResult.Should().BeOfType<FileContentHttpResult>();
    }

    [Fact]
    public void ExportFile_WithColumns_ShouldParseAndPassColumnList()
    {
        // Arrange
        var sessions = new List<SessionExportDto>();
        var result = new AdminExportSessionDataResult(SessionData: sessions);
        byte[] expectedBytes = [0x01];

        _exportServiceMock
            .Setup(x =>
                x.Export(It.IsAny<List<SessionExportDto>>(), EnumSessionExportFormat.Csv, It.IsAny<List<string>?>())
            )
            .Returns(expectedBytes);
        _exportServiceMock.Setup(x => x.GetContentType(It.IsAny<EnumSessionExportFormat>())).Returns("text/csv");
        _exportServiceMock
            .Setup(x => x.GenerateFileName(It.IsAny<EnumSessionExportFormat>(), It.IsAny<string?>()))
            .Returns("sessions.csv");

        // Act
        IResult fileResult = AdminExportSessionDataEndpointV1.ExportFile(
            _exportServiceMock.Object,
            result,
            "csv",
            columns: "Id, UserName, IpAddress"
        );

        // Assert
        fileResult.Should().NotBeNull();
        _exportServiceMock.Verify(
            x =>
                x.Export(
                    It.IsAny<List<SessionExportDto>>(),
                    EnumSessionExportFormat.Csv,
                    It.Is<List<string>?>(c => c != null && c.Count == 3)
                ),
            Times.Once
        );
    }

    [Fact]
    public void ExportFile_WithNullColumns_ShouldPassNullColumnList()
    {
        // Arrange
        var sessions = new List<SessionExportDto>();
        var result = new AdminExportSessionDataResult(SessionData: sessions);
        byte[] expectedBytes = [0x01];

        _exportServiceMock
            .Setup(x =>
                x.Export(It.IsAny<List<SessionExportDto>>(), EnumSessionExportFormat.Csv, It.IsAny<List<string>?>())
            )
            .Returns(expectedBytes);
        _exportServiceMock.Setup(x => x.GetContentType(It.IsAny<EnumSessionExportFormat>())).Returns("text/csv");
        _exportServiceMock
            .Setup(x => x.GenerateFileName(It.IsAny<EnumSessionExportFormat>(), It.IsAny<string?>()))
            .Returns("sessions.csv");

        // Act
        IResult fileResult = AdminExportSessionDataEndpointV1.ExportFile(
            _exportServiceMock.Object,
            result,
            "csv",
            columns: null
        );

        // Assert
        fileResult.Should().NotBeNull();
        _exportServiceMock.Verify(
            x =>
                x.Export(
                    It.IsAny<List<SessionExportDto>>(),
                    EnumSessionExportFormat.Csv,
                    It.Is<List<string>?>(c => c == null)
                ),
            Times.Once
        );
    }

    [Fact]
    public void ExportFile_WithInvalidFormat_ShouldThrowArgumentException()
    {
        // Arrange
        var sessions = new List<SessionExportDto>();
        var result = new AdminExportSessionDataResult(SessionData: sessions);

        // Act
        Action act = () =>
            AdminExportSessionDataEndpointV1.ExportFile(_exportServiceMock.Object, result, "invalid", columns: null);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion
}
