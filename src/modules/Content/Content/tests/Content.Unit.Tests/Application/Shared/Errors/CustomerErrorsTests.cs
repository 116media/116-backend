using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="CustomerErrors"/>.
/// </summary>
public class CustomerErrorsTests
{
    private readonly CustomerErrors _errors = TestErrorsFactory.CreateCustomerErrors();
    private readonly CustomerErrorMessage _message = LocalizerFactory.CreateMessage<CustomerErrorMessage>();

    [Fact]
    public void AlreadyExists_WithEmail_ShouldReturnConflictException()
    {
        // Arrange
        string email = "customer@example.com";

        // Act
        ConflictException exception = _errors.AlreadyExists(email);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(email);
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
}
