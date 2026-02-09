using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Unit.Tests.Common.Constants;
using Bogus;

namespace _116.Unit.Tests.Common.Builders.Entities;

/// <summary>
/// Fluent builder for creating <see cref="SessionEntity"/> instances in tests.
/// For test code, prefer using SessionFactory instead of direct Builder usage.
/// </summary>
internal class SessionBuilder
{
    private readonly Faker _faker = new();

    private Guid _id;
    private Guid _userId;
    private string _deviceId;
    private string _refreshTokenHash;
    private DateTime _expiresAt;
    private EnumBrowser _browser = EnumBrowser.Chrome;
    private EnumDevice _device = EnumDevice.Desktop;
    private EnumPlatform _platform = EnumPlatform.Windows;
    private EnumClient _client = EnumClient.WebApp;
    private string? _ipAddress;
    private string? _userAgent;
    private bool _isRevoked;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionBuilder"/> class with random default values.
    /// </summary>
    public SessionBuilder()
    {
        _id = Guid.NewGuid();
        _userId = Guid.NewGuid();
        _deviceId = $"device-{_faker.Random.AlphaNumeric(16)}";
        _refreshTokenHash = _faker.Random.Hash();
        _expiresAt = DateTime.UtcNow.AddDays(TestConstants.Session.DefaultRefreshTokenExpirationDays);
        _ipAddress = TestConstants.Session.ValidIpAddress;
        _userAgent = TestConstants.Session.ValidUserAgent;
    }

    /// <summary>
    /// Sets the session ID.
    /// </summary>
    /// <param name="id">The session identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public SessionBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the user ID.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public SessionBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    /// <summary>
    /// Sets the device ID.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public SessionBuilder WithDeviceId(string deviceId)
    {
        _deviceId = deviceId;
        return this;
    }

    /// <summary>
    /// Sets the refresh token hash.
    /// </summary>
    /// <param name="refreshTokenHash">The refresh token hash.</param>
    /// <returns>The builder instance for chaining.</returns>
    public SessionBuilder WithRefreshTokenHash(string refreshTokenHash)
    {
        _refreshTokenHash = refreshTokenHash;
        return this;
    }

    /// <summary>
    /// Sets the expiration date.
    /// </summary>
    /// <param name="expiresAt">The expiration date.</param>
    /// <returns>The builder instance for chaining.</returns>
    public SessionBuilder WithExpiresAt(DateTime expiresAt)
    {
        _expiresAt = expiresAt;
        return this;
    }

    /// <summary>
    /// Sets the session as expired.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public SessionBuilder AsExpired()
    {
        _expiresAt = DateTime.UtcNow.AddDays(-1);
        return this;
    }

    /// <summary>
    /// Sets the browser type.
    /// </summary>
    /// <param name="browser">The browser type.</param>
    /// <returns>The builder instance for chaining.</returns>
    public SessionBuilder WithBrowser(EnumBrowser browser)
    {
        _browser = browser;
        return this;
    }

    /// <summary>
    /// Sets the device type.
    /// </summary>
    /// <param name="device">The device type.</param>
    /// <returns>The builder instance for chaining.</returns>
    public SessionBuilder WithDevice(EnumDevice device)
    {
        _device = device;
        return this;
    }

    /// <summary>
    /// Sets the platform type.
    /// </summary>
    /// <param name="platform">The platform type.</param>
    /// <returns>The builder instance for chaining.</returns>
    public SessionBuilder WithPlatform(EnumPlatform platform)
    {
        _platform = platform;
        return this;
    }

    /// <summary>
    /// Sets the client type.
    /// </summary>
    /// <param name="client">The client type.</param>
    /// <returns>The builder instance for chaining.</returns>
    public SessionBuilder WithClient(EnumClient client)
    {
        _client = client;
        return this;
    }

    /// <summary>
    /// Sets the IP address.
    /// </summary>
    /// <param name="ipAddress">The IP address.</param>
    /// <returns>The builder instance for chaining.</returns>
    public SessionBuilder WithIpAddress(string? ipAddress)
    {
        _ipAddress = ipAddress;
        return this;
    }

    /// <summary>
    /// Sets the user agent.
    /// </summary>
    /// <param name="userAgent">The user agent string.</param>
    /// <returns>The builder instance for chaining.</returns>
    public SessionBuilder WithUserAgent(string? userAgent)
    {
        _userAgent = userAgent;
        return this;
    }

    /// <summary>
    /// Marks the session as revoked.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public SessionBuilder AsRevoked()
    {
        _isRevoked = true;
        return this;
    }

    /// <summary>
    /// Configures the session for a mobile device.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public SessionBuilder AsMobileSession()
    {
        _device = EnumDevice.Mobile;
        _client = EnumClient.MobileApp;
        _platform = EnumPlatform.Ios;
        _browser = EnumBrowser.Safari;
        return this;
    }

    /// <summary>
    /// Configures the session for a desktop device.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public SessionBuilder AsDesktopSession()
    {
        _device = EnumDevice.Desktop;
        _client = EnumClient.WebApp;
        _platform = EnumPlatform.Windows;
        _browser = EnumBrowser.Chrome;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="SessionEntity"/> instance.
    /// </summary>
    /// <returns>A configured SessionEntity instance.</returns>
    public SessionEntity Build()
    {
        var session = SessionEntity.Create(
            _id,
            _userId,
            _deviceId,
            _refreshTokenHash,
            _expiresAt,
            _browser,
            _device,
            _platform,
            _client,
            _ipAddress,
            _userAgent
        );

        if (_isRevoked)
        {
            session.Revoke();
        }

        return session;
    }
}
