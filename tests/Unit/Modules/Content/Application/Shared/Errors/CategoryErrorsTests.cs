using _116.Content.Application.Shared.Errors;
using _116.Shared.Application.Exceptions;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="CategoryErrors"/>.
/// </summary>
public class CategoryErrorsTests
{
    [Fact]
    public void AlreadyExists_WithSlug_ShouldReturnConflictException()
    {
        // Arrange
        string slug = "artist-profile";

        // Act
        ConflictException exception = CategoryErrors.AlreadyExists(slug);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(slug);
    }

    [Fact]
    public void NotFound_WithId_ShouldReturnNotFoundException()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        NotFoundException exception = CategoryErrors.NotFound(id);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain(id.ToString());
    }

    [Fact]
    public void AlreadyActive_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = CategoryErrors.AlreadyActive();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadyInactive_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = CategoryErrors.AlreadyInactive();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void NameRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = CategoryErrors.NameRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SlugRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = CategoryErrors.SlugRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void PricingAlreadyExists_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = CategoryErrors.PricingAlreadyExists();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void PricingNotFound_WithIds_ShouldReturnNotFoundException()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        Guid tierId = Guid.NewGuid();

        // Act
        NotFoundException exception = CategoryErrors.PricingNotFound(categoryId, tierId);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void PriceMustBeNonNegative_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = CategoryErrors.PriceMustBeNonNegative();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }
}
