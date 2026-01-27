using _116.Shared.Contracts.Application.CQRS;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Infrastructure;

/// <summary>
/// Provides mock setup helpers for <see cref="IDispatcher"/>.
/// </summary>
public static class MockDispatcher
{
    /// <summary>
    /// Creates a new mock instance of IDispatcher.
    /// </summary>
    /// <returns>A configured Mock of IDispatcher.</returns>
    public static Mock<IDispatcher> Create()
    {
        return new Mock<IDispatcher>();
    }

    /// <summary>
    /// Sets up Send to return the specified response for the given request type.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="mock">The mock instance.</param>
    /// <param name="response">The response to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IDispatcher> SetupSend<TRequest, TResponse>(this Mock<IDispatcher> mock, TResponse response)
        where TRequest : IRequest<TResponse>
    {
        mock.Setup(x => x.Send(It.IsAny<TRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
        return mock;
    }

    /// <summary>
    /// Sets up Send to return the specified response for a specific request instance.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="mock">The mock instance.</param>
    /// <param name="request">The specific request to match.</param>
    /// <param name="response">The response to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IDispatcher> SetupSendForRequest<TRequest, TResponse>(
        this Mock<IDispatcher> mock,
        TRequest request,
        TResponse response
    )
        where TRequest : IRequest<TResponse>
    {
        mock.Setup(x => x.Send(request, It.IsAny<CancellationToken>())).ReturnsAsync(response);
        return mock;
    }

    /// <summary>
    /// Sets up Send to throw an exception for the given request type.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="mock">The mock instance.</param>
    /// <param name="exception">The exception to throw.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IDispatcher> SetupSendThrows<TRequest, TResponse>(
        this Mock<IDispatcher> mock,
        Exception exception
    )
        where TRequest : IRequest<TResponse>
    {
        mock.Setup(x => x.Send(It.IsAny<TRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(exception);
        return mock;
    }

    /// <summary>
    /// Sets up Send (void) to complete successfully for the given request type.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <param name="mock">The mock instance.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IDispatcher> SetupSendVoid<TRequest>(this Mock<IDispatcher> mock)
        where TRequest : IRequest
    {
        mock.Setup(x => x.Send(It.IsAny<TRequest>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        return mock;
    }

    /// <summary>
    /// Sets up Send (void) to throw an exception for the given request type.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <param name="mock">The mock instance.</param>
    /// <param name="exception">The exception to throw.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IDispatcher> SetupSendVoidThrows<TRequest>(this Mock<IDispatcher> mock, Exception exception)
        where TRequest : IRequest
    {
        mock.Setup(x => x.Send(It.IsAny<TRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(exception);
        return mock;
    }

    /// <summary>
    /// Verifies that Send was called with the specified request type.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="mock">The mock instance.</param>
    /// <param name="verifyRequest">Optional predicate to verify the request.</param>
    public static void VerifySendCalled<TRequest, TResponse>(
        this Mock<IDispatcher> mock,
        Func<TRequest, bool>? verifyRequest = null
    )
        where TRequest : IRequest<TResponse>
    {
        if (verifyRequest is not null)
        {
            mock.Verify(x => x.Send(It.Is<TRequest>(r => verifyRequest(r)), It.IsAny<CancellationToken>()), Times.Once);
        }
        else
        {
            mock.Verify(x => x.Send(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    /// <summary>
    /// Verifies that Send (void) was called with the specified request type.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <param name="mock">The mock instance.</param>
    /// <param name="verifyRequest">Optional predicate to verify the request.</param>
    public static void VerifySendVoidCalled<TRequest>(
        this Mock<IDispatcher> mock,
        Func<TRequest, bool>? verifyRequest = null
    )
        where TRequest : IRequest
    {
        if (verifyRequest is not null)
        {
            mock.Verify(x => x.Send(It.Is<TRequest>(r => verifyRequest(r)), It.IsAny<CancellationToken>()), Times.Once);
        }
        else
        {
            mock.Verify(x => x.Send(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    /// <summary>
    /// Verifies that Send was never called with the specified request type.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="mock">The mock instance.</param>
    public static void VerifySendNotCalled<TRequest, TResponse>(this Mock<IDispatcher> mock)
        where TRequest : IRequest<TResponse>
    {
        mock.Verify(x => x.Send(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
