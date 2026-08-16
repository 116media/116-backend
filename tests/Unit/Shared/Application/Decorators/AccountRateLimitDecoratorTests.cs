using _116.Shared.Application.Builders.RateLimit;
using _116.Shared.Application.Decorators;
using _116.Shared.Contracts.Application.CQRS;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Decorators;

/// <summary>
/// Unit tests for <see cref="AccountRateLimitDecorator{TRequest,TResponse}"/>. The throttle runs only
/// for requests that opt in via <see cref="IAccountRateLimited"/>; everything else passes straight
/// through to the handler.
/// </summary>
public class AccountRateLimitDecoratorTests
{
    public record TestResponse(string Result);

    public record PlainRequest : IRequest<TestResponse>;

    public record ThrottledRequest(string Policy, string Key) : IRequest<TestResponse>, IAccountRateLimited
    {
        public string RateLimitPolicy => Policy;
        public string AccountKey => Key;
    }

    [Fact]
    public async Task Handle_WhenRequestOptsIn_ThrottlesWithPolicyAndKeyThenCallsHandler()
    {
        // Arrange
        var limiter = new Mock<IAccountRateLimiter>();
        var inner = new Mock<IRequestHandler<ThrottledRequest, TestResponse>>();
        var expected = new TestResponse("ok");
        inner.Setup(h => h.Handle(It.IsAny<ThrottledRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var decorator = new AccountRateLimitDecorator<ThrottledRequest, TestResponse>(inner.Object, limiter.Object);
        var request = new ThrottledRequest("Authentication", "user@x.com");

        // Act
        TestResponse result = await decorator.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
        limiter.Verify(
            l => l.EnsureWithinLimitAsync("Authentication", "user@x.com", It.IsAny<CancellationToken>()),
            Times.Once
        );
        inner.Verify(h => h.Handle(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRequestDoesNotOptIn_SkipsThrottleAndCallsHandler()
    {
        // Arrange
        var limiter = new Mock<IAccountRateLimiter>();
        var inner = new Mock<IRequestHandler<PlainRequest, TestResponse>>();
        var expected = new TestResponse("ok");
        inner.Setup(h => h.Handle(It.IsAny<PlainRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var decorator = new AccountRateLimitDecorator<PlainRequest, TestResponse>(inner.Object, limiter.Object);

        // Act
        TestResponse result = await decorator.Handle(new PlainRequest(), CancellationToken.None);

        // Assert
        result.Should().Be(expected);
        limiter.Verify(
            l => l.EnsureWithinLimitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        inner.Verify(h => h.Handle(It.IsAny<PlainRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
