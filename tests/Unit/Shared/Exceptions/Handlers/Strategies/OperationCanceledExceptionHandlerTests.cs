using _116.Shared.Application.Exceptions.Handlers.Strategies;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace _116.Unit.Tests.Shared.Exceptions.Handlers.Strategies;

/// <summary>
/// Unit tests for <see cref="OperationCanceledExceptionHandler"/>.
/// The title, instance and trace extensions are covered for every strategy by
/// <see cref="ExceptionStrategyContractTests" />; the 499 (client closed request) status and the
/// localized cancelled detail are asserted here.
/// </summary>
public class OperationCanceledExceptionHandlerTests
{
    private const int StatusClientClosedRequest = 499;

    private readonly OperationCanceledExceptionHandler _handler = new();
    private readonly SharedExceptionMessage i18n = LocalizerFactory.CreateMessage<SharedExceptionMessage>();

    [Fact]
    public void CreateProblemDetails_ShouldReturn499StatusCode()
    {
        // Arrange
        OperationCanceledException exception = new("The request was cancelled");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Status.Should().Be(StatusClientClosedRequest);
    }

    [Fact]
    public void CreateProblemDetails_ShouldUseTheLocalizedCancelledDetail()
    {
        // Arrange
        OperationCanceledException exception = new("The request was cancelled");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(i18n.RequestCancelled());
    }
}
