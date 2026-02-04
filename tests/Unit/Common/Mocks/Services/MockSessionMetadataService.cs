using _116.Identity.Application.Adapters.Wangkanai.Detection;
using _116.Identity.Application.Session.Services;
using _116.Identity.Domain.Enums;
using _116.Unit.Tests.Common.Constants;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Services;

/// <summary>
/// Provides mock setup helpers for <see cref="ISessionMetadataService"/>.
/// </summary>
public static class MockSessionMetadataService
{
    /// <summary>
    /// Creates a new mock instance of ISessionMetadataService.
    /// </summary>
    /// <returns>A configured Mock of ISessionMetadataService.</returns>
    public static Mock<ISessionMetadataService> Create()
    {
        Mock<ISessionMetadataService> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    /// <summary>
    /// Sets up ExtractIpAddress to return the specified IP address.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="ipAddress">The IP address to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionMetadataService> SetupExtractIpAddress(
        this Mock<ISessionMetadataService> mock,
        string? ipAddress
    )
    {
        mock.Setup(x => x.ExtractIpAddress()).Returns(ipAddress);
        return mock;
    }

    /// <summary>
    /// Sets up ExtractUserAgent to return the specified user agent.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userAgent">The user agent to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionMetadataService> SetupExtractUserAgent(
        this Mock<ISessionMetadataService> mock,
        string? userAgent
    )
    {
        mock.Setup(x => x.ExtractUserAgent()).Returns(userAgent);
        return mock;
    }

    /// <summary>
    /// Sets up GetClientOriginInfo to return the specified client origin info.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="clientOriginInfo">The client origin info to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionMetadataService> SetupGetClientOriginInfo(
        this Mock<ISessionMetadataService> mock,
        ClientOriginInfo clientOriginInfo
    )
    {
        mock.Setup(x => x.GetClientOriginInfo()).Returns(clientOriginInfo);
        return mock;
    }

    /// <summary>
    /// Sets up GetClientOriginInfo with specific browser, device, and platform.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="browser">The browser type.</param>
    /// <param name="device">The device type.</param>
    /// <param name="platform">The platform type.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionMetadataService> SetupGetClientOriginInfo(
        this Mock<ISessionMetadataService> mock,
        EnumBrowser browser,
        EnumDevice device,
        EnumPlatform platform
    )
    {
        ClientOriginInfo info = new(browser, device, platform);
        mock.Setup(x => x.GetClientOriginInfo()).Returns(info);
        return mock;
    }

    /// <summary>
    /// Sets up ExtractClientApp to return the specified client type.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="clientApp">The client app type to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionMetadataService> SetupExtractClientApp(
        this Mock<ISessionMetadataService> mock,
        EnumClient clientApp
    )
    {
        mock.Setup(x => x.ExtractClientApp()).Returns(clientApp);
        return mock;
    }

    /// <summary>
    /// Sets up ExtractDeviceId to return the specified device ID.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="deviceId">The device ID to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionMetadataService> SetupExtractDeviceId(
        this Mock<ISessionMetadataService> mock,
        string? deviceId
    )
    {
        mock.Setup(x => x.ExtractDeviceId()).Returns(deviceId);
        return mock;
    }

    /// <summary>
    /// Sets up all session metadata with the specified values.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="ipAddress">The IP address.</param>
    /// <param name="userAgent">The user agent.</param>
    /// <param name="browser">The browser type.</param>
    /// <param name="device">The device type.</param>
    /// <param name="platform">The platform type.</param>
    /// <param name="clientApp">The client app type.</param>
    /// <param name="deviceId">The device ID.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionMetadataService> SetupAllMetadata(
        this Mock<ISessionMetadataService> mock,
        string? ipAddress,
        string? userAgent,
        EnumBrowser browser,
        EnumDevice device,
        EnumPlatform platform,
        EnumClient clientApp,
        string? deviceId
    )
    {
        mock.SetupExtractIpAddress(ipAddress);
        mock.SetupExtractUserAgent(userAgent);
        mock.SetupGetClientOriginInfo(browser, device, platform);
        mock.SetupExtractClientApp(clientApp);
        mock.SetupExtractDeviceId(deviceId);
        return mock;
    }

    /// <summary>
    /// Verifies that ExtractIpAddress was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyExtractIpAddressCalled(this Mock<ISessionMetadataService> mock)
    {
        mock.Verify(x => x.ExtractIpAddress(), Times.Once);
    }

    /// <summary>
    /// Verifies that GetClientOriginInfo was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyGetClientOriginInfoCalled(this Mock<ISessionMetadataService> mock)
    {
        mock.Verify(x => x.GetClientOriginInfo(), Times.Once);
    }

    /// <summary>
    /// Verifies that ExtractDeviceId was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyExtractDeviceIdCalled(this Mock<ISessionMetadataService> mock)
    {
        mock.Verify(x => x.ExtractDeviceId(), Times.Once);
    }

    /// <summary>
    /// Sets up default behaviors for the mock.
    /// </summary>
    private static void SetupDefaults(Mock<ISessionMetadataService> mock)
    {
        mock.Setup(x => x.ExtractIpAddress()).Returns(TestConstants.Session.DefaultIpAddress);

        mock.Setup(x => x.ExtractUserAgent()).Returns(TestConstants.Session.DefaultUserAgent);

        mock.Setup(x => x.GetClientOriginInfo())
            .Returns(
                new ClientOriginInfo(
                    TestConstants.Session.DefaultBrowser,
                    TestConstants.Session.DefaultDevice,
                    TestConstants.Session.DefaultPlatform
                )
            );

        mock.Setup(x => x.ExtractClientApp()).Returns(TestConstants.Session.DefaultClient);

        mock.Setup(x => x.ExtractDeviceId()).Returns(TestConstants.Session.DefaultDeviceId);
    }
}
