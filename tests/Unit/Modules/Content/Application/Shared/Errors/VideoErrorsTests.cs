using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="VideoErrors"/>.
/// </summary>
public class VideoErrorsTests
{
    private readonly VideoErrors _errors = TestErrorsFactory.CreateVideoErrors();
    private readonly VideoErrorMessage _message = LocalizerFactory.CreateMessage<VideoErrorMessage>();

    [Fact]
    public void NotFound_WithId_ShouldReturnNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        NotFoundException exception = _errors.NotFound(id);

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
        ConflictException exception = _errors.SlugAlreadyExists(slug);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(slug);
    }

    [Fact]
    public void TitleRequired_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.TitleRequired();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.TitleRequired());
    }

    [Fact]
    public void SlugRequired_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.SlugRequired();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.SlugRequired());
    }

    [Fact]
    public void CannotPublishWithoutYoutubeUrl_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.CannotPublishWithoutYoutubeUrl();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.CannotPublishWithoutYoutubeUrl());
    }

    [Fact]
    public void CannotDeletePublishedVideo_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.CannotDeletePublishedVideo();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.CannotDeletePublishedVideo());
    }

    [Fact]
    public void AlreadySubmitted_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.AlreadySubmitted();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.AlreadySubmitted());
    }

    [Fact]
    public void AlreadyPendingReview_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.AlreadyPendingReview();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.AlreadyPendingReview());
    }

    [Fact]
    public void AlreadyApproved_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.AlreadyApproved();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.AlreadyApproved());
    }

    [Fact]
    public void AlreadyPublished_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.AlreadyPublished();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.AlreadyPublished());
    }

    [Fact]
    public void AlreadyRejected_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.AlreadyRejected();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.AlreadyRejected());
    }

    [Fact]
    public void AlreadyArchived_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.AlreadyArchived();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.AlreadyArchived());
    }

    [Fact]
    public void InvalidStatusTransition_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.InvalidStatusTransition("Draft", "Published");

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.InvalidStatusTransition("Draft", "Published"));
    }

    [Fact]
    public void CannotAttachYoutubeUrlBeforeShoot_ShouldReturnBadRequestExceptionWithShootDate()
    {
        // Arrange
        DateTimeOffset shootDate = new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero);

        // Act
        BadRequestException exception = _errors.CannotAttachYoutubeUrlBeforeShoot(shootDate);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.CannotAttachYoutubeUrlBeforeShoot(shootDate));
    }
}
