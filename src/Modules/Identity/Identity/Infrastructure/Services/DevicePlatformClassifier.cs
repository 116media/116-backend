using _116.Identity.Application.Session.Services;
using _116.Identity.Domain.Constants;
using _116.Identity.Domain.Enums;

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
            return nameof(EnumDevicePlatform.Other).ToLowerInvariant();
        }

        string name = deviceName.ToLowerInvariant();

        var mappings = new Dictionary<IEnumerable<string>, EnumDevicePlatform>
        {
            [key: SessionConstants.DevicePatterns.Mobile] = EnumDevicePlatform.Mobile,
            [key: SessionConstants.DevicePatterns.Tablet] = EnumDevicePlatform.Tablet,
            [key: SessionConstants.DevicePatterns.Desktop] = EnumDevicePlatform.Desktop
        };

        EnumDevicePlatform platform = mappings.FirstOrDefault(m => m.Key.Any(predicate: name.Contains)).Value;

        return (platform == default ? EnumDevicePlatform.Other : platform).ToString().ToLowerInvariant();
    }
}
