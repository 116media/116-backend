using System.Reflection;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Entities.Identity;

/// <summary>
/// Fluent builder for creating <see cref="SessionEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; SessionFactory only names chains three or more tests share.
/// </summary>
public class SessionBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private Guid _id;
    private Guid _userId;
    private string _deviceId;
    private string _refreshTokenHash;
    private DateTime _expiresAt;
    private DateTime _absoluteExpiresAt;
    private EnumBrowser _browser = EnumBrowser.Chrome;
    private EnumDevice _device = EnumDevice.Desktop;
    private EnumPlatform _platform = EnumPlatform.Windows;
    private EnumClient _client = EnumClient.WebApp;
    private string? _ipAddress;
    private string? _userAgent;
    private bool _isRevoked;
    private DateTime? _createdAt;
    private UserEntity? _user;

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
        _absoluteExpiresAt = DateTime.UtcNow.AddDays(TestConstants.Session.DefaultAbsoluteLifetimeDays);
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
    /// Sets the absolute expiration ceiling.
    /// </summary>
    /// <param name="absoluteExpiresAt">The absolute expiration date.</param>
    /// <returns>The builder instance for chaining.</returns>
    public SessionBuilder WithAbsoluteExpiresAt(DateTime absoluteExpiresAt)
    {
        _absoluteExpiresAt = absoluteExpiresAt;
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
    /// Overrides the <c>CreatedAt</c> audit stamp the persistence interceptor would write,
    /// for tests that order or filter sessions by creation time.
    /// </summary>
    /// <param name="createdAt">The creation timestamp to stamp on the session.</param>
    /// <returns>The builder instance for chaining.</returns>
    public SessionBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// Attaches the User navigation EF Core populates through <c>.Include(s =&gt; s.User)</c>,
    /// and points the foreign key at the same user.
    /// </summary>
    /// <param name="user">The owning user the session should carry.</param>
    /// <returns>The builder instance for chaining.</returns>
    public SessionBuilder WithUser(UserEntity user)
    {
        _user = user;
        _userId = user.Id;
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
            _absoluteExpiresAt,
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

        if (_createdAt.HasValue)
        {
            session.CreatedAt = _createdAt.Value;
        }

        if (_user is not null)
        {
            typeof(SessionEntity)
                .GetProperty(nameof(SessionEntity.User), BindingFlags.Public | BindingFlags.Instance)!
                .SetValue(session, _user);
        }

        return session;
    }
}
