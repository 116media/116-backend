using _116.Content.Application.Shared.Errors;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="ArticleErrors"/>.
/// </summary>
public class ArticleErrorsTests
{
    private readonly ArticleErrors _errors = TestErrorsFactory.CreateArticleErrors();

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
        const string slug = "fally-ipupa-profile";

        // Act
        ConflictException exception = _errors.SlugAlreadyExists(slug);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(slug);
    }

    [Fact]
    public void TitleRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _errors.TitleRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SlugRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _errors.SlugRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadySubmitted_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = _errors.AlreadySubmitted();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadyPendingReview_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = _errors.AlreadyPendingReview();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadyApproved_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = _errors.AlreadyApproved();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadyPublished_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = _errors.AlreadyPublished();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadyRejected_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = _errors.AlreadyRejected();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadyArchived_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = _errors.AlreadyArchived();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void InvalidStatusTransition_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _errors.InvalidStatusTransition("Draft", "Published");

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CannotDeletePublishedArticle_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _errors.CannotDeletePublishedArticle();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }
}
