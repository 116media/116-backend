using _116.Core.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Application.Shared.Errors.Messages;

/// <summary>
/// Unit tests for <see cref="InternalServerErrorMessage"/>.
/// </summary>
public class InternalServerErrorMessageTests
{
    private readonly InternalServerErrorMessage _message = LocalizerFactory.CreateMessage<InternalServerErrorMessage>(
        "en"
    );

    [Fact]
    public void Localizer_ShouldNotBeNull()
    {
        _message.Localizer.Should().NotBeNull();
    }

    [Fact]
    public void ServiceUnavailable_WithServiceName_ShouldReturnFormattedMessage()
    {
        // Arrange
        string serviceName = "Cloudinary";

        // Act
        string message = _message.ServiceUnavailable(serviceName);

        // Assert
        message.Should().Be($"Service '{serviceName}' is currently unavailable.");
    }

    [Fact]
    public void DatabaseConnectionFailed_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.DatabaseConnectionFailed();

        // Assert
        message.Should().Be("Unable to connect to the database.");
    }

    [Fact]
    public void FileDownloadFailed_WithUrlAndReason_ShouldReturnFormattedMessage()
    {
        // Arrange
        string fileUrl = "https://cdn.example.com/file.mp4";
        string reason = "connection timeout";

        // Act
        string message = _message.FileDownloadFailed(fileUrl, reason);

        // Assert
        message.Should().Be($"Failed to download file from '{fileUrl}': {reason}.");
    }

    [Fact]
    public void FileStorageFailed_WithReason_ShouldReturnFormattedMessage()
    {
        // Arrange
        string reason = "disk full";

        // Act
        string message = _message.FileStorageFailed(reason);

        // Assert
        message.Should().Be($"Failed to store file: {reason}.");
    }
}
