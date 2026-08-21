using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="PackageErrors"/>.
/// </summary>
public class PackageErrorsTests
{
    private readonly PackageErrors _errors = TestErrorsFactory.CreatePackageErrors();
    private readonly PackageErrorMessage _message = LocalizerFactory.CreateMessage<PackageErrorMessage>();

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
        ConflictException exception = _errors.AlreadyActive();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.AlreadyActive());
    }

    [Fact]
    public void AlreadyInactive_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.AlreadyInactive();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.AlreadyInactive());
    }

    [Fact]
    public void PriceMustBeNonNegative_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.PriceMustBeNonNegative();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.PriceMustBeNonNegative());
    }

    [Fact]
    public void SlotNotFound_WithSlotId_ShouldReturnNotFoundException()
    {
        // Arrange
        var slotId = Guid.NewGuid();

        // Act
        NotFoundException exception = _errors.SlotNotFound(slotId);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain(slotId.ToString());
    }
}
