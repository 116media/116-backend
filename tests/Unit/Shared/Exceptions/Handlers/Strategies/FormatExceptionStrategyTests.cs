using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Handlers.Strategies;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace _116.Unit.Tests.Shared.Exceptions.Handlers.Strategies;

/// <summary>
/// Unit tests for <see cref="FormatExceptionStrategy"/>.
/// The instance and trace extensions are covered for every strategy by
/// <see cref="ExceptionStrategyContractTests" />; the rewritten title, the localized detail and the
/// <see cref="InvalidFormatException" /> type it names are asserted here.
/// </summary>
public class FormatExceptionStrategyTests
{
    private readonly FormatExceptionStrategy _handler = new();
    private readonly SharedExceptionMessage i18n = LocalizerFactory.CreateMessage<SharedExceptionMessage>();

    #region CreateProblemDetails Tests

    [Fact]
    public void CreateProblemDetails_ShouldReturnProblemDetailsWithInvalidFormatTitle()
    {
        // Arrange
        FormatException exception = new("Input string was not in a correct format.");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Title.Should().Be(nameof(InvalidFormatException));
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturnLocalizedDetailMessage()
    {
        // Arrange
        FormatException exception = new("Input string was not in a correct format.");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(i18n.InvalidIdentifier());
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturn400StatusCode()
    {
        // Arrange
        FormatException exception = new("Input string was not in a correct format.");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void CreateProblemDetails_InFrench_ShouldReturnFrenchDetailMessage()
    {
        // Arrange
        string enDetail = i18n.InvalidIdentifier();
        using var scope = new CultureScope("fr");
        FormatException exception = new("Input string was not in a correct format.");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().NotBe(enDetail);
    }

    [Fact]
    public void SharedExceptionMessage_Localizer_InvalidIdentifier_ShouldReturnLocalizedString()
    {
        i18n.Localizer["InvalidIdentifier"].Value.Should().Be(i18n.InvalidIdentifier());
    }

    #endregion

    #region InvalidFormatException Tests

    [Fact]
    public void InvalidFormatException_WithMessageOnly_ShouldSetMessage()
    {
        const string message = "Invalid format error";

        var exception = new InvalidFormatException(message);

        exception.Message.Should().Be(message);
    }

    [Fact]
    public void InvalidFormatException_WithMessageOnly_ShouldHaveNullDetails()
    {
        const string message = "Invalid format error";

        var exception = new InvalidFormatException(message);

        exception.Details.Should().BeNull();
    }

    [Fact]
    public void InvalidFormatException_WithMessageAndDetails_ShouldSetMessage()
    {
        const string message = "Invalid format error";
        const string details = "The value must be a valid UUID.";

        var exception = new InvalidFormatException(message, details);

        exception.Message.Should().Be(message);
    }

    [Fact]
    public void InvalidFormatException_WithMessageAndDetails_ShouldSetDetails()
    {
        const string message = "Invalid format error";
        const string details = "The value must be a valid UUID.";

        var exception = new InvalidFormatException(message, details);

        exception.Details.Should().Be(details);
    }

    [Fact]
    public void InvalidFormatException_ShouldInheritFromException()
    {
        var exception = new InvalidFormatException("error");

        exception.Should().BeAssignableTo<Exception>();
    }

    #endregion
}
