using _116.Content.Application.Shared.Errors;
using _116.Shared.Application.Exceptions;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="PackageErrors"/>.
/// </summary>
public class PackageErrorsTests
{
    [Fact]
    public void NotFound_WithId_ShouldReturnNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        NotFoundException exception = PackageErrors.NotFound(id);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain(id.ToString());
    }

    [Fact]
    public void AlreadyActive_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = PackageErrors.AlreadyActive();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadyInactive_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = PackageErrors.AlreadyInactive();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void NameRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = PackageErrors.NameRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void PriceMustBeNonNegative_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = PackageErrors.PriceMustBeNonNegative();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SlotQuantityMustBePositive_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = PackageErrors.SlotQuantityMustBePositive();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SlotNotFound_WithSlotId_ShouldReturnNotFoundException()
    {
        // Arrange
        var slotId = Guid.NewGuid();

        // Act
        NotFoundException exception = PackageErrors.SlotNotFound(slotId);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain(slotId.ToString());
    }
}
