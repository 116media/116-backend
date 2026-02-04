using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.UseCases.Admin.Queries.GetSessionMetrics;
using _116.Identity.Domain.Enums;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Sessions.Admin.Queries.GetSessionMetrics;

/// <summary>
/// Unit tests for <see cref="AdminGetSessionMetricsHandler"/>.
/// </summary>
public class AdminGetSessionMetricsHandlerTests
{
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly AdminGetSessionMetricsHandler _handler;

    public AdminGetSessionMetricsHandlerTests()
    {
        _sessionRepositoryMock = MockSessionRepository.Create();
        _handler = new AdminGetSessionMetricsHandler(_sessionRepositoryMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldReturnSessionMetrics()
    {
        // Arrange
        AdminGetSessionMetricsQuery query = new();
        SetupDefaultMetrics();

        // Act
        AdminGetSessionMetricsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Browsers.Should().NotBeNull();
        result.Devices.Should().NotBeNull();
        result.Platforms.Should().NotBeNull();
        result.Clients.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectBrowserMetrics()
    {
        // Arrange
        AdminGetSessionMetricsQuery query = new();
        Dictionary<EnumBrowser, int> browserCounts = new()
        {
            { EnumBrowser.Chrome, 50 },
            { EnumBrowser.Firefox, 20 },
            { EnumBrowser.Safari, 15 },
            { EnumBrowser.Edge, 10 },
            { EnumBrowser.Unknown, 5 },
        };

        _sessionRepositoryMock.SetupGetActiveSessionCountByBrowser(browserCounts);
        SetupOtherMetrics();

        // Act
        AdminGetSessionMetricsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Browsers.Chrome.Should().Be(50);
        result.Browsers.Firefox.Should().Be(20);
        result.Browsers.Safari.Should().Be(15);
        result.Browsers.Edge.Should().Be(10);
        result.Browsers.Unknown.Should().Be(5);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectDeviceMetrics()
    {
        // Arrange
        AdminGetSessionMetricsQuery query = new();
        Dictionary<EnumDevice, int> deviceCounts = new()
        {
            { EnumDevice.Desktop, 60 },
            { EnumDevice.Mobile, 30 },
            { EnumDevice.Tablet, 10 },
        };

        _sessionRepositoryMock.SetupGetActiveSessionCountByDevice(deviceCounts);
        SetupOtherMetricsExceptDevice();

        // Act
        AdminGetSessionMetricsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Devices.Desktop.Should().Be(60);
        result.Devices.Mobile.Should().Be(30);
        result.Devices.Tablet.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectPlatformMetrics()
    {
        // Arrange
        AdminGetSessionMetricsQuery query = new();
        Dictionary<EnumPlatform, int> platformCounts = new()
        {
            { EnumPlatform.Windows, 40 },
            { EnumPlatform.Mac, 25 },
            { EnumPlatform.Android, 20 },
            { EnumPlatform.Ios, 15 },
        };

        _sessionRepositoryMock.SetupGetActiveSessionCountByPlatform(platformCounts);
        SetupOtherMetricsExceptPlatform();

        // Act
        AdminGetSessionMetricsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Platforms.Windows.Should().Be(40);
        result.Platforms.Mac.Should().Be(25);
        result.Platforms.Android.Should().Be(20);
        result.Platforms.Ios.Should().Be(15);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectClientMetrics()
    {
        // Arrange
        AdminGetSessionMetricsQuery query = new();
        Dictionary<EnumClient, int> clientCounts = new()
        {
            { EnumClient.WebApp, 70 },
            { EnumClient.MobileApp, 25 },
            { EnumClient.Dashboard, 5 },
        };

        _sessionRepositoryMock.SetupGetActiveSessionCountByClient(clientCounts);
        SetupOtherMetricsExceptClient();

        // Act
        AdminGetSessionMetricsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Clients.WebApp.Should().Be(70);
        result.Clients.MobileApp.Should().Be(25);
        result.Clients.Dashboard.Should().Be(5);
    }

    [Fact]
    public async Task Handle_ShouldReturnTotalActiveSessionsCount()
    {
        // Arrange
        AdminGetSessionMetricsQuery query = new();
        _sessionRepositoryMock.SetupGetActiveSessionCountByBrowser(new Dictionary<EnumBrowser, int>());
        _sessionRepositoryMock.SetupGetActiveSessionCountByDevice(new Dictionary<EnumDevice, int>());
        _sessionRepositoryMock.SetupGetActiveSessionCountByPlatform(new Dictionary<EnumPlatform, int>());
        _sessionRepositoryMock.SetupGetActiveSessionCountByClient(new Dictionary<EnumClient, int>());
        _sessionRepositoryMock.SetupGetTotalActiveSessionsCount(100);
        _sessionRepositoryMock.SetupGetTotalActiveUsersCount(0);

        // Act
        AdminGetSessionMetricsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalActiveSessions.Should().Be(100);
    }

    [Fact]
    public async Task Handle_ShouldReturnTotalActiveUsersCount()
    {
        // Arrange
        AdminGetSessionMetricsQuery query = new();
        _sessionRepositoryMock.SetupGetActiveSessionCountByBrowser(new Dictionary<EnumBrowser, int>());
        _sessionRepositoryMock.SetupGetActiveSessionCountByDevice(new Dictionary<EnumDevice, int>());
        _sessionRepositoryMock.SetupGetActiveSessionCountByPlatform(new Dictionary<EnumPlatform, int>());
        _sessionRepositoryMock.SetupGetActiveSessionCountByClient(new Dictionary<EnumClient, int>());
        _sessionRepositoryMock.SetupGetTotalActiveSessionsCount(0);
        _sessionRepositoryMock.SetupGetTotalActiveUsersCount(50);

        // Act
        AdminGetSessionMetricsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalActiveUsers.Should().Be(50);
    }

    [Fact]
    public async Task Handle_WhenNoSessions_ShouldReturnZeroMetrics()
    {
        // Arrange
        AdminGetSessionMetricsQuery query = new();
        _sessionRepositoryMock.SetupGetActiveSessionCountByBrowser(new Dictionary<EnumBrowser, int>());
        _sessionRepositoryMock.SetupGetActiveSessionCountByDevice(new Dictionary<EnumDevice, int>());
        _sessionRepositoryMock.SetupGetActiveSessionCountByPlatform(new Dictionary<EnumPlatform, int>());
        _sessionRepositoryMock.SetupGetActiveSessionCountByClient(new Dictionary<EnumClient, int>());
        _sessionRepositoryMock.SetupGetTotalActiveSessionsCount(0);
        _sessionRepositoryMock.SetupGetTotalActiveUsersCount(0);

        // Act
        AdminGetSessionMetricsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalActiveSessions.Should().Be(0);
        result.TotalActiveUsers.Should().Be(0);
        result.Browsers.Chrome.Should().Be(0);
        result.Devices.Desktop.Should().Be(0);
        result.Platforms.Windows.Should().Be(0);
        result.Clients.WebApp.Should().Be(0);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task Handle_ShouldCallAllRepositoryMethods()
    {
        // Arrange
        AdminGetSessionMetricsQuery query = new();
        SetupDefaultMetrics();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(
            x => x.GetActiveSessionCountByBrowserAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
        _sessionRepositoryMock.Verify(
            x => x.GetActiveSessionCountByDeviceAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
        _sessionRepositoryMock.Verify(
            x => x.GetActiveSessionCountByPlatformAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
        _sessionRepositoryMock.Verify(
            x => x.GetActiveSessionCountByClientAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
        _sessionRepositoryMock.Verify(
            x => x.GetTotalActiveSessionsCountAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
        _sessionRepositoryMock.Verify(x => x.GetTotalActiveUsersCountAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        AdminGetSessionMetricsQuery query = new();
        using CancellationTokenSource cts = new();
        SetupDefaultMetrics();

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _sessionRepositoryMock.Verify(x => x.GetActiveSessionCountByBrowserAsync(cts.Token), Times.Once);
        _sessionRepositoryMock.Verify(x => x.GetTotalActiveSessionsCountAsync(cts.Token), Times.Once);
    }

    #endregion

    #region Helper Methods

    private void SetupDefaultMetrics()
    {
        _sessionRepositoryMock.SetupGetActiveSessionCountByBrowser(new Dictionary<EnumBrowser, int>());
        _sessionRepositoryMock.SetupGetActiveSessionCountByDevice(new Dictionary<EnumDevice, int>());
        _sessionRepositoryMock.SetupGetActiveSessionCountByPlatform(new Dictionary<EnumPlatform, int>());
        _sessionRepositoryMock.SetupGetActiveSessionCountByClient(new Dictionary<EnumClient, int>());
        _sessionRepositoryMock.SetupGetTotalActiveSessionsCount(0);
        _sessionRepositoryMock.SetupGetTotalActiveUsersCount(0);
    }

    private void SetupOtherMetrics()
    {
        _sessionRepositoryMock.SetupGetActiveSessionCountByDevice(new Dictionary<EnumDevice, int>());
        _sessionRepositoryMock.SetupGetActiveSessionCountByPlatform(new Dictionary<EnumPlatform, int>());
        _sessionRepositoryMock.SetupGetActiveSessionCountByClient(new Dictionary<EnumClient, int>());
        _sessionRepositoryMock.SetupGetTotalActiveSessionsCount(0);
        _sessionRepositoryMock.SetupGetTotalActiveUsersCount(0);
    }

    private void SetupOtherMetricsExceptDevice()
    {
        _sessionRepositoryMock.SetupGetActiveSessionCountByBrowser(new Dictionary<EnumBrowser, int>());
        _sessionRepositoryMock.SetupGetActiveSessionCountByPlatform(new Dictionary<EnumPlatform, int>());
        _sessionRepositoryMock.SetupGetActiveSessionCountByClient(new Dictionary<EnumClient, int>());
        _sessionRepositoryMock.SetupGetTotalActiveSessionsCount(0);
        _sessionRepositoryMock.SetupGetTotalActiveUsersCount(0);
    }

    private void SetupOtherMetricsExceptPlatform()
    {
        _sessionRepositoryMock.SetupGetActiveSessionCountByBrowser(new Dictionary<EnumBrowser, int>());
        _sessionRepositoryMock.SetupGetActiveSessionCountByDevice(new Dictionary<EnumDevice, int>());
        _sessionRepositoryMock.SetupGetActiveSessionCountByClient(new Dictionary<EnumClient, int>());
        _sessionRepositoryMock.SetupGetTotalActiveSessionsCount(0);
        _sessionRepositoryMock.SetupGetTotalActiveUsersCount(0);
    }

    private void SetupOtherMetricsExceptClient()
    {
        _sessionRepositoryMock.SetupGetActiveSessionCountByBrowser(new Dictionary<EnumBrowser, int>());
        _sessionRepositoryMock.SetupGetActiveSessionCountByDevice(new Dictionary<EnumDevice, int>());
        _sessionRepositoryMock.SetupGetActiveSessionCountByPlatform(new Dictionary<EnumPlatform, int>());
        _sessionRepositoryMock.SetupGetTotalActiveSessionsCount(0);
        _sessionRepositoryMock.SetupGetTotalActiveUsersCount(0);
    }

    #endregion
}
