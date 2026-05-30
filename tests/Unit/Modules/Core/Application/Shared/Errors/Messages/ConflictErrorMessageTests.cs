using _116.Core.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Application.Shared.Errors.Messages;

/// <summary>
/// Unit tests for <see cref="ConflictErrorMessage"/>.
/// </summary>
public class ConflictErrorMessageTests
{
    private readonly ConflictErrorMessage _message = LocalizerFactory.CreateMessage<ConflictErrorMessage>("en");

    [Fact]
    public void Localizer_ShouldNotBeNull()
    {
        _message.Localizer.Should().NotBeNull();
    }

    [Fact]
    public void FileUploadFailed_WithFileNameAndReason_ShouldReturnFormattedMessage()
    {
        // Arrange
        string fileName = "avatar.jpg";
        string reason = "storage quota exceeded";

        // Act
        string message = _message.FileUploadFailed(fileName, reason);

        // Assert
        message.Should().Be($"Failed to upload file '{fileName}': {reason}.");
    }
}
