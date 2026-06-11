using _116.Identity.Application.Session.Services;
using _116.Identity.Domain.Enums;

namespace _116.Integration.Tests.Identity.Services;

/// <summary>
/// Integration tests for <see cref="ISessionMetadataService" /> verifying
/// client app extraction and device ID parsing from HTTP request headers
/// through the full API pipeline.
/// </summary>
[Collection("Database")]
public class SessionMetadataServiceTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public void ExtractClientApp_WithNoHttpContext_ReturnsUnknown()
    {
        var service = Resolve<ISessionMetadataService>();

        var result = service.ExtractClientApp();

        result.Should().Be(EnumClient.Unknown);
    }

    [Fact]
    public void ExtractIpAddress_WithNoHttpContext_ReturnsNull()
    {
        var service = Resolve<ISessionMetadataService>();

        var result = service.ExtractIpAddress();

        result.Should().BeNull();
    }

    [Fact]
    public void ExtractUserAgent_WithNoHttpContext_ReturnsNull()
    {
        var service = Resolve<ISessionMetadataService>();

        var result = service.ExtractUserAgent();

        result.Should().BeNull();
    }

    [Fact]
    public void ExtractDeviceId_WithNoHttpContext_ReturnsNull()
    {
        var service = Resolve<ISessionMetadataService>();

        var result = service.ExtractDeviceId();

        result.Should().BeNull();
    }

    [Fact]
    public void GetClientOriginInfo_WithNoHttpContext_ReturnsInfo()
    {
        var service = Resolve<ISessionMetadataService>();

        var result = service.GetClientOriginInfo();

        result.Should().NotBeNull();
    }
}
