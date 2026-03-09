using _116.Content.Application.Shared.Errors;
using _116.Shared.Application.Exceptions;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="PricingTierErrors"/>.
/// </summary>
public class PricingTierErrorsTests
{
    [Fact]
    public void AlreadyExists_WithName_ShouldReturnConflictException()
    {
        // Arrange
        string name = "base_upload";

        // Act
        ConflictException exception = PricingTierErrors.AlreadyExists(name);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(name);
    }

    [Fact]
    public void NotFound_WithId_ShouldReturnNotFoundException()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        NotFoundException exception = PricingTierErrors.NotFound(id);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain(id.ToString());
    }

    [Fact]
    public void AlreadyActive_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = PricingTierErrors.AlreadyActive();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadyInactive_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = PricingTierErrors.AlreadyInactive();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void IsInactive_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = PricingTierErrors.IsInactive();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void NameRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = PricingTierErrors.NameRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }
}
