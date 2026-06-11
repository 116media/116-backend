using _116.Content.Application.Shared.Errors;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="CategoryErrors"/>.
/// </summary>
public class CategoryErrorsTests
{
    private readonly CategoryErrors _errors = TestErrorsFactory.CreateCategoryErrors();

    [Fact]
    public void AlreadyExists_WithSlug_ShouldReturnConflictException()
    {
        // Arrange
        string slug = "artist-profile";

        // Act
        ConflictException exception = _errors.AlreadyExists(slug);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(slug);
    }

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
    public void AlreadyActive_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = _errors.AlreadyActive();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadyInactive_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = _errors.AlreadyInactive();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void NameRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _errors.NameRequired();

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
    public void PricingAlreadyExists_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = _errors.PricingAlreadyExists();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void PricingNotFound_WithIds_ShouldReturnNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var tierId = Guid.NewGuid();

        // Act
        NotFoundException exception = _errors.PricingNotFound(categoryId, tierId);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void PriceMustBeNonNegative_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _errors.PriceMustBeNonNegative();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }
}
