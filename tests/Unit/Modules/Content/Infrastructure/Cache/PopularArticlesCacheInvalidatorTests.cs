using _116.Content.Infrastructure.Cache;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Cache;

/// <summary>
/// Unit tests for <see cref="PopularArticlesCacheInvalidator"/>.
/// Verifies token lifecycle: initial token is live, cancels on Invalidate,
/// and a fresh live token is issued for subsequent cache fills.
/// </summary>
public class PopularArticlesCacheInvalidatorTests
{
    private readonly PopularArticlesCacheInvalidator _invalidator = new();

    #region GetEvictionToken

    [Fact]
    public void GetEvictionToken_BeforeInvalidate_ShouldReturnNonCancelledToken()
    {
        CancellationToken token = _invalidator.GetEvictionToken();

        token.IsCancellationRequested.Should().BeFalse();
    }

    [Fact]
    public void GetEvictionToken_CalledTwiceBeforeInvalidate_ShouldReturnSameToken()
    {
        CancellationToken first = _invalidator.GetEvictionToken();
        CancellationToken second = _invalidator.GetEvictionToken();

        first.Should().Be(second);
    }

    #endregion

    #region Invalidate

    [Fact]
    public void Invalidate_ShouldCancelPreviousToken()
    {
        CancellationToken tokenBefore = _invalidator.GetEvictionToken();

        _invalidator.Invalidate();

        tokenBefore.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public void Invalidate_ShouldIssueNewLiveTokenAfterCancellation()
    {
        _invalidator.Invalidate();

        CancellationToken tokenAfter = _invalidator.GetEvictionToken();

        tokenAfter.IsCancellationRequested.Should().BeFalse();
    }

    [Fact]
    public void Invalidate_CalledTwice_ShouldCancelBothPreviousTokensAndIssueNewLiveToken()
    {
        CancellationToken first = _invalidator.GetEvictionToken();
        _invalidator.Invalidate();

        CancellationToken second = _invalidator.GetEvictionToken();
        _invalidator.Invalidate();

        CancellationToken third = _invalidator.GetEvictionToken();

        first.IsCancellationRequested.Should().BeTrue();
        second.IsCancellationRequested.Should().BeTrue();
        third.IsCancellationRequested.Should().BeFalse();
    }

    [Fact]
    public void Invalidate_NewTokenShouldBeDifferentFromCancelledToken()
    {
        CancellationToken before = _invalidator.GetEvictionToken();
        _invalidator.Invalidate();
        CancellationToken after = _invalidator.GetEvictionToken();

        after.Should().NotBe(before);
    }

    #endregion
}
