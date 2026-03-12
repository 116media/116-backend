using _116.Content.Application.Shared.Errors;
using _116.Shared.Application.Exceptions;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="ArticleErrors"/>.
/// </summary>
public class ArticleErrorsTests
{
    [Fact]
    public void NotFound_WithId_ShouldReturnNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        NotFoundException exception = ArticleErrors.NotFound(id);

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
        ConflictException exception = ArticleErrors.SlugAlreadyExists(slug);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(slug);
    }

    [Fact]
    public void TitleRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = ArticleErrors.TitleRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SlugRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = ArticleErrors.SlugRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadySubmitted_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = ArticleErrors.AlreadySubmitted();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadyPendingReview_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = ArticleErrors.AlreadyPendingReview();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadyApproved_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = ArticleErrors.AlreadyApproved();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadyPublished_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = ArticleErrors.AlreadyPublished();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadyRejected_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = ArticleErrors.AlreadyRejected();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadyArchived_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = ArticleErrors.AlreadyArchived();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void InvalidStatusTransition_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = ArticleErrors.InvalidStatusTransition("Draft", "Published");

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CannotDeletePublishedArticle_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = ArticleErrors.CannotDeletePublishedArticle();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }
}
