namespace _116.Identity.Application.Session.Services;

/// <summary>
/// Interface for classifying device platforms based on device names.
/// </summary>
public interface IDevicePlatformClassifier
{
    /// <summary>
    /// Determines the device platform category from a device name.
    /// </summary>
    /// <param name="deviceName">The device name to classify.</param>
    /// <returns>The platform category (mobile, desktop, tablet, or other).</returns>
    string ClassifyPlatform(string deviceName);
}
