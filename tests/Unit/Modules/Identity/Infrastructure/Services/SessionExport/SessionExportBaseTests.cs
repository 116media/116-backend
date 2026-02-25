using System.Dynamic;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Services.SessionExport;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Services.SessionExport;

/// <summary>
/// Unit tests for <see cref="SessionExportBase"/>.
/// </summary>
public class SessionExportBaseTests
{
    private class TestableSessionExportBase : SessionExportBase
    {
        public static List<ExpandoObject> PublicGetFilteredColumns(
            List<SessionExportDto> sessions,
            List<string> columns
        )
        {
            return GetFilteredColumns(sessions, columns);
        }
    }

    [Fact]
    public void GetFilteredColumns_WithAllColumns_ShouldReturnAllProperties()
    {
        // Arrange
        var sessions = new List<SessionExportDto>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "192.168.1.1",
                "Mozilla/5.0",
                EnumBrowser.Chrome,
                EnumDevice.Desktop,
                EnumPlatform.Windows,
                EnumClient.WebApp,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(1),
                true,
                null
            ),
        };

        var columns = new List<string>
        {
            "Id",
            "UserId",
            "IpAddress",
            "UserAgent",
            "Browser",
            "Device",
            "Platform",
            "Client",
            "CreatedAt",
            "ExpiresAt",
            "IsActive",
            "DeletedAt",
        };

        // Act
        List<ExpandoObject> result = TestableSessionExportBase.PublicGetFilteredColumns(sessions, columns);

        // Assert
        result.Should().ContainSingle();
        var firstResult = (IDictionary<string, object?>)result[0];
        firstResult.Should().HaveCount(12);
        firstResult.Should().ContainKey("Id");
        firstResult.Should().ContainKey("UserId");
        firstResult.Should().ContainKey("IpAddress");
    }

    [Fact]
    public void GetFilteredColumns_WithSelectedColumns_ShouldReturnOnlySelectedProperties()
    {
        // Arrange
        var sessions = new List<SessionExportDto>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "192.168.1.1",
                "Mozilla/5.0",
                EnumBrowser.Chrome,
                EnumDevice.Desktop,
                EnumPlatform.Windows,
                EnumClient.WebApp,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(1),
                true,
                null
            ),
        };

        var columns = new List<string> { "Id", "IpAddress", "IsActive" };

        // Act
        List<ExpandoObject> result = TestableSessionExportBase.PublicGetFilteredColumns(sessions, columns);

        // Assert
        result.Should().ContainSingle();
        var firstResult = (IDictionary<string, object?>)result[0];
        firstResult.Should().HaveCount(3);
        firstResult.Should().ContainKey("Id");
        firstResult.Should().ContainKey("IpAddress");
        firstResult.Should().ContainKey("IsActive");
        firstResult.Should().NotContainKey("UserId");
        firstResult.Should().NotContainKey("UserAgent");
    }

    [Fact]
    public void GetFilteredColumns_WithCaseInsensitiveColumns_ShouldMatchColumns()
    {
        // Arrange
        var sessions = new List<SessionExportDto>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "192.168.1.1",
                "Mozilla/5.0",
                EnumBrowser.Chrome,
                EnumDevice.Desktop,
                EnumPlatform.Windows,
                EnumClient.WebApp,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(1),
                true,
                null
            ),
        };

        var columns = new List<string> { "id", "ipaddress", "isactive" };

        // Act
        List<ExpandoObject> result = TestableSessionExportBase.PublicGetFilteredColumns(sessions, columns);

        // Assert
        result.Should().ContainSingle();
        var firstResult = (IDictionary<string, object?>)result[0];
        firstResult.Should().HaveCount(3);
        firstResult.Should().ContainKey("Id");
        firstResult.Should().ContainKey("IpAddress");
        firstResult.Should().ContainKey("IsActive");
    }

    [Fact]
    public void GetFilteredColumns_WithMultipleSessions_ShouldReturnAllSessions()
    {
        // Arrange
        var sessions = new List<SessionExportDto>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "192.168.1.1",
                "Mozilla/5.0",
                EnumBrowser.Chrome,
                EnumDevice.Desktop,
                EnumPlatform.Windows,
                EnumClient.WebApp,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(1),
                true,
                null
            ),
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "192.168.1.2",
                "Mozilla/5.0",
                EnumBrowser.Firefox,
                EnumDevice.Mobile,
                EnumPlatform.Android,
                EnumClient.MobileApp,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(1),
                false,
                DateTime.UtcNow
            ),
        };

        var columns = new List<string> { "Id", "IpAddress" };

        // Act
        List<ExpandoObject> result = TestableSessionExportBase.PublicGetFilteredColumns(sessions, columns);

        // Assert
        result.Should().HaveCount(2);
        var firstResult = (IDictionary<string, object?>)result[0];
        var secondResult = (IDictionary<string, object?>)result[1];

        firstResult["IpAddress"].Should().Be("192.168.1.1");
        secondResult["IpAddress"].Should().Be("192.168.1.2");
    }

    [Fact]
    public void GetFilteredColumns_WithEmptySessions_ShouldReturnEmptyList()
    {
        // Arrange
        var sessions = new List<SessionExportDto>();
        var columns = new List<string> { "Id", "IpAddress" };

        // Act
        List<ExpandoObject> result = TestableSessionExportBase.PublicGetFilteredColumns(sessions, columns);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetFilteredColumns_WithEmptyColumns_ShouldReturnEmptyProperties()
    {
        // Arrange
        var sessions = new List<SessionExportDto>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "192.168.1.1",
                "Mozilla/5.0",
                EnumBrowser.Chrome,
                EnumDevice.Desktop,
                EnumPlatform.Windows,
                EnumClient.WebApp,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(1),
                true,
                null
            ),
        };

        var columns = new List<string>();

        // Act
        List<ExpandoObject> result = TestableSessionExportBase.PublicGetFilteredColumns(sessions, columns);

        // Assert
        result.Should().ContainSingle();
        var firstResult = (IDictionary<string, object?>)result[0];
        firstResult.Should().BeEmpty();
    }

    [Fact]
    public void GetFilteredColumns_WithNonExistentColumn_ShouldIgnoreNonExistentColumn()
    {
        // Arrange
        var sessions = new List<SessionExportDto>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "192.168.1.1",
                "Mozilla/5.0",
                EnumBrowser.Chrome,
                EnumDevice.Desktop,
                EnumPlatform.Windows,
                EnumClient.WebApp,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(1),
                true,
                null
            ),
        };

        var columns = new List<string> { "Id", "NonExistentColumn", "IpAddress" };

        // Act
        List<ExpandoObject> result = TestableSessionExportBase.PublicGetFilteredColumns(sessions, columns);

        // Assert
        result.Should().ContainSingle();
        var firstResult = (IDictionary<string, object?>)result[0];
        firstResult.Should().HaveCount(2);
        firstResult.Should().ContainKey("Id");
        firstResult.Should().ContainKey("IpAddress");
        firstResult.Should().NotContainKey("NonExistentColumn");
    }

    [Fact]
    public void GetFilteredColumns_ShouldPreservePropertyValues()
    {
        // Arrange
        Guid sessionId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        DateTime createdAt = DateTime.UtcNow;

        var sessions = new List<SessionExportDto>
        {
            new(
                sessionId,
                userId,
                "192.168.1.1",
                "Mozilla/5.0",
                EnumBrowser.Chrome,
                EnumDevice.Desktop,
                EnumPlatform.Windows,
                EnumClient.WebApp,
                createdAt,
                DateTime.UtcNow.AddDays(1),
                true,
                null
            ),
        };

        var columns = new List<string> { "Id", "UserId", "CreatedAt", "IsActive" };

        // Act
        List<ExpandoObject> result = TestableSessionExportBase.PublicGetFilteredColumns(sessions, columns);

        // Assert
        var firstResult = (IDictionary<string, object?>)result[0];
        firstResult["Id"].Should().Be(sessionId);
        firstResult["UserId"].Should().Be(userId);
        firstResult["CreatedAt"].Should().Be(createdAt);
        firstResult["IsActive"].Should().Be(true);
    }
}
