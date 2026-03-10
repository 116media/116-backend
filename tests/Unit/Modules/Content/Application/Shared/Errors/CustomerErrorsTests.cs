using _116.Content.Application.Shared.Errors;
using _116.Shared.Application.Exceptions;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="CustomerErrors"/>.
/// </summary>
public class CustomerErrorsTests
{
    [Fact]
    public void AlreadyExists_WithEmail_ShouldReturnConflictException()
    {
        // Arrange
        string email = "customer@example.com";

        // Act
        ConflictException exception = CustomerErrors.AlreadyExists(email);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(email);
    }

    [Fact]
    public void NotFound_WithId_ShouldReturnNotFoundException()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        NotFoundException exception = CustomerErrors.NotFound(id);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain(id.ToString());
    }

    [Fact]
    public void FullNameRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = CustomerErrors.FullNameRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void EmailRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = CustomerErrors.EmailRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }
}
