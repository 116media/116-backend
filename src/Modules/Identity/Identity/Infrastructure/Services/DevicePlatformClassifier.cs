using _116.Identity.Application.Session.Services;
using _116.Identity.Domain.Constants;

namespace _116.Identity.Infrastructure.Services;

/// <summary>
/// Service for classifying device platforms based on device names.
/// Uses pattern matching against known device identifiers.
/// </summary>
public class DevicePlatformClassifier : IDevicePlatformClassifier
{
    /// <inheritdoc />
    public string ClassifyPlatform(string deviceName)
    {
        if (string.IsNullOrWhiteSpace(value: deviceName))
        {
            return SessionConstants.Platform.Other;
        }

        string lowerDeviceName = deviceName.ToLower();

        if (SessionConstants.DevicePatterns.Mobile.Any(pattern => lowerDeviceName.Contains(pattern)))
        {
            return SessionConstants.Platform.Mobile;
        }

        if (SessionConstants.DevicePatterns.Tablet.Any(pattern => lowerDeviceName.Contains(pattern)))
        {
            return SessionConstants.Platform.Tablet;
        }

        if (SessionConstants.DevicePatterns.Desktop.Any(pattern => lowerDeviceName.Contains(pattern)))
        {
            return SessionConstants.Platform.Desktop;
        }

        return SessionConstants.Platform.Other;
    }
}
