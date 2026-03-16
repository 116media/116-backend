using _116.Content.Application.Shared.Errors;
using _116.Shared.Application.Exceptions;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="VideoErrors"/>.
/// </summary>
public class VideoErrorsTests
{
    [Fact]
    public void NotFound_WithId_ShouldReturnNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        NotFoundException exception = VideoErrors.NotFound(id);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain(id.ToString());
    }

    [Fact]
    public void SlugAlreadyExists_WithSlug_ShouldReturnConflictException()
    {
        // Arrange
        const string slug = "116-le-focus-fally-ipupa";

        // Act
        ConflictException exception = VideoErrors.SlugAlreadyExists(slug);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(slug);
    }

    [Fact]
    public void TitleRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = VideoErrors.TitleRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SlugRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = VideoErrors.SlugRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CannotPublishWithoutYoutubeId_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = VideoErrors.CannotPublishWithoutYoutubeId();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CannotDeletePublishedVideo_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = VideoErrors.CannotDeletePublishedVideo();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadySubmitted_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = VideoErrors.AlreadySubmitted();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadyPendingReview_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = VideoErrors.AlreadyPendingReview();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadyApproved_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = VideoErrors.AlreadyApproved();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadyPublished_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = VideoErrors.AlreadyPublished();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadyRejected_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = VideoErrors.AlreadyRejected();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadyArchived_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = VideoErrors.AlreadyArchived();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void InvalidStatusTransition_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = VideoErrors.InvalidStatusTransition("Draft", "Published");

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }
}
