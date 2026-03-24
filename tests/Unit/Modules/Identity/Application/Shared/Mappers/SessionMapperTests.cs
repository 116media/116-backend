using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.DTOs;
using _116.Tests.Fixtures.Factories;
using AwesomeAssertions;
using Mapster;
using MapsterMapper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Mappers;

/// <summary>
/// Unit tests for <see cref="SessionMapper"/>.
/// </summary>
public class SessionMapperTests
{
    private readonly IMapper _mapper;

    public SessionMapperTests()
    {
        TypeAdapterConfig config = MappingRegistration.CreateConfiguration();
        _mapper = new Mapper(config);
    }

    #region Register

    [Fact]
    public void Register_ShouldNotThrowException()
    {
        // Arrange
        var config = new TypeAdapterConfig();

        // Act & Assert
        Action act = () => SessionMapper.Register(config);
        act.Should().NotThrow();
    }

    #endregion

    #region SessionDto — core fields

    [Fact]
    public void ToSessionDto_ShouldMapCoreFieldsCorrectly()
    {
        // Arrange
        var session = SessionFactory.Create();

        // Act
        var dto = session.ToSessionDto(_mapper);

        // Assert
        dto.Id.Should().Be(session.Id);
        dto.IpAddress.Should().Be(session.IpAddress);
        dto.UserAgent.Should().Be(session.UserAgent);
        dto.ExpiresAt.Should().Be(session.ExpiresAt);
    }

    [Fact]
    public void ToSessionDto_ShouldMapBrowserDevicePlatformClientAsStrings()
    {
        // Arrange
        var session = SessionFactory.CreateWithBrowser(EnumBrowser.Chrome);

        // Act
        var dto = session.ToSessionDto(_mapper);

        // Assert
        dto.Browser.Should().Be(session.Browser.ToString());
        dto.Device.Should().Be(session.Device.ToString());
        dto.Platform.Should().Be(session.Platform.ToString());
        dto.Client.Should().Be(session.Client.ToString());
    }

    [Fact]
    public void ToSessionDto_WhenNotRevokedAndNotExpired_ShouldBeActive()
    {
        // Arrange
        var session = SessionFactory.Create();

        // Act
        var dto = session.ToSessionDto(_mapper);

        // Assert
        dto.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ToSessionDto_WhenRevoked_ShouldBeInactive()
    {
        // Arrange
        var session = SessionFactory.CreateRevoked();

        // Act
        var dto = session.ToSessionDto(_mapper);

        // Assert
        dto.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ToSessionDto_WhenExpired_ShouldBeInactive()
    {
        // Arrange
        var session = SessionFactory.CreateExpired();

        // Act
        var dto = session.ToSessionDto(_mapper);

        // Assert
        dto.IsActive.Should().BeFalse();
    }

    #endregion

    #region SessionDto — AuditableDto fields

    [Fact]
    public void ToSessionDto_ShouldInheritAuditableDto()
    {
        // Arrange
        var session = SessionFactory.Create();

        // Act
        var dto = session.ToSessionDto(_mapper);

        // Assert
        dto.Should().BeAssignableTo<AuditableDto>();
    }

    [Fact]
    public void ToSessionDto_ShouldMapCreatedAtFromEntity()
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow.AddDays(-5);
        var session = SessionFactory.Create();
        session.CreatedAt = createdAt;

        // Act
        var dto = session.ToSessionDto(_mapper);

        // Assert
        dto.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void ToSessionDto_ShouldMapUpdatedAtFromEntity()
    {
        // Arrange
        DateTime updatedAt = DateTime.UtcNow.AddHours(-1);
        var session = SessionFactory.Create();
        session.UpdatedAt = updatedAt;

        // Act
        var dto = session.ToSessionDto(_mapper);

        // Assert
        dto.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void ToSessionDto_ShouldMapCreatedByFromEntity()
    {
        // Arrange
        var session = SessionFactory.Create();
        session.CreatedBy = "system";

        // Act
        var dto = session.ToSessionDto(_mapper);

        // Assert
        dto.CreatedBy.Should().Be("system");
    }

    [Fact]
    public void ToSessionDto_ShouldMapUpdatedByFromEntity()
    {
        // Arrange
        var session = SessionFactory.Create();
        session.UpdatedBy = "admin-user";

        // Act
        var dto = session.ToSessionDto(_mapper);

        // Assert
        dto.UpdatedBy.Should().Be("admin-user");
    }

    [Fact]
    public void ToSessionDto_WhenAuditFieldsNotSet_ShouldBeNull()
    {
        // Arrange
        var session = SessionFactory.Create();
        session.CreatedBy = null;
        session.UpdatedBy = null;

        // Act
        var dto = session.ToSessionDto(_mapper);

        // Assert
        dto.CreatedBy.Should().BeNull();
        dto.UpdatedBy.Should().BeNull();
    }

    #endregion

    #region SessionExportDto — AuditableDto fields

    [Fact]
    public void ToSessionExportDtos_ShouldInheritAuditableDto()
    {
        // Arrange
        var sessions = SessionFactory.CreateMany(1);

        // Act
        var dtos = sessions.ToSessionExportDtos(_mapper);

        // Assert
        dtos.Should().ContainSingle();
        dtos[0].Should().BeAssignableTo<AuditableDto>();
    }

    [Fact]
    public void ToSessionExportDtos_ShouldMapCreatedAtFromEntity()
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow.AddDays(-2);
        var session = SessionFactory.Create();
        session.CreatedAt = createdAt;

        // Act
        var dtos = new List<_116.Identity.Domain.Entities.SessionEntity> { session }.ToSessionExportDtos(_mapper);

        // Assert
        dtos[0].CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void ToSessionExportDtos_ShouldMapUpdatedAtFromEntity()
    {
        // Arrange
        DateTime updatedAt = DateTime.UtcNow.AddMinutes(-30);
        var session = SessionFactory.Create();
        session.UpdatedAt = updatedAt;

        // Act
        var dtos = new List<_116.Identity.Domain.Entities.SessionEntity> { session }.ToSessionExportDtos(_mapper);

        // Assert
        dtos[0].UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void ToSessionExportDtos_ShouldMapCreatedByFromEntity()
    {
        // Arrange
        var session = SessionFactory.Create();
        session.CreatedBy = "system";

        // Act
        var dtos = new List<_116.Identity.Domain.Entities.SessionEntity> { session }.ToSessionExportDtos(_mapper);

        // Assert
        dtos[0].CreatedBy.Should().Be("system");
    }

    [Fact]
    public void ToSessionExportDtos_ShouldMapCoreExportFields()
    {
        // Arrange
        var session = SessionFactory.Create();

        // Act
        var dtos = new List<_116.Identity.Domain.Entities.SessionEntity> { session }.ToSessionExportDtos(_mapper);

        // Assert
        var dto = dtos[0];
        dto.Id.Should().Be(session.Id);
        dto.UserId.Should().Be(session.UserId);
        dto.IpAddress.Should().Be(session.IpAddress);
        dto.ExpiresAt.Should().Be(session.ExpiresAt);
    }

    [Fact]
    public void ToSessionExportDtos_WithMultipleSessions_ShouldMapAll()
    {
        // Arrange
        var sessions = SessionFactory.CreateMany(3);

        // Act
        var dtos = sessions.ToSessionExportDtos(_mapper);

        // Assert
        dtos.Should().HaveCount(3);
        dtos.Select(d => d.Id).Should().BeEquivalentTo(sessions.Select(s => s.Id));
    }

    #endregion

    #region SessionDto — with expression

    [Fact]
    public void SessionDto_AuditFields_CanBeSetViaWithExpression()
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow.AddDays(-1);
        var dto = new SessionDto(
            Guid.NewGuid(),
            "127.0.0.1",
            "Mozilla/5.0",
            "Chrome",
            "Desktop",
            "Windows",
            "WebApp",
            DateTime.UtcNow.AddDays(1),
            true
        );

        // Act
        var updated = dto with
        {
            CreatedAt = createdAt,
            CreatedBy = "system",
        };

        // Assert
        updated.CreatedAt.Should().Be(createdAt);
        updated.CreatedBy.Should().Be("system");
        updated.UpdatedAt.Should().BeNull();
        updated.UpdatedBy.Should().BeNull();
    }

    #endregion
}
